using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public sealed class ViewCameraTouchController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlacementController placementController;
    [SerializeField] private Transform viewTarget;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineFollow cinemachineFollow;

    [Header("Pan")]
    [SerializeField] private float panSpeed = 0.02f;
    [SerializeField] private float panSmoothTime = 0.08f;
    [SerializeField] private float panDeadZone = 2f;
    [SerializeField] private bool invertPan = true;

    [Header("Rotation")]
    [SerializeField] private bool enableTwoFingerRotation = true;
    [SerializeField] private float rotationSpeed = 0.8f;
    [SerializeField] private float rotationSmoothTime = 0.08f;
    [SerializeField] private float rotationDeadZone = 3f;
    [SerializeField] private float zoomSuppressesRotationRatio = 1.35f;
    [SerializeField] private bool invertRotation;

    [Header("Zoom")]
    [SerializeField] private float pinchZoomSpeed = 0.015f;
    [SerializeField] private float zoomSmoothTime = 0.08f;
    [SerializeField] private float zoomDeadZone = 2f;
    [SerializeField] private float minZoom = 4f;
    [SerializeField] private float maxZoom = 12f;
    [SerializeField] private Vector3 fallbackFollowOffset = new Vector3(0f, 6f, -8f);

    [Header("Bounds")]
    [SerializeField] private Vector3 mapBoundsCenter = Vector3.zero;
    [SerializeField] private Vector2 mapBoundsSize = new Vector2(10f, 5f);
    [SerializeField] private float boundsSoftness = 1f;

    [Header("Editor")]
    [SerializeField] private bool enableMouseInEditor = true;

    private readonly List<RaycastResult> uiResults = new List<RaycastResult>();
    private Vector3 desiredTargetPosition;
    private Vector3 panVelocity;
    private float followVerticalRatio = 0.6f;
    private float followHorizontalRatio = 0.8f;
    private float currentYawDegrees;
    private float targetYawDegrees;
    private float yawVelocity;
    private float currentZoom;
    private float targetZoom;
    private float zoomVelocity;
    private bool isMousePanning;

    private bool CanControlView => placementController == null
        || (!placementController.IsPlacementMode && !placementController.IsDraggingPlacement);

    private void Awake()
    {
        ResolveReferences();
        ConfigureCinemachine();
        InitializeState();
    }

    private void OnValidate()
    {
        panSpeed = Mathf.Max(0f, panSpeed);
        panSmoothTime = Mathf.Max(0f, panSmoothTime);
        panDeadZone = Mathf.Max(0f, panDeadZone);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
        rotationSmoothTime = Mathf.Max(0f, rotationSmoothTime);
        rotationDeadZone = Mathf.Max(0f, rotationDeadZone);
        zoomSuppressesRotationRatio = Mathf.Max(0f, zoomSuppressesRotationRatio);
        pinchZoomSpeed = Mathf.Max(0f, pinchZoomSpeed);
        zoomSmoothTime = Mathf.Max(0f, zoomSmoothTime);
        zoomDeadZone = Mathf.Max(0f, zoomDeadZone);
        minZoom = Mathf.Max(0.01f, minZoom);
        maxZoom = Mathf.Max(minZoom, maxZoom);
        mapBoundsSize.x = Mathf.Max(0f, mapBoundsSize.x);
        mapBoundsSize.y = Mathf.Max(0f, mapBoundsSize.y);
        boundsSoftness = Mathf.Max(0f, boundsSoftness);
    }

    private void Update()
    {
        if (viewTarget == null)
        {
            return;
        }

        if (!CanControlView)
        {
            isMousePanning = false;
            desiredTargetPosition = viewTarget.position;
            return;
        }

        HandleTouchInput();
        HandleMouseInput();
        ApplySmoothing();
    }

    private void ResolveReferences()
    {
        if (placementController == null)
        {
            placementController = FindAnyObjectByType<PlacementController>();
        }

        if (viewTarget == null)
        {
            viewTarget = transform;
        }

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        if (cinemachineCamera == null)
        {
            cinemachineCamera = FindAnyObjectByType<CinemachineCamera>();
        }

        if (cinemachineFollow == null && cinemachineCamera != null)
        {
            cinemachineFollow = cinemachineCamera.GetComponent<CinemachineFollow>();
        }
    }

    private void ConfigureCinemachine()
    {
        if (worldCamera != null && worldCamera.GetComponent<CinemachineBrain>() == null)
        {
            worldCamera.gameObject.AddComponent<CinemachineBrain>();
        }

        if (cinemachineCamera == null || viewTarget == null)
        {
            return;
        }

        cinemachineCamera.Follow = viewTarget;
        cinemachineCamera.LookAt = viewTarget;

        if (cinemachineFollow == null)
        {
            cinemachineFollow = cinemachineCamera.GetComponent<CinemachineFollow>();
        }

        if (cinemachineFollow == null)
        {
            cinemachineFollow = cinemachineCamera.gameObject.AddComponent<CinemachineFollow>();
        }

        if (cinemachineCamera.GetComponent<CinemachineHardLookAt>() == null)
        {
            cinemachineCamera.gameObject.AddComponent<CinemachineHardLookAt>();
        }
    }

    private void InitializeState()
    {
        if (viewTarget != null)
        {
            desiredTargetPosition = ClampToBounds(viewTarget.position);
            viewTarget.position = desiredTargetPosition;
        }

        Vector3 initialOffset = fallbackFollowOffset;
        if (cinemachineFollow != null && cinemachineFollow.FollowOffset.sqrMagnitude > 0.001f)
        {
            initialOffset = cinemachineFollow.FollowOffset;
        }
        else if (worldCamera != null && viewTarget != null)
        {
            Vector3 cameraOffset = worldCamera.transform.position - viewTarget.position;
            if (cameraOffset.sqrMagnitude > 0.001f)
            {
                initialOffset = cameraOffset;
            }
        }

        if (initialOffset.sqrMagnitude <= 0.001f)
        {
            initialOffset = new Vector3(0f, 6f, -8f);
        }

        SetFollowDirection(initialOffset.normalized);
        currentZoom = Mathf.Clamp(initialOffset.magnitude, minZoom, maxZoom);
        targetZoom = currentZoom;
        ApplyZoom(currentZoom);
    }

    private void HandleTouchInput()
    {
#if ENABLE_INPUT_SYSTEM
        var touchscreen = Touchscreen.current;
        if (touchscreen == null)
        {
            return;
        }

        int touchCount = 0;
        TouchSample first = default;
        TouchSample second = default;
        for (int i = 0; i < touchscreen.touches.Count; i++)
        {
            var touch = touchscreen.touches[i];
            if (!touch.press.isPressed)
            {
                continue;
            }

            var sample = new TouchSample(
                touch.position.ReadValue(),
                touch.delta.ReadValue(),
                touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began);
            if (touchCount == 0)
            {
                first = sample;
            }
            else if (touchCount == 1)
            {
                second = sample;
            }

            touchCount++;
            if (touchCount >= 2)
            {
                break;
            }
        }

        if (touchCount == 1)
        {
            if (first.Began || IsScreenPositionOverUi(first.Position))
            {
                return;
            }

            PanByScreenDelta(first.Delta);
            return;
        }

        if (touchCount >= 2)
        {
            if (IsScreenPositionOverUi(first.Position) || IsScreenPositionOverUi(second.Position))
            {
                return;
            }

            HandleTwoFingerGesture(first, second);
        }
#endif
    }

    private void HandleMouseInput()
    {
        if (!enableMouseInEditor)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        Vector2 mousePosition = mouse.position.ReadValue();
        if (mouse.leftButton.wasPressedThisFrame)
        {
            isMousePanning = !IsScreenPositionOverUi(mousePosition);
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            isMousePanning = false;
        }

        if (isMousePanning && mouse.leftButton.isPressed)
        {
            PanByScreenDelta(mouse.delta.ReadValue());
        }

        float scrollY = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scrollY) > 0.01f && !IsScreenPositionOverUi(mousePosition))
        {
            ZoomByPinchDelta(scrollY);
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        Vector2 mousePosition = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
            isMousePanning = !IsScreenPositionOverUi(mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isMousePanning = false;
        }

        if (isMousePanning && Input.GetMouseButton(0))
        {
            PanByScreenDelta(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
        }

        float scrollY = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scrollY) > 0.01f && !IsScreenPositionOverUi(mousePosition))
        {
            ZoomByPinchDelta(scrollY * 120f);
        }
