using System.Collections;
using UnityEngine;

public sealed class MapTile : MonoBehaviour
{
    [SerializeField] private Transform occupant;
    [SerializeField] private Renderer tileRenderer;

    public bool IsOccupied => occupant != null;

    private Color baseColor;
    private Color activeHighlightColor;
    private bool hasBaseColor;
    private bool isPlacementHighlighted;
    private Coroutine invalidFlashRoutine;

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
        CacheRenderer();
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
        return isPlacementHighlighted ? activeHighlightColor : baseColor;
    }
}
