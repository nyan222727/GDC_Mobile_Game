using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

public sealed class PlacementController : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;
    [SerializeField] private GraphicRaycaster uiRaycaster;
    [SerializeField] private LayerMask placementMask = ~0;
    [SerializeField] private Color selectedOutlineColor = new Color32(0, 170, 255, 255);
    [SerializeField] private Vector2 selectedOutlineDistance = new Vector2(4f, -4f);
    [SerializeField] private Vector3 characterOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private Vector3 characterScale = new Vector3(0.45f, 0.8f, 0.45f);

    private readonly List<RaycastResult> uiResults = new List<RaycastResult>();
    private SlotSelectionButton selectedSlot;

    private void Awake()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        if (uiRaycaster == null)
        {
            uiRaycaster = FindAnyObjectByType<GraphicRaycaster>();
        }

        ConfigureEventSystemInputModule();
    }

    private void Update()
    {
        if (!TryGetPointerDown(out Vector2 screenPosition))
        {
            return;
        }

        if (TrySelectSlot(screenPosition))
        {
            return;
        }

        TryPlaceOnTile(screenPosition);
    }

    public void SelectSlot(SlotSelectionButton slot)
    {
        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(false, selectedOutlineColor, selectedOutlineDistance);
        }

        selectedSlot = slot;

        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(true, selectedOutlineColor, selectedOutlineDistance);
        }
    }

    public void TryPlaceOnTile(MapTile tile)
    {
        if (selectedSlot == null || tile == null || tile.IsOccupied)
        {
            return;
        }

        var character = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        character.name = "Prototype Character";
        character.transform.position = tile.transform.position + characterOffset;
        character.transform.localScale = characterScale;
        character.transform.SetParent(tile.transform, true);

        var characterCollider = character.GetComponent<Collider>();
        if (characterCollider != null)
        {
            Destroy(characterCollider);
        }

        tile.SetOccupant(character.transform);
    }

    private bool TrySelectSlot(Vector2 screenPosition)
    {
        if (uiRaycaster == null || EventSystem.current == null)
        {
            return false;
        }

        uiResults.Clear();
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };
        uiRaycaster.Raycast(eventData, uiResults);

        for (int i = 0; i < uiResults.Count; i++)
        {
            var slot = uiResults[i].gameObject.GetComponentInParent<SlotSelectionButton>();
            if (slot == null)
            {
                continue;
            }

            SelectSlot(slot);
            return true;
        }

        return false;
    }

    private void TryPlaceOnTile(Vector2 screenPosition)
    {
        if (selectedSlot == null || worldCamera == null)
        {
            return;
        }

        var ray = worldCamera.ScreenPointToRay(screenPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, placementMask, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        var tile = hit.collider.GetComponentInParent<MapTile>();
        TryPlaceOnTile(tile);
    }

    private bool TryGetPointerDown(out Vector2 screenPosition)
    {
#if ENABLE_INPUT_SYSTEM
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            screenPosition = Pointer.current.position.ReadValue();
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetMouseButtonDown(0))
        {
            screenPosition = Input.mousePosition;
            return true;
        }
#endif

        screenPosition = default;
        return false;
    }

    private void ConfigureEventSystemInputModule()
    {
#if ENABLE_INPUT_SYSTEM
        var eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            eventSystem = FindAnyObjectByType<EventSystem>();
        }

        if (eventSystem == null)
        {
            return;
        }

        var legacyInputModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (legacyInputModule != null)
        {
            legacyInputModule.enabled = false;
            Destroy(legacyInputModule);
        }

        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
#endif
    }
}
