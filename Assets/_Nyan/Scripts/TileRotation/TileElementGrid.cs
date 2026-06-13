using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TileElementGrid : MonoBehaviour
{
    [SerializeField] private float coordinateTolerance = 0.05f;
    [SerializeField] private float fallbackCellSize = 1f;

    private readonly Dictionary<Vector2Int, TileProperty> tiles = new Dictionary<Vector2Int, TileProperty>();
    private readonly List<float> xCoordinates = new List<float>();
    private readonly List<float> zCoordinates = new List<float>();

    public bool IsReady { get; private set; }
    public int Width => xCoordinates.Count;
    public int Height => zCoordinates.Count;
    public int TileCount => tiles.Count;

    public bool Rebuild()
    {
        tiles.Clear();
        xCoordinates.Clear();
        zCoordinates.Clear();
        IsReady = false;

        TileProperty[] sceneTiles = FindObjectsByType<TileProperty>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        for (int i = 0; i < sceneTiles.Length; i++)
        {
            TileProperty tile = sceneTiles[i];
            if (tile == null)
            {
                continue;
            }

            AddCoordinate(xCoordinates, tile.transform.position.x);
            AddCoordinate(zCoordinates, tile.transform.position.z);
        }

        xCoordinates.Sort();
        zCoordinates.Sort();

        for (int i = 0; i < sceneTiles.Length; i++)
        {
            TileProperty tile = sceneTiles[i];
            if (tile == null)
            {
                continue;
            }

            int x = FindCoordinateIndex(xCoordinates, tile.transform.position.x);
            int y = FindCoordinateIndex(zCoordinates, tile.transform.position.z);
            if (x < 0 || y < 0)
            {
                continue;
            }

            var coordinate = new Vector2Int(x, y);
            if (!tiles.TryAdd(coordinate, tile))
            {
                Debug.LogWarning($"Multiple tiles were mapped to grid coordinate {coordinate}.", tile);
            }
        }

        IsReady = Width >= 2 && Height >= 2 && tiles.Count >= 4;
        return IsReady;
    }

    public bool IsValidSelectionOrigin(Vector2Int origin)
    {
        return IsReady &&
               origin.x >= 0 && origin.y >= 0 &&
               origin.x + 1 < Width && origin.y + 1 < Height &&
               TryGetSelection(origin, out _, out _, out _, out _);
    }

    public Vector2Int ClampSelectionOrigin(Vector2Int origin)
    {
        int maxX = Mathf.Max(0, Width - 2);
        int maxY = Mathf.Max(0, Height - 2);
        return new Vector2Int(
            Mathf.Clamp(origin.x, 0, maxX),
            Mathf.Clamp(origin.y, 0, maxY));
    }

    public bool TryGetSelection(
        Vector2Int origin,
        out TileProperty bottomLeft,
        out TileProperty bottomRight,
        out TileProperty topLeft,
        out TileProperty topRight)
    {
        bool foundBottomLeft = tiles.TryGetValue(origin, out bottomLeft);
        bool foundBottomRight = tiles.TryGetValue(origin + Vector2Int.right, out bottomRight);
        bool foundTopLeft = tiles.TryGetValue(origin + Vector2Int.up, out topLeft);
        bool foundTopRight = tiles.TryGetValue(origin + Vector2Int.one, out topRight);
        return foundBottomLeft && foundBottomRight && foundTopLeft && foundTopRight;
    }

    public bool TryGetSelectionFrame(
        Vector2Int origin,
        out Vector3 center,
        out Vector2 size)
    {
        center = Vector3.zero;
        size = Vector2.zero;

        if (!TryGetSelection(origin, out TileProperty bottomLeft, out TileProperty bottomRight,
                out TileProperty topLeft, out TileProperty topRight))
        {
            return false;
        }

        center = (bottomLeft.transform.position + bottomRight.transform.position +
                  topLeft.transform.position + topRight.transform.position) * 0.25f;

        float cellWidth = GetCellSpacing(xCoordinates);
        float cellDepth = GetCellSpacing(zCoordinates);
        size = new Vector2(cellWidth * 2f, cellDepth * 2f);
        return true;
    }

    private void AddCoordinate(List<float> coordinates, float value)
    {
        for (int i = 0; i < coordinates.Count; i++)
        {
            if (Mathf.Abs(coordinates[i] - value) <= coordinateTolerance)
            {
                return;
            }
        }

        coordinates.Add(value);
    }

    private int FindCoordinateIndex(List<float> coordinates, float value)
    {
        for (int i = 0; i < coordinates.Count; i++)
        {
            if (Mathf.Abs(coordinates[i] - value) <= coordinateTolerance)
            {
                return i;
            }
        }

        return -1;
    }

    private float GetCellSpacing(List<float> coordinates)
    {
        if (coordinates.Count < 2)
        {
            return Mathf.Max(0.01f, fallbackCellSize);
        }

        return Mathf.Max(0.01f, Mathf.Abs(coordinates[1] - coordinates[0]));
    }
}
