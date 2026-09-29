using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionTooltip : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button unlockButton;
    [SerializeField] private Image costIcon;
    [SerializeField] private TMP_Text costText;

    public Button UnlockButton => unlockButton;

    public void Show(EvolutionNodeInfo node, Vector2 position, bool unlockable, Color currencyColor)
    {
        nameText.text = node.name;
        valueText.text = node.value;
        descriptionText.text = node.description;
        unlockButton.gameObject.SetActive(unlockable);
        costIcon.color = currencyColor;
        costText.text = $"x {node.cost}";
        ((RectTransform)transform).anchoredPosition = position;
        gameObject.SetActive(true);
    }

    public void Hide()
        => gameObject.SetActive(false);
}
