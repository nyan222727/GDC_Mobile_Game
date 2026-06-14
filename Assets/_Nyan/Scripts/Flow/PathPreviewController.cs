using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PathPreviewController : MonoBehaviour
{
    [SerializeField] private LevelFlowController levelFlowController;
    [SerializeField] private GameObject pathMarkerPrefab;
    [SerializeField] private Color pathColor = new Color32(255, 205, 80, 255);
    [SerializeField] private float refreshInterval = 0.5f;

    private readonly List<MapTile> pathTiles = new List<MapTile>();
    private LevelFlowController.LevelState lastState;
    private float nextRefreshTime;
    private bool wasVisible;
    private bool warnedMissingMarkerPrefab;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        RebuildPathTiles();
        RefreshVisibility(true);
    }

    private void OnDisable()
    {
        SetPathVisible(false);
    }

    private void Update()
    {
        ResolveReferences();

        bool forceRefresh = false;
        if (Time.time >= nextRefreshTime)
        {
            nextRefreshTime = Time.time + Mathf.Max(0.05f, refreshInterval);
            RebuildPathTiles();
            forceRefresh = true;
        }

        RefreshVisibility(forceRefresh);
    }

    private void ResolveReferences()
    {
        if (levelFlowController == null)
        {
            levelFlowController = FindAnyObjectByType<LevelFlowController>();
        }
    }

    private void RebuildPathTiles()
    {
        for (int i = 0; i < pathTiles.Count; i++)
        {
            if (pathTiles[i] != null)
            {
                pathTiles[i].SetPathHighlight(false, pathColor, pathMarkerPrefab);
            }
        }

        pathTiles.Clear();
        MapTile[] tiles = FindObjectsByType<MapTile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < tiles.Length; i++)
        {
            MapTile tile = tiles[i];
            if (tile != null && tile.BlocksPlacement)
            {
                pathTiles.Add(tile);
            }
        }
    }

    private void RefreshVisibility(bool force)
    {
        LevelFlowController.LevelState state = levelFlowController != null
            ? levelFlowController.CurrentState
            : LevelFlowController.LevelState.Placement;

        bool shouldShow = state == LevelFlowController.LevelState.Placement;
        if (!force && state == lastState && shouldShow == wasVisible)
        {
            return;
        }

        lastState = state;
        wasVisible = shouldShow;
        SetPathVisible(shouldShow);
    }

    private void SetPathVisible(bool visible)
    {
        if (visible && pathMarkerPrefab == null)
        {
            if (!warnedMissingMarkerPrefab)
            {
                Debug.LogWarning(
                    $"{nameof(PathPreviewController)} needs a Path Marker Prefab to show enemy path preview.",
                    this);
                warnedMissingMarkerPrefab = true;
            }

            return;
        }

        for (int i = 0; i < pathTiles.Count; i++)
        {
            if (pathTiles[i] != null)
            {
                pathTiles[i].SetPathHighlight(visible, pathColor, pathMarkerPrefab);
            }
        }
    }
}