#endif
    }

    private void PanByScreenDelta(Vector2 screenDelta)
    {
        if (screenDelta.sqrMagnitude < panDeadZone * panDeadZone || worldCamera == null)
        {
            return;
        }

        Vector3 right = Vector3.ProjectOnPlane(worldCamera.transform.right, Vector3.up);
        if (right.sqrMagnitude <= 0.001f)
        {
            right = Vector3.right;
        }
        else
        {
            right.Normalize();
        }

        Vector3 forward = Vector3.ProjectOnPlane(worldCamera.transform.forward, Vector3.up);
        if (forward.sqrMagnitude <= 0.001f)
        {
            forward = Vector3.forward;
        }
        else
        {
            forward.Normalize();
        }

        float direction = invertPan ? -1f : 1f;
        Vector3 movement = (right * screenDelta.x + forward * screenDelta.y) * panSpeed * direction;
        desiredTargetPosition = ApplyBoundsResistance(desiredTargetPosition, desiredTargetPosition + movement);
    }

    private void ZoomByPinchDelta(float pinchDelta)
    {
        if (Mathf.Abs(pinchDelta) < zoomDeadZone)
        {
            return;
        }

        targetZoom = Mathf.Clamp(targetZoom - pinchDelta * pinchZoomSpeed, minZoom, maxZoom);
    }

    private void HandleTwoFingerGesture(TouchSample first, TouchSample second)
    {
        if (first.Began || second.Began)
        {
            return;
        }

        Vector2 currentVector = second.Position - first.Position;
        Vector2 previousVector = (second.Position - second.Delta) - (first.Position - first.Delta);
        if (currentVector.sqrMagnitude <= 0.001f || previousVector.sqrMagnitude <= 0.001f)
        {
            return;
        }

        float currentDistance = currentVector.magnitude;
        float previousDistance = previousVector.magnitude;
        float pinchDelta = currentDistance - previousDistance;
        float rotationDelta = Vector2.SignedAngle(previousVector, currentVector);

        ZoomByPinchDelta(pinchDelta);
        RotateByGestureDelta(rotationDelta, pinchDelta);
    }

    private void RotateByGestureDelta(float rotationDelta, float pinchDelta)
    {
        if (!enableTwoFingerRotation || Mathf.Abs(rotationDelta) < rotationDeadZone)
        {
            return;
        }

        float rotationScore = Mathf.Abs(rotationDelta) / Mathf.Max(rotationDeadZone, 0.001f);
        float zoomScore = Mathf.Abs(pinchDelta) / Mathf.Max(zoomDeadZone, 0.001f);
        if (zoomScore > rotationScore * zoomSuppressesRotationRatio)
        {
            return;
        }

        float direction = invertRotation ? -1f : 1f;
        targetYawDegrees += rotationDelta * rotationSpeed * direction;
    }

    private void ApplySmoothing()
    {
        if (panSmoothTime <= 0f)
        {
            viewTarget.position = desiredTargetPosition;
        }
        else
        {
            viewTarget.position = Vector3.SmoothDamp(
                viewTarget.position,
                desiredTargetPosition,
                ref panVelocity,
                panSmoothTime);
        }

        if (zoomSmoothTime <= 0f)
        {
            currentZoom = targetZoom;
        }
        else
        {
            currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, zoomSmoothTime);
        }

        if (rotationSmoothTime <= 0f)
        {
            currentYawDegrees = targetYawDegrees;
        }
        else
        {
            currentYawDegrees = Mathf.SmoothDampAngle(
                currentYawDegrees,
                targetYawDegrees,
                ref yawVelocity,
                rotationSmoothTime);
        }

        ApplyZoom(currentZoom);
    }

    private void ApplyZoom(float zoom)
    {
        if (cinemachineFollow != null)
        {
            cinemachineFollow.FollowOffset = GetFollowDirection(currentYawDegrees) * zoom;
        }
    }

    private void SetFollowDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = new Vector3(0f, 0.6f, -0.8f);
        }

        direction.Normalize();
        followVerticalRatio = Mathf.Clamp(direction.y, -0.95f, 0.95f);
        followHorizontalRatio = Mathf.Sqrt(Mathf.Max(0.001f, 1f - followVerticalRatio * followVerticalRatio));
        currentYawDegrees = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        targetYawDegrees = currentYawDegrees;
    }

    private Vector3 GetFollowDirection(float yawDegrees)
    {
        float yawRadians = yawDegrees * Mathf.Deg2Rad;
        return new Vector3(
            Mathf.Sin(yawRadians) * followHorizontalRatio,
            followVerticalRatio,
            Mathf.Cos(yawRadians) * followHorizontalRatio).normalized;
    }

    private Vector3 ApplyBoundsResistance(Vector3 current, Vector3 proposed)
    {
        if (mapBoundsSize.x <= 0f || mapBoundsSize.y <= 0f)
        {
            return proposed;
        }

        float halfX = mapBoundsSize.x * 0.5f;
        float halfZ = mapBoundsSize.y * 0.5f;
        float minX = mapBoundsCenter.x - halfX;
        float maxX = mapBoundsCenter.x + halfX;
        float minZ = mapBoundsCenter.z - halfZ;
        float maxZ = mapBoundsCenter.z + halfZ;

        proposed.x = ApplyAxisBoundsResistance(current.x, proposed.x, minX, maxX);
        proposed.z = ApplyAxisBoundsResistance(current.z, proposed.z, minZ, maxZ);
        proposed.y = mapBoundsCenter.y;
        return proposed;
    }

    private float ApplyAxisBoundsResistance(float current, float proposed, float min, float max)
    {
        if (min >= max)
        {
            return current;
        }

        float delta = proposed - current;
        if (Mathf.Approximately(delta, 0f))
        {
            return Mathf.Clamp(proposed, min, max);
        }

        if (boundsSoftness > 0f)
        {
            if (delta < 0f)
            {
                float distanceToMin = current - min;
                if (distanceToMin < boundsSoftness)
                {
                    proposed = current + delta * Mathf.Clamp01(distanceToMin / boundsSoftness);
                }
            }
            else
            {
                float distanceToMax = max - current;
                if (distanceToMax < boundsSoftness)
                {
                    proposed = current + delta * Mathf.Clamp01(distanceToMax / boundsSoftness);
                }
            }
        }

        return Mathf.Clamp(proposed, min, max);
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        if (mapBoundsSize.x <= 0f || mapBoundsSize.y <= 0f)
        {
            return position;
        }

        float halfX = mapBoundsSize.x * 0.5f;
        float halfZ = mapBoundsSize.y * 0.5f;
        position.x = Mathf.Clamp(position.x, mapBoundsCenter.x - halfX, mapBoundsCenter.x + halfX);
        position.y = mapBoundsCenter.y;
        position.z = Mathf.Clamp(position.z, mapBoundsCenter.z - halfZ, mapBoundsCenter.z + halfZ);
        return position;
    }

    private bool IsScreenPositionOverUi(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        uiResults.Clear();
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };
        EventSystem.current.RaycastAll(eventData, uiResults);

        for (int i = 0; i < uiResults.Count; i++)
        {
            GameObject hitObject = uiResults[i].gameObject;
            if (hitObject.GetComponentInParent<Selectable>() != null
                || hitObject.GetComponentInParent<SlotSelectionButton>() != null)
            {
                return true;
            }
        }

        return false;
    }

    private readonly struct TouchSample
    {
        public TouchSample(Vector2 position, Vector2 delta, bool began)
        {
            Position = position;
            Delta = delta;
            Began = began;
        }

        public Vector2 Position { get; }
        public Vector2 Delta { get; }
        public bool Began { get; }
    }
}
