using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TileElementRotationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelFlowController levelFlowController;
    [SerializeField] private TileElementGrid grid;
    [SerializeField] private TileElementSelectionView selectionView;
    [SerializeField] private GameObject combatHud;
    [SerializeField] private Button moveUpButton;
    [SerializeField] private Button moveDownButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private Button rotateButton;

    [Header("Behavior")]
    [SerializeField] private Vector2Int initialSelectionOrigin = Vector2Int.zero;
    [SerializeField] private bool resetElementTimersAfterRotation = true;
    [SerializeField] private bool syncTileVisualsAfterRotation = true;
    [SerializeField] private float gridRetryInterval = 0.25f;

    private readonly Dictionary<TileProperty, Material> defaultMaterialByTile = new Dictionary<TileProperty, Material>();
    private Vector2Int selectionOrigin;
    private bool gridReady;
    private bool wasInCombat;
    private Coroutine gridRetryRoutine;
    private Material fallbackDefaultMaterial;

    public Vector2Int SelectionOrigin => selectionOrigin;
    public bool IsReady => gridReady;

    private void Awake()
    {
        ResolveReferences();
        RegisterUiActions();
        selectionOrigin = initialSelectionOrigin;
        SetInteractionVisible(false);
    }

    private void Start()
    {
        TryPrepareGrid();
        RefreshState(true);
    }

    private void Update()
    {
        RefreshState(false);
    }

    private void OnDestroy()
    {
        UnregisterUiActions();
    }

    public void MoveUp()
    {
        TryMove(Vector2Int.up);
    }

    public void MoveDown()
    {
        TryMove(Vector2Int.down);
    }

    public void MoveLeft()
    {
        TryMove(Vector2Int.left);
    }

    public void MoveRight()
    {
        TryMove(Vector2Int.right);
    }

    public void RotateClockwise()
    {
        if (!CanInteract() || !grid.TryGetSelection(selectionOrigin,
                out TileProperty bottomLeft, out TileProperty bottomRight,
                out TileProperty topLeft, out TileProperty topRight))
        {
            selectionView?.FlashInvalid();
            return;
        }

        RotateSelectionElementsClockwise(topLeft, topRight, bottomLeft, bottomRight);

        if (resetElementTimersAfterRotation)
        {
            ResetElementTimer(topLeft);
            ResetElementTimer(topRight);
            ResetElementTimer(bottomRight);
            ResetElementTimer(bottomLeft);
        }

        if (syncTileVisualsAfterRotation)
        {
            SyncTileVisual(topLeft);
            SyncTileVisual(topRight);
            SyncTileVisual(bottomRight);
            SyncTileVisual(bottomLeft);
        }

        selectionView?.FlashRotated();
    }

    private void TryMove(Vector2Int direction)
    {
        if (!CanInteract())
        {
            return;
        }

        Vector2Int nextOrigin = selectionOrigin + direction;
        if (!grid.IsValidSelectionOrigin(nextOrigin))
        {
            selectionView?.FlashInvalid();
            return;
        }

        selectionOrigin = nextOrigin;
        selectionView?.ShowSelection(grid, selectionOrigin);
    }

    private void RefreshState(bool force)
    {
        bool isInCombat = levelFlowController != null && levelFlowController.IsInCombatState;
        if (!force && isInCombat == wasInCombat)
        {
            return;
        }

        wasInCombat = isInCombat;
        if (isInCombat)
        {
            if (!TryPrepareGrid() && gridRetryRoutine == null)
            {
                gridRetryRoutine = StartCoroutine(RetryGridRoutine());
            }
        }

        SetInteractionVisible(isInCombat && gridReady);
    }

    private bool TryPrepareGrid()
    {
        if (grid == null || !grid.Rebuild())
        {
            gridReady = false;
            return false;
        }

        gridReady = true;
        CacheDefaultMaterials();
        selectionOrigin = grid.ClampSelectionOrigin(selectionOrigin);
        if (!grid.IsValidSelectionOrigin(selectionOrigin))
        {
            selectionOrigin = Vector2Int.zero;
        }

        if (levelFlowController != null && levelFlowController.IsInCombatState)
        {
            selectionView?.ShowSelection(grid, selectionOrigin);
        }

        return true;
    }

    private IEnumerator RetryGridRoutine()
    {
        while (levelFlowController != null && levelFlowController.IsInCombatState && !gridReady)
        {
            if (TryPrepareGrid())
            {
                SetInteractionVisible(true);
                break;
            }

            yield return new WaitForSeconds(gridRetryInterval);
        }

        gridRetryRoutine = null;
    }

    private bool CanInteract()
    {
        return gridReady && levelFlowController != null && levelFlowController.IsInCombatState;
    }

    private void SetInteractionVisible(bool visible)
    {
        if (combatHud != null && combatHud.activeSelf != visible)
        {
            combatHud.SetActive(visible);
        }

        if (selectionView != null)
        {
            if (visible)
            {
                selectionView.ShowSelection(grid, selectionOrigin);
            }
            else
            {
                selectionView.SetVisible(false);
            }
        }
    }

    private void ResolveReferences()
    {
        if (levelFlowController == null)
        {
            levelFlowController = FindAnyObjectByType<LevelFlowController>();
        }

        if (grid == null)
        {
            grid = GetComponent<TileElementGrid>();
        }

        if (selectionView == null)
        {
            selectionView = GetComponentInChildren<TileElementSelectionView>(true);
        }
    }

    private void RegisterUiActions()
    {
        moveUpButton?.onClick.AddListener(MoveUp);
        moveDownButton?.onClick.AddListener(MoveDown);
        moveLeftButton?.onClick.AddListener(MoveLeft);
        moveRightButton?.onClick.AddListener(MoveRight);
        rotateButton?.onClick.AddListener(RotateClockwise);
    }

    private void UnregisterUiActions()
    {
        moveUpButton?.onClick.RemoveListener(MoveUp);
        moveDownButton?.onClick.RemoveListener(MoveDown);
        moveLeftButton?.onClick.RemoveListener(MoveLeft);
        moveRightButton?.onClick.RemoveListener(MoveRight);
        rotateButton?.onClick.RemoveListener(RotateClockwise);
    }

    private void CacheDefaultMaterials()
    {
        if (grid == null)
        {
            return;
        }

        foreach (TileProperty tile in grid.Tiles)
        {
            if (tile == null || !tile.TryGetComponent(out Renderer tileRenderer))
            {
                continue;
            }

            Material defaultMaterial = tile.defaultMaterial;
            if (defaultMaterial == null)
            {
                Material currentMaterial = tileRenderer.sharedMaterial;
                if (!IsElementMaterial(tile, currentMaterial))
                {
                    defaultMaterial = currentMaterial;
                }
            }

            if (defaultMaterial == null)
            {
                defaultMaterial = fallbackDefaultMaterial;
            }

            if (defaultMaterial == null)
            {
                continue;
            }

            fallbackDefaultMaterial = defaultMaterial;
            defaultMaterialByTile[tile] = defaultMaterial;

            if (tile.defaultMaterial == null)
            {
                tile.defaultMaterial = defaultMaterial;
            }
        }
    }

    private static void RotateSelectionElementsClockwise(
        TileProperty topLeft,
        TileProperty topRight,
        TileProperty bottomLeft,
        TileProperty bottomRight)
    {
        Element[] before =
        {
            topLeft.element,
            topRight.element,
            bottomLeft.element,
            bottomRight.element
        };

        topRight.element = before[0];
        bottomRight.element = before[1];
        bottomLeft.element = before[3];
        topLeft.element = before[2];
    }

    private void SyncTileVisual(TileProperty tile)
    {
        if (tile == null || !tile.TryGetComponent(out Renderer tileRenderer))
        {
            return;
        }

        Material targetMaterial = ResolveMaterial(tile);
        if (targetMaterial == null)
        {
            return;
        }

        tileRenderer.sharedMaterial = targetMaterial;
    }

    private Material ResolveMaterial(TileProperty tile)
    {
        switch (tile.element)
        {
            case Element.Fire:
                return tile.fireMaterial;
            case Element.Ice:
                return tile.iceMaterial;
            case Element.None:
                return ResolveDefaultMaterial(tile);
            default:
                return null;
        }
    }

    private Material ResolveDefaultMaterial(TileProperty tile)
    {
        if (tile.defaultMaterial != null)
        {
            return tile.defaultMaterial;
        }

        if (!defaultMaterialByTile.TryGetValue(tile, out Material defaultMaterial))
        {
            defaultMaterial = fallbackDefaultMaterial;
        }

        if (defaultMaterial != null)
        {
            tile.defaultMaterial = defaultMaterial;
        }

        return defaultMaterial;
    }

    private static void ResetElementTimer(TileProperty tile)
    {
        if (tile != null && tile.element != Element.None)
        {
            tile.ResetElementTimer();
        }
    }

    private static bool IsElementMaterial(TileProperty tile, Material material)
    {
        return MaterialMatches(material, tile.fireMaterial) ||
               MaterialMatches(material, tile.iceMaterial);
    }

    private static bool MaterialMatches(Material a, Material b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        return a == b || a.name == b.name || a.name == $"{b.name} (Instance)";
    }
}
