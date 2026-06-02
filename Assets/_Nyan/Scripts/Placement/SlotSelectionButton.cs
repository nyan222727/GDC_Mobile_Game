using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Outline))]
public sealed class SlotSelectionButton : MonoBehaviour
{
    [SerializeField] private int slotIndex;
    [SerializeField] private PlacementController controller;
    [SerializeField] private Button button;
    [SerializeField] private Outline selectionOutline;
    [SerializeField] private Graphic slotGraphic;
    [SerializeField] private Text countText;
    [SerializeField] private Graphic countBadgeBackground;
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private int initialCount = 3;
    [SerializeField] private Color normalSlotColor = new Color32(217, 217, 217, 255);
    [SerializeField] private Color emptySlotColor = new Color32(90, 90, 90, 255);
    [SerializeField] private Color normalBadgeColor = new Color32(45, 45, 45, 235);
    [SerializeField] private Color emptyBadgeColor = new Color32(155, 45, 45, 245);
    [SerializeField] private Color unavailableFlashColor = new Color32(255, 75, 75, 255);

    public int SlotIndex => slotIndex;
    public GameObject CharacterPrefab => characterPrefab;
    public int RemainingCount => Application.isPlaying ? remainingCount : Mathf.Max(0, initialCount);
    public int PreviewReservedCount => reservedPreviewCount;
    public int AvailableCount => Mathf.Max(0, RemainingCount - reservedPreviewCount);
    public bool HasAvailableCount => AvailableCount > 0;

    private int remainingCount;
    private int reservedPreviewCount;
    private Coroutine unavailableFlashRoutine;

    private void Awake()
    {
        initialCount = Mathf.Max(0, initialCount);
        remainingCount = initialCount;

        if (controller == null)
        {
            controller = FindAnyObjectByType<PlacementController>();
        }

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (selectionOutline == null)
        {
            selectionOutline = GetComponent<Outline>();
        }

        if (slotGraphic == null)
        {
            slotGraphic = GetComponent<Graphic>();
        }

        if (countText == null)
        {
            countText = GetComponentInChildren<Text>(true);
        }

        if (countBadgeBackground == null && countText != null)
        {
            Transform badgeTransform = countText.transform.parent;
            if (badgeTransform != null)
            {
                countBadgeBackground = badgeTransform.GetComponent<Graphic>();
            }
        }

        if (button != null)
        {
            button.onClick.AddListener(Select);
        }

        SetSelected(false, Color.white, Vector2.zero);
        RefreshCountUi();
    }

    private void OnValidate()
    {
        initialCount = Mathf.Max(0, initialCount);

        if (!Application.isPlaying)
        {
            remainingCount = initialCount;
            RefreshCountUi();
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(Select);
        }
    }

    public void Select()
    {
        if (!HasAvailableCount)
        {
            FlashUnavailable();
            return;
        }

        if (controller != null)
        {
            controller.SelectSlot(this);
        }
    }

    public bool TryReservePreview()
    {
        if (!HasAvailableCount)
        {
            FlashUnavailable();
            return false;
        }

        reservedPreviewCount++;
        RefreshCountUi();
        return true;
    }

    public void ReleasePreviewReservations()
    {
        if (reservedPreviewCount == 0)
        {
            return;
        }

        reservedPreviewCount = 0;
        RefreshCountUi();
    }

    public void CommitPreviewReservations(int acceptedCount)
    {
        int committedCount = Mathf.Clamp(acceptedCount, 0, reservedPreviewCount);
        remainingCount = Mathf.Max(0, remainingCount - committedCount);
        reservedPreviewCount = 0;
        RefreshCountUi();
    }

    public void RestoreCount(int count)
    {
        if (count <= 0)
        {
            return;
        }

        remainingCount = Mathf.Min(initialCount, remainingCount + count);
        RefreshCountUi();
    }

    public void FlashUnavailable()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (unavailableFlashRoutine != null)
        {
            StopCoroutine(unavailableFlashRoutine);
        }

        unavailableFlashRoutine = StartCoroutine(FlashUnavailableRoutine());
    }

    public void SetSelected(bool selected, Color outlineColor, Vector2 outlineDistance)
    {
        if (selectionOutline == null)
        {
            return;
        }

        selectionOutline.enabled = selected;
        selectionOutline.effectColor = outlineColor;
        selectionOutline.effectDistance = outlineDistance;
    }

    private IEnumerator FlashUnavailableRoutine()
    {
        if (slotGraphic != null)
        {
            slotGraphic.color = unavailableFlashColor;
        }

        if (countBadgeBackground != null)
        {
            countBadgeBackground.color = unavailableFlashColor;
        }

        yield return new WaitForSeconds(0.18f);

        unavailableFlashRoutine = null;
        RefreshCountUi();
    }

    private void RefreshCountUi()
    {
        if (countText != null)
        {
            countText.text = $"x{AvailableCount}";
        }

        bool hasCount = HasAvailableCount;
        if (slotGraphic != null)
        {
            slotGraphic.color = hasCount ? normalSlotColor : emptySlotColor;
        }

        if (countBadgeBackground != null)
        {
            countBadgeBackground.color = hasCount ? normalBadgeColor : emptyBadgeColor;
        }

        if (button != null)
        {
            button.interactable = hasCount;
        }
    }
}
