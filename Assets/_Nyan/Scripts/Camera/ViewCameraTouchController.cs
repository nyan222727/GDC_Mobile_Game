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

    [Header("Pitch")]
    [SerializeField] private bool enableTwoFingerPitch = true;
    [SerializeField] private float pitchSpeed = 0.1f;
    [SerializeField] private float pitchSmoothTime = 0.08f;
    [SerializeField] private float pitchDeadZone = 4f;
    [SerializeField] private float pitchLockRatio = 1.25f;
    [SerializeField] private float pitchVerticalBias = 1.2f;
    [SerializeField] private float minPitchAngle = 30f;
    [SerializeField] private float maxPitchAngle = 65f;
    [SerializeField] private bool invertPitch;

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
    private float currentYawDegrees;
    private float targetYawDegrees;
    private float yawVelocity;
    private float currentPitchDegrees;
    private float targetPitchDegrees;
    private float pitchVelocity;
    private float currentZoom;
    private float targetZoom;
    private float zoomVelocity;
    private TwoFingerGestureMode activeTwoFingerGestureMode;
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
        pitchSpeed = Mathf.Max(0f, pitchSpeed);
        pitchSmoothTime = Mathf.Max(0f, pitchSmoothTime);
        pitchDeadZone = Mathf.Max(0f, pitchDeadZone);
        pitchLockRatio = Mathf.Max(0f, pitchLockRatio);
        pitchVerticalBias = Mathf.Max(0f, pitchVerticalBias);
        minPitchAngle = Mathf.Clamp(minPitchAngle, -85f, 85f);
        maxPitchAngle = Mathf.Clamp(maxPitchAngle, minPitchAngle, 85f);
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
            activeTwoFingerGestureMode = TwoFingerGestureMode.None;
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
            activeTwoFingerGestureMode = TwoFingerGestureMode.None;

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
            return;
        }

        activeTwoFingerGestureMode = TwoFingerGestureMode.None;
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
            activeTwoFingerGestureMode = TwoFingerGestureMode.None;
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

        if (activeTwoFingerGestureMode == TwoFingerGestureMode.Pitch)
        {
            PitchByGestureDelta(GetPitchGestureDelta(first, second));
            return;
        }

        if (ShouldLockPitchGesture(first, second, pinchDelta, rotationDelta))
        {
            activeTwoFingerGestureMode = TwoFingerGestureMode.Pitch;
            PitchByGestureDelta(GetPitchGestureDelta(first, second));
            return;
        }

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

    private bool ShouldLockPitchGesture(
        TouchSample first,
        TouchSample second,
        float pinchDelta,
        float rotationDelta)
    {
        float pitchDelta = GetPitchGestureDelta(first, second);
        if (!enableTwoFingerPitch || Mathf.Abs(pitchDelta) < pitchDeadZone)
        {
            return false;
        }

        if (!AreTouchesMovingTogether(first.Delta, second.Delta))
        {
            return false;
        }

        Vector2 averageDelta = (first.Delta + second.Delta) * 0.5f;
        if (Mathf.Abs(averageDelta.y) < Mathf.Abs(averageDelta.x) * pitchVerticalBias)
        {
            return false;
        }

        float pitchScore = Mathf.Abs(pitchDelta) / Mathf.Max(pitchDeadZone, 0.001f);
        float zoomScore = Mathf.Abs(pinchDelta) / Mathf.Max(zoomDeadZone, 0.001f);
        float rotationScore = enableTwoFingerRotation
            ? Mathf.Abs(rotationDelta) / Mathf.Max(rotationDeadZone, 0.001f)
            : 0f;

        return pitchScore > Mathf.Max(zoomScore, rotationScore) * pitchLockRatio;
    }

    private void PitchByGestureDelta(float pitchDelta)
    {
        if (!enableTwoFingerPitch || Mathf.Abs(pitchDelta) < pitchDeadZone)
        {
            return;
        }

        float direction = invertPitch ? -1f : 1f;
        targetPitchDegrees = Mathf.Clamp(
            targetPitchDegrees + pitchDelta * pitchSpeed * direction,
            minPitchAngle,
            maxPitchAngle);
    }

    private static float GetPitchGestureDelta(TouchSample first, TouchSample second)
    {
        return ((first.Delta + second.Delta) * 0.5f).y;
    }

    private static bool AreTouchesMovingTogether(Vector2 firstDelta, Vector2 secondDelta)
    {
        if (firstDelta.sqrMagnitude <= 0.001f || secondDelta.sqrMagnitude <= 0.001f)
        {
            return false;
        }

        return Vector2.Dot(firstDelta.normalized, secondDelta.normalized) > 0.7f;
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

        if (pitchSmoothTime <= 0f)
        {
            currentPitchDegrees = targetPitchDegrees;
        }
        else
        {
            currentPitchDegrees = Mathf.SmoothDamp(
                currentPitchDegrees,
                targetPitchDegrees,
                ref pitchVelocity,
                pitchSmoothTime);
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
        var horizontalDirection = new Vector3(direction.x, 0f, direction.z);
        if (horizontalDirection.sqrMagnitude <= 0.001f)
        {
            horizontalDirection = Vector3.back;
        }
        else
        {
            horizontalDirection.Normalize();
        }

        currentPitchDegrees = Mathf.Clamp(
            Mathf.Asin(Mathf.Clamp(direction.y, -0.95f, 0.95f)) * Mathf.Rad2Deg,
            minPitchAngle,
            maxPitchAngle);
        targetPitchDegrees = currentPitchDegrees;
        currentYawDegrees = Mathf.Atan2(horizontalDirection.x, horizontalDirection.z) * Mathf.Rad2Deg;
        targetYawDegrees = currentYawDegrees;
    }

    private Vector3 GetFollowDirection(float yawDegrees)
    {
        float yawRadians = yawDegrees * Mathf.Deg2Rad;
        float pitchRadians = currentPitchDegrees * Mathf.Deg2Rad;
        float horizontalRatio = Mathf.Cos(pitchRadians);
        return new Vector3(
            Mathf.Sin(yawRadians) * horizontalRatio,
            Mathf.Sin(pitchRadians),
            Mathf.Cos(yawRadians) * horizontalRatio).normalized;
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

    private enum TwoFingerGestureMode
    {
        None,
        Pitch
    }
}
