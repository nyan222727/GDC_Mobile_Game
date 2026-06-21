using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
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
    [FormerlySerializedAs("countText")]
    [SerializeField] private Text costText;
    [FormerlySerializedAs("countBadgeBackground")]
    [SerializeField] private Graphic costBadgeBackground;
    [SerializeField] private Text selectedNameText;
    [SerializeField] private GameObject characterPrefab;
    [FormerlySerializedAs("initialCount")]
    [SerializeField] private int placementCost = 10;
    [SerializeField] private MonoBehaviour moneyManager;
    [SerializeField] private Color normalSlotColor = new Color32(217, 217, 217, 255);
    [SerializeField] private Color emptySlotColor = new Color32(90, 90, 90, 255);
    [SerializeField] private Color normalBadgeColor = new Color32(45, 45, 45, 235);
    [SerializeField] private Color emptyBadgeColor = new Color32(155, 45, 45, 245);
    [SerializeField] private Color unavailableFlashColor = new Color32(255, 75, 75, 255);

    public int SlotIndex => slotIndex;
    public GameObject CharacterPrefab => characterPrefab;
    public int PlacementCost => Mathf.Max(0, placementCost);
    public int PreviewReservedCount => reservedPreviewCount;
    public bool HasAvailableCount => CanAffordOnePlacement();

    private int reservedPreviewCount;
    private Coroutine unavailableFlashRoutine;
    private ILevelMoneyWallet MoneyWallet => moneyManager as ILevelMoneyWallet;

    private void Awake()
    {
        placementCost = Mathf.Max(0, placementCost);

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

        if (costText == null)
        {
            costText = GetComponentInChildren<Text>(true);
        }

        if (costBadgeBackground == null && costText != null)
        {
            Transform badgeTransform = costText.transform.parent;
            if (badgeTransform != null)
            {
                costBadgeBackground = badgeTransform.GetComponent<Graphic>();
            }
        }

        if (moneyManager == null)
        {
            moneyManager = FindMoneyManager();
        }

        if (MoneyWallet != null)
        {
            MoneyWallet.MoneyChanged += RefreshCostUi;
        }

        if (button != null)
        {
            button.onClick.AddListener(Select);
        }

        SetSelected(false, Color.white, Vector2.zero);
        RefreshCostUi();
    }

    private void OnValidate()
    {
        placementCost = Mathf.Max(0, placementCost);

        if (!Application.isPlaying)
        {
            RefreshCostUi();
        }
    }

    private void OnDestroy()
    {
        if (MoneyWallet != null)
        {
            MoneyWallet.MoneyChanged -= RefreshCostUi;
        }

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

        if (MoneyWallet != null && !MoneyWallet.TryReserve(PlacementCost))
        {
            FlashUnavailable();
            return false;
        }

        reservedPreviewCount++;
        RefreshCostUi();
        return true;
    }

    public void ReleasePreviewReservations()
    {
        if (reservedPreviewCount == 0)
        {
            return;
        }

        int releasedCount = reservedPreviewCount;
        reservedPreviewCount = 0;

        if (MoneyWallet != null)
        {
            MoneyWallet.ReleaseReserved(PlacementCost * releasedCount);
        }

        RefreshCostUi();
    }

    public void CommitPreviewReservations(int acceptedCount)
    {
        int committedCount = Mathf.Clamp(acceptedCount, 0, reservedPreviewCount);
        int releasedCount = Mathf.Max(0, reservedPreviewCount - committedCount);
        reservedPreviewCount = 0;

        if (MoneyWallet != null)
        {
            MoneyWallet.CommitReserved(PlacementCost * committedCount);
            MoneyWallet.ReleaseReserved(PlacementCost * releasedCount);
        }

        RefreshCostUi();
    }

    public void RestoreCount(int count)
    {
        if (count <= 0)
        {
            return;
        }

        if (MoneyWallet != null)
        {
            MoneyWallet.Refund(PlacementCost * count);
        }

        RefreshCostUi();
    }

    public void FlashUnavailable()
    {
        MoneyWallet?.ShowInsufficientFunds();

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
        if (selectionOutline != null)
        {
            selectionOutline.enabled = selected;
            selectionOutline.effectColor = outlineColor;
            selectionOutline.effectDistance = outlineDistance;
        }

        if (selectedNameText != null)
        {
            selectedNameText.gameObject.SetActive(selected);
        }
    }

    private IEnumerator FlashUnavailableRoutine()
    {
        if (slotGraphic != null)
        {
            slotGraphic.color = unavailableFlashColor;
        }

        if (costBadgeBackground != null)
        {
            costBadgeBackground.color = unavailableFlashColor;
        }

        yield return new WaitForSeconds(0.18f);

        unavailableFlashRoutine = null;
        RefreshCostUi();
    }

    private void RefreshCostUi()
    {
        if (costText != null)
        {
            costText.text = $"${PlacementCost}";
        }

        bool hasCount = HasAvailableCount;
        if (slotGraphic != null)
        {
            slotGraphic.color = hasCount ? normalSlotColor : emptySlotColor;
        }

        if (costBadgeBackground != null)
        {
            costBadgeBackground.color = hasCount ? normalBadgeColor : emptyBadgeColor;
        }

        if (button != null)
        {
            button.interactable = true;
        }
    }

    private bool CanAffordOnePlacement()
    {
        return MoneyWallet == null || MoneyWallet.CanReserve(PlacementCost);
    }

    private static MonoBehaviour FindMoneyManager()
    {
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour is ILevelMoneyWallet)
            {
                return behaviour;
            }
        }

        return null;
    }
}
