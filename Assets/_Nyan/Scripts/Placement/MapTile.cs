using System.Collections;
using UnityEngine;

public sealed class MapTile : MonoBehaviour
{
    [SerializeField] private Transform occupant;
    [SerializeField] private Renderer tileRenderer;

    public bool IsOccupied => occupant != null;
    public bool BlocksPlacement => GetTileProperty() != null && GetTileProperty().isPath;
    public bool CanPlace => !IsOccupied && !BlocksPlacement;

    private Color baseColor;
    private Color activeHighlightColor;
    private bool hasBaseColor;
    private bool isPlacementHighlighted;
    private Coroutine invalidFlashRoutine;
    private TileProperty tileProperty;
    private GameObject pathHighlightObject;
    private Material pathHighlightMaterial;

    public void SetOccupant(Transform newOccupant)
    {
        occupant = newOccupant;
    }

    public void ClearOccupant(Transform expectedOccupant)
    {
        if (expectedOccupant == null || occupant != expectedOccupant)
        {
            return;
        }

        occupant = null;
    }

    public void SetPlacementHighlight(bool highlighted, Color highlightColor)
    {
        CacheRenderer();
        if (tileRenderer == null)
        {
            return;
        }

        isPlacementHighlighted = highlighted;
        activeHighlightColor = highlightColor;
        SetRendererColor(GetCurrentTileColor());
    }

    public void SetPathHighlight(bool highlighted, Color highlightColor, GameObject markerPrefab)
    {
        if (markerPrefab == null)
        {
            if (pathHighlightObject != null)
            {
                pathHighlightObject.SetActive(false);
            }

            return;
        }

        if (highlighted)
        {
            EnsurePathHighlightObject(highlightColor, markerPrefab);
        }

        if (pathHighlightObject != null)
        {
            pathHighlightObject.SetActive(highlighted);
        }
    }

    public void FlashInvalid(Color invalidColor)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        CacheRenderer();
        if (tileRenderer == null)
        {
            return;
        }

        if (invalidFlashRoutine != null)
        {
            StopCoroutine(invalidFlashRoutine);
        }

        invalidFlashRoutine = StartCoroutine(FlashInvalidRoutine(invalidColor));
    }

    private void Awake()
    {
        CacheTileProperty();
        CacheRenderer();
    }

    private TileProperty GetTileProperty()
    {
        if (tileProperty == null)
        {
            CacheTileProperty();
        }

        return tileProperty;
    }

    private void CacheTileProperty()
    {
        if (tileProperty == null)
        {
            tileProperty = GetComponent<TileProperty>();
        }
    }

    private void CacheRenderer()
    {
        if (tileRenderer == null)
        {
            tileRenderer = GetComponentInChildren<Renderer>();
        }

        if (tileRenderer == null || hasBaseColor)
        {
            return;
        }

        Material material = tileRenderer.material;
        baseColor = ReadRendererColor(material);
        hasBaseColor = true;
    }

    private void EnsurePathHighlightObject(Color highlightColor, GameObject markerPrefab)
    {
        if (pathHighlightObject == null)
        {
            pathHighlightObject = CreatePathHighlightObject(markerPrefab);
        }

        Renderer[] markerRenderers = pathHighlightObject.GetComponentsInChildren<Renderer>(true);
        if (markerRenderers.Length == 0)
        {
            return;
        }

        if (pathHighlightMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            pathHighlightMaterial = new Material(shader);
        }

        ApplyColor(pathHighlightMaterial, highlightColor);
        for (int i = 0; i < markerRenderers.Length; i++)
        {
            markerRenderers[i].sharedMaterial = pathHighlightMaterial;
        }
    }

    private GameObject CreatePathHighlightObject(GameObject markerPrefab)
    {
        GameObject marker = Instantiate(markerPrefab, transform, false);
        marker.name = "Path Highlight";
        RemoveMarkerColliders(marker);
        marker.SetActive(false);
        return marker;
    }

    private static void RemoveMarkerColliders(GameObject marker)
    {
        Collider[] markerColliders = marker.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < markerColliders.Length; i++)
        {
            Destroy(markerColliders[i]);
        }
    }

    private static Color ReadRendererColor(Material material)
    {
        if (material == null)
        {
            return Color.white;
        }

        if (material.HasProperty("_BaseColor"))
        {
            return material.GetColor("_BaseColor");
        }

        if (material.HasProperty("_Color"))
        {
            return material.GetColor("_Color");
        }

        return Color.white;
    }

    private void SetRendererColor(Color color)
    {
        Material material = tileRenderer.material;
        ApplyColor(material, color);
    }

    private static void ApplyColor(Material material, Color color)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    private IEnumerator FlashInvalidRoutine(Color invalidColor)
    {
        SetRendererColor(invalidColor);
        yield return new WaitForSeconds(0.16f);
        invalidFlashRoutine = null;
        SetRendererColor(GetCurrentTileColor());
    }

    private Color GetCurrentTileColor()
    {
        if (isPlacementHighlighted)
        {
            return activeHighlightColor;
        }

        return baseColor;
    }
}
