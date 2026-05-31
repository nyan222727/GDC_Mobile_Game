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

    public int SlotIndex => slotIndex;

    private void Awake()
    {
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

        if (button != null)
        {
            button.onClick.AddListener(Select);
        }

        SetSelected(false, Color.white, Vector2.zero);
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
        if (controller != null)
        {
            controller.SelectSlot(this);
        }
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
}
