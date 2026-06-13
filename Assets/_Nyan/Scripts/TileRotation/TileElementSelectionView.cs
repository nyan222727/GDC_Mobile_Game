using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
[RequireComponent(typeof(LineRenderer))]
public sealed class TileElementSelectionView : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Color normalColor = new Color32(0, 200, 255, 255);
    [SerializeField] private Color invalidColor = new Color32(255, 70, 70, 255);
    [SerializeField] private Color rotateColor = new Color32(255, 210, 70, 255);
    [SerializeField] private float lineWidth = 0.09f;
    [SerializeField] private float verticalOffset = 0.58f;
    [SerializeField] private float feedbackDuration = 0.16f;

    private Material runtimeMaterial;
    private Coroutine feedbackRoutine;

    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        ConfigureLineRenderer();
        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }

    public void SetVisible(bool visible)
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = visible;
        }
    }

    public void ShowSelection(TileElementGrid grid, Vector2Int origin)
    {
        if (lineRenderer == null || grid == null ||
            !grid.TryGetSelectionFrame(origin, out Vector3 center, out Vector2 size))
        {
            SetVisible(false);
            return;
        }

        float halfWidth = size.x * 0.5f;
        float halfDepth = size.y * 0.5f;
        float y = center.y + verticalOffset;

        lineRenderer.positionCount = 5;
        lineRenderer.SetPosition(0, new Vector3(center.x - halfWidth, y, center.z - halfDepth));
        lineRenderer.SetPosition(1, new Vector3(center.x - halfWidth, y, center.z + halfDepth));
        lineRenderer.SetPosition(2, new Vector3(center.x + halfWidth, y, center.z + halfDepth));
        lineRenderer.SetPosition(3, new Vector3(center.x + halfWidth, y, center.z - halfDepth));
        lineRenderer.SetPosition(4, new Vector3(center.x - halfWidth, y, center.z - halfDepth));
        SetLineColor(normalColor);
        SetVisible(true);
    }

    public void FlashInvalid()
    {
        StartFeedback(invalidColor);
    }

    public void FlashRotated()
    {
        StartFeedback(rotateColor);
    }

    private void ConfigureLineRenderer()
    {
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.numCornerVertices = 2;
        lineRenderer.numCapVertices = 2;
        lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.textureMode = LineTextureMode.Stretch;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader != null)
        {
            runtimeMaterial = new Material(shader);
            lineRenderer.material = runtimeMaterial;
        }

        SetLineColor(normalColor);
    }

    private void StartFeedback(Color color)
    {
        if (!isActiveAndEnabled || lineRenderer == null)
        {
            return;
        }

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }

        feedbackRoutine = StartCoroutine(FeedbackRoutine(color));
    }

    private IEnumerator FeedbackRoutine(Color color)
    {
        SetLineColor(color);
        yield return new WaitForSeconds(feedbackDuration);
        feedbackRoutine = null;
        SetLineColor(normalColor);
    }

    private void SetLineColor(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        if (runtimeMaterial == null)
        {
            return;
        }

        if (runtimeMaterial.HasProperty("_BaseColor"))
        {
            runtimeMaterial.SetColor("_BaseColor", color);
        }
        else if (runtimeMaterial.HasProperty("_Color"))
        {
            runtimeMaterial.SetColor("_Color", color);
        }
    }
}
