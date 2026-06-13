using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

public sealed class MoneyManager : MonoBehaviour, ILevelMoneyWallet
{
    [Header("Money")]
    [SerializeField] private int startingMoney = 100;
    [SerializeField] private Text moneyText;
    [SerializeField] private string moneyFormat = "Money: {0}";
    [SerializeField] private bool showAvailableMoneyWhilePreviewing = true;

    [Header("Feedback")]
    [SerializeField] private Graphic flashGraphic;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color insufficientColor = new Color32(255, 75, 75, 255);
    [SerializeField] private float flashDuration = 0.18f;

    private int currentMoney;
    private int reservedMoney;
    private Coroutine flashRoutine;

    public event System.Action MoneyChanged;

    public int CurrentMoney => Application.isPlaying ? currentMoney : Mathf.Max(0, startingMoney);
    public int ReservedMoney => Application.isPlaying ? reservedMoney : 0;
    public int AvailableMoney => Mathf.Max(0, CurrentMoney - ReservedMoney);

    private void Awake()
    {
        currentMoney = Mathf.Max(0, startingMoney);
        reservedMoney = 0;

        if (flashGraphic == null && moneyText != null)
        {
            flashGraphic = moneyText;
        }

        RefreshUi();
    }

    private void OnValidate()
    {
        startingMoney = Mathf.Max(0, startingMoney);
        flashDuration = Mathf.Max(0f, flashDuration);

        if (!Application.isPlaying)
        {
            RefreshUi();
        }
    }

    public void SetMoney(int amount)
    {
        currentMoney = Mathf.Max(0, amount);
        reservedMoney = Mathf.Min(reservedMoney, currentMoney);
        NotifyMoneyChanged();
    }

    public bool CanReserve(int amount)
    {
        return Mathf.Max(0, amount) <= AvailableMoney;
    }

    public bool TryReserve(int amount)
    {
        int safeAmount = Mathf.Max(0, amount);
        if (safeAmount == 0)
        {
            return true;
        }

        if (!CanReserve(safeAmount))
        {
            ShowInsufficientFunds();
            return false;
        }

        reservedMoney += safeAmount;
        NotifyMoneyChanged();
        return true;
    }

    public void ReleaseReserved(int amount)
    {
        int safeAmount = Mathf.Max(0, amount);
        if (safeAmount == 0 || reservedMoney == 0)
        {
            return;
        }

        reservedMoney = Mathf.Max(0, reservedMoney - safeAmount);
        NotifyMoneyChanged();
    }

    public void CommitReserved(int amount)
    {
        int safeAmount = Mathf.Max(0, amount);
        if (safeAmount == 0)
        {
            return;
        }

        int committedAmount = Mathf.Min(safeAmount, reservedMoney);
        reservedMoney -= committedAmount;
        currentMoney = Mathf.Max(0, currentMoney - committedAmount);
        NotifyMoneyChanged();
    }

    public void AddMoney(int amount)
    {
        int safeAmount = Mathf.Max(0, amount);
        if (safeAmount == 0)
        {
            return;
        }

        currentMoney += safeAmount;
        NotifyMoneyChanged();
    }

    public void Refund(int amount)
    {
        AddMoney(amount);
    }

    public void ShowInsufficientFunds()
    {
        if (!isActiveAndEnabled || flashGraphic == null)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private void NotifyMoneyChanged()
    {
        RefreshUi();
        MoneyChanged?.Invoke();
    }

    private void RefreshUi()
    {
        if (moneyText != null)
        {
            int displayMoney = showAvailableMoneyWhilePreviewing ? AvailableMoney : currentMoney;
            moneyText.text = string.Format(moneyFormat, displayMoney);
        }

        if (flashGraphic != null && flashRoutine == null)
        {
            flashGraphic.color = normalColor;
        }
    }

    private IEnumerator FlashRoutine()
    {
        flashGraphic.color = insufficientColor;
        yield return new WaitForSeconds(flashDuration);
        flashRoutine = null;
        RefreshUi();
    }
}

#pragma warning restore 0649
