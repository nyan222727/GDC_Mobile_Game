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
    private sealed class PreviewPlacement
    {
        public PreviewPlacement(MapTile tile, GameObject character)
        {
            Tile = tile;
            Character = character;
        }

        public MapTile Tile { get; }
        public GameObject Character { get; }
    }

    private sealed class PlacedCharacter
    {
        public PlacedCharacter(MapTile tile, Transform character, SlotSelectionButton sourceSlot)
        {
            Tile = tile;
            Character = character;
            SourceSlot = sourceSlot;
        }

        public MapTile Tile { get; }
        public Transform Character { get; }
        public SlotSelectionButton SourceSlot { get; }
    }

    [SerializeField] private Camera worldCamera;
    [SerializeField] private GraphicRaycaster uiRaycaster;
    [SerializeField] private Button viewModeButton;
    [SerializeField] private Button undoButton;
    [SerializeField] private RectTransform trashDropZone;
    [SerializeField] private Graphic trashGraphic;
    [SerializeField] private LayerMask placementMask = ~0;
    [SerializeField] private Color selectedOutlineColor = new Color32(0, 170, 255, 255);
    [SerializeField] private Vector2 selectedOutlineDistance = new Vector2(4f, -4f);
    [SerializeField] private Color previewColor = new Color32(0, 170, 255, 120);
    [SerializeField] private Color placedColor = new Color32(40, 145, 255, 255);
    [SerializeField] private Color tilePreviewColor = new Color32(110, 220, 255, 255);
    [SerializeField] private Color tileInvalidColor = new Color32(255, 80, 80, 255);
    [SerializeField] private Color trashNormalColor = new Color32(70, 70, 70, 220);
    [SerializeField] private Color trashHoverColor = new Color32(255, 80, 80, 240);
    [SerializeField] private Vector3 characterOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private Vector3 characterScale = new Vector3(0.45f, 0.8f, 0.45f);

    private readonly List<RaycastResult> uiResults = new List<RaycastResult>();
    private readonly Dictionary<MapTile, PreviewPlacement> previewsByTile = new Dictionary<MapTile, PreviewPlacement>();
    private readonly List<PreviewPlacement> previewOrder = new List<PreviewPlacement>();
    private readonly Stack<List<PlacedCharacter>> placedBatches = new Stack<List<PlacedCharacter>>();
    private SlotSelectionButton selectedSlot;
    private bool isDraggingPlacement;
    private bool placementEnabled = true;
    private Material previewMaterial;
    private Material placedMaterial;

    public bool IsPlacementEnabled => placementEnabled;
    public bool IsPlacementMode => placementEnabled && selectedSlot != null;
    public bool IsDraggingPlacement => isDraggingPlacement;

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
        CreatePlacementMaterials();
        RegisterUiActions();
        RefreshModeUi(false);
    }

    private void OnDestroy()
    {
        UnregisterUiActions();
    }

    private void Update()
    {
        if (!placementEnabled)
        {
            return;
        }

        if (TryGetCancelPressed())
        {
            CancelOrExitPlacementMode();
            return;
        }

        if (!TryGetPointerState(out PointerState pointer))
        {
            return;
        }

        if (pointer.Down)
        {
            if (IsPointerOverAnyUi(pointer.ScreenPosition))
            {
                return;
            }

            if (selectedSlot != null)
            {
                BeginPlacementDrag();
            }
        }

        if (!isDraggingPlacement)
        {
            return;
        }

        bool pointerOverTrash = IsPointerOverTrash(pointer.ScreenPosition);
        SetTrashHover(pointerOverTrash);

        if (pointer.Held && !pointerOverTrash && !IsPointerOverAnyUi(pointer.ScreenPosition))
        {
            TryAddPreviewAt(pointer.ScreenPosition);
        }

        if (pointer.Up)
        {
            if (pointerOverTrash)
            {
                CancelPlacementDrag();
            }
            else
            {
                ConfirmPlacementDrag();
            }
        }
    }

    public void SelectSlot(SlotSelectionButton slot)
    {
        if (!placementEnabled)
        {
            return;
        }

        if (isDraggingPlacement)
        {
            CancelPlacementDrag();
        }

        if (selectedSlot == slot)
        {
            ClearSelectedSlot();
            return;
        }

        if (slot != null && !slot.HasAvailableCount)
        {
            slot.FlashUnavailable();
            RefreshModeUi(false);
            return;
        }

        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(false, selectedOutlineColor, selectedOutlineDistance);
        }

        selectedSlot = slot;

        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(true, selectedOutlineColor, selectedOutlineDistance);
        }

        RefreshModeUi(false);
    }

    public void TryPlaceOnTile(MapTile tile)
    {
        if (!placementEnabled || selectedSlot == null || tile == null)
        {
            return;
        }

        if (!tile.CanPlace)
        {
            tile.FlashInvalid(tileInvalidColor);
            return;
        }

        if (!selectedSlot.TryReservePreview())
        {
            tile.FlashInvalid(tileInvalidColor);
            return;
        }

        SlotSelectionButton sourceSlot = selectedSlot;
        GameObject character = CreateCharacterObject(
            "Prototype Character",
            tile.transform,
            sourceSlot.CharacterPrefab,
            placedMaterial,
            false);
        tile.SetOccupant(character.transform);
        sourceSlot.CommitPreviewReservations(1);
        placedBatches.Push(new List<PlacedCharacter> { new PlacedCharacter(tile, character.transform, sourceSlot) });
        if (!sourceSlot.HasAvailableCount)
        {
            ClearSelectedSlot();
            return;
        }

        RefreshModeUi(false);
    }

    public void EnterViewMode()
    {
        CancelPlacementDrag();
        ClearSelectedSlot();
    }

    public void UndoLastPlacementBatch()
    {
        if (!placementEnabled)
        {
            return;
        }

        if (placedBatches.Count == 0)
        {
            return;
        }

        List<PlacedCharacter> batch = placedBatches.Pop();
        for (int i = 0; i < batch.Count; i++)
        {
            PlacedCharacter placed = batch[i];
            if (placed.Tile != null)
            {
                placed.Tile.ClearOccupant(placed.Character);
            }

            if (placed.Character != null)
            {
                Destroy(placed.Character.gameObject);
            }

            if (placed.SourceSlot != null)
            {
                placed.SourceSlot.RestoreCount(1);
            }
        }

        RefreshModeUi(false);
    }

    public void SetPlacementEnabled(bool enabled)
    {
        if (placementEnabled == enabled)
        {
            RefreshModeUi(false);
            return;
        }

        placementEnabled = enabled;
        if (!placementEnabled)
        {
            CancelPlacementDrag();
            ClearSelectedSlot();
        }

        RefreshModeUi(false);
    }

    private void BeginPlacementDrag()
    {
        isDraggingPlacement = true;
        RefreshModeUi(true);
    }

    private void CancelPlacementDrag()
    {
        if (selectedSlot != null)
        {
            selectedSlot.ReleasePreviewReservations();
        }

        ClearPreviews(true);
        isDraggingPlacement = false;
        RefreshModeUi(false);
    }

    private void ConfirmPlacementDrag()
    {
        var batch = new List<PlacedCharacter>();
        SlotSelectionButton sourceSlot = selectedSlot;
        for (int i = 0; i < previewOrder.Count; i++)
        {
            PreviewPlacement preview = previewOrder[i];
            if (preview.Tile == null || preview.Character == null || preview.Tile.IsOccupied)
            {
                if (preview.Tile != null)
                {
                    preview.Tile.SetPlacementHighlight(false, tilePreviewColor);
                }

                if (preview.Character != null)
                {
                    Destroy(preview.Character);
                }

                continue;
            }

            GameObject character = CreateCharacterObject(
                "Prototype Character",
                preview.Tile.transform,
                sourceSlot != null ? sourceSlot.CharacterPrefab : null,
                placedMaterial,
                false);

            Destroy(preview.Character);
            preview.Tile.SetOccupant(character.transform);
            preview.Tile.SetPlacementHighlight(false, tilePreviewColor);
            batch.Add(new PlacedCharacter(preview.Tile, character.transform, sourceSlot));
        }

        previewsByTile.Clear();
        previewOrder.Clear();
        isDraggingPlacement = false;

        if (sourceSlot != null)
        {
            sourceSlot.CommitPreviewReservations(batch.Count);
        }

        if (batch.Count > 0)
        {
            placedBatches.Push(batch);
        }

        if (sourceSlot != null && !sourceSlot.HasAvailableCount)
        {
            ClearSelectedSlot();
            return;
        }

        RefreshModeUi(false);
    }

    private void TryAddPreviewAt(Vector2 screenPosition)
    {
        MapTile tile = GetTileAt(screenPosition);
        if (tile == null || previewsByTile.ContainsKey(tile))
        {
            return;
        }

        if (!tile.CanPlace)
        {
            tile.FlashInvalid(tileInvalidColor);
            return;
        }

        if (selectedSlot == null || !selectedSlot.TryReservePreview())
        {
            tile.FlashInvalid(tileInvalidColor);
            return;
        }

        GameObject preview = CreateCharacterObject(
            "Prototype Character Preview",
            tile.transform,
            selectedSlot.CharacterPrefab,
            previewMaterial,
            true);
        var placement = new PreviewPlacement(tile, preview);
        previewsByTile.Add(tile, placement);
        previewOrder.Add(placement);
        tile.SetPlacementHighlight(true, tilePreviewColor);
    }

    private MapTile GetTileAt(Vector2 screenPosition)
    {
        if (worldCamera == null)
        {
            return null;
        }

        var ray = worldCamera.ScreenPointToRay(screenPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, placementMask, QueryTriggerInteraction.Ignore))
        {
            return null;
        }

        return hit.collider.GetComponentInParent<MapTile>();
    }

    private GameObject CreateCharacterObject(
        string objectName,
        Transform parent,
        GameObject prefab,
        Material material,
        bool overridePrefabMaterial)
    {
        bool usesPrefab = prefab != null;
        GameObject character = usesPrefab ? Instantiate(prefab, parent) : GameObject.CreatePrimitive(PrimitiveType.Capsule);
        character.name = objectName;
        character.transform.SetParent(parent, false);
        character.transform.localPosition = characterOffset;
        character.transform.localScale = characterScale;

        if (material != null && (!usesPrefab || overridePrefabMaterial))
        {
            ApplyCharacterMaterial(character, material);
        }

        return character;
    }

    private void ClearPreviews(bool destroyObjects)
    {
        for (int i = 0; i < previewOrder.Count; i++)
        {
            PreviewPlacement preview = previewOrder[i];
            if (preview.Tile != null)
            {
                preview.Tile.SetPlacementHighlight(false, tilePreviewColor);
            }

            if (destroyObjects && preview.Character != null)
            {
                Destroy(preview.Character);
            }
        }

        previewsByTile.Clear();
        previewOrder.Clear();
    }

    private void ClearSelectedSlot()
    {
        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(false, selectedOutlineColor, selectedOutlineDistance);
        }

        selectedSlot = null;
        RefreshModeUi(false);
    }

    private void CancelOrExitPlacementMode()
    {
        if (isDraggingPlacement)
        {
            CancelPlacementDrag();
            return;
        }

        ClearSelectedSlot();
    }

    private bool IsPointerOverAnyUi(Vector2 screenPosition)
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
            if (slot != null)
            {
                return true;
            }

            if (uiResults[i].gameObject.GetComponentInParent<Button>() != null)
            {
                return true;
            }

            if (trashDropZone != null && uiResults[i].gameObject.transform.IsChildOf(trashDropZone))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPointerOverTrash(Vector2 screenPosition)
    {
        if (trashDropZone == null || !trashDropZone.gameObject.activeInHierarchy)
        {
            return false;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(trashDropZone, screenPosition, GetUiCamera());
    }

    private Camera GetUiCamera()
    {
        if (uiRaycaster == null)
        {
            return null;
        }

        var canvas = uiRaycaster.GetComponent<Canvas>();
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return uiRaycaster.eventCamera;
    }

    private bool TryGetPointerState(out PointerState pointer)
    {
        pointer = default;

#if ENABLE_INPUT_SYSTEM
        if (Pointer.current != null)
        {
            pointer = new PointerState(
                Pointer.current.position.ReadValue(),
                Pointer.current.press.wasPressedThisFrame,
                Pointer.current.press.isPressed,
                Pointer.current.press.wasReleasedThisFrame);
            return pointer.Down || pointer.Held || pointer.Up;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        pointer = new PointerState(
            Input.mousePosition,
            Input.GetMouseButtonDown(0),
            Input.GetMouseButton(0),
            Input.GetMouseButtonUp(0));
        return pointer.Down || pointer.Held || pointer.Up;
#else
        return false;
#endif
    }

    private bool TryGetCancelPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            return true;
        }
#endif

        return false;
    }

    private void RegisterUiActions()
    {
        if (viewModeButton != null)
        {
            viewModeButton.onClick.AddListener(EnterViewMode);
        }

        if (undoButton != null)
        {
            undoButton.onClick.AddListener(UndoLastPlacementBatch);
        }
    }

    private void UnregisterUiActions()
    {
        if (viewModeButton != null)
        {
            viewModeButton.onClick.RemoveListener(EnterViewMode);
        }

        if (undoButton != null)
        {
            undoButton.onClick.RemoveListener(UndoLastPlacementBatch);
        }
    }

    private void RefreshModeUi(bool showTrash)
    {
        if (viewModeButton != null)
        {
            viewModeButton.gameObject.SetActive(placementEnabled && selectedSlot != null);
        }

        if (undoButton != null)
        {
            bool canUndo = placementEnabled && placedBatches.Count > 0;
            undoButton.gameObject.SetActive(canUndo);
            undoButton.interactable = canUndo;
        }

        if (trashDropZone != null)
        {
            trashDropZone.gameObject.SetActive(placementEnabled && showTrash);
        }

        SetTrashHover(false);
    }

    private void SetTrashHover(bool hovered)
    {
        if (trashGraphic == null)
        {
            return;
        }

        trashGraphic.color = hovered ? trashHoverColor : trashNormalColor;
    }

    private void CreatePlacementMaterials()
    {
        previewMaterial = CreateCharacterMaterial(previewColor, true);
        placedMaterial = CreateCharacterMaterial(placedColor, false);
    }

    private static Material CreateCharacterMaterial(Color color, bool transparent)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        var material = new Material(shader);
        ApplyColor(material, color);

        if (transparent)
        {
            ConfigureTransparentMaterial(material);
        }

        return material;
    }

    private static void ApplyCharacterMaterial(GameObject character, Material material)
    {
        if (material == null)
        {
            return;
        }

        var characterRenderers = character.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < characterRenderers.Length; i++)
        {
            characterRenderers[i].sharedMaterial = material;
        }
    }

    private static void ApplyColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    private static void ConfigureTransparentMaterial(Material material)
    {
        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
        }

        if (material.HasProperty("_SrcBlend"))
        {
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        }

        if (material.HasProperty("_DstBlend"))
        {
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        }

        if (material.HasProperty("_ZWrite"))
        {
            material.SetFloat("_ZWrite", 0f);
        }

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
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

    private readonly struct PointerState
    {
        public PointerState(Vector2 screenPosition, bool down, bool held, bool up)
        {
            ScreenPosition = screenPosition;
            Down = down;
            Held = held;
            Up = up;
        }

        public Vector2 ScreenPosition { get; }
        public bool Down { get; }
        public bool Held { get; }
        public bool Up { get; }
    }
}
