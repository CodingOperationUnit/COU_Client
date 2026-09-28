using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class EquipDetailPopUp : UIPopup
{
    [SerializeField] private Image gradeBanner;
    [SerializeField] private TMP_Text gradeBannerText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image gradeBadge;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text atkText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text skillListText;
    [SerializeField] private Button equipButton;
    [SerializeField] private TMP_Text equipButtonText;
    [SerializeField] private Button closeButton;

    [SerializeField] private Color generalColor;
    [SerializeField] private Color superColor;
    [SerializeField] private Color rareColor;

    private OwnedItem boundItem;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        equipButton.onClick.AddListener(OnEquipButtonClicked);
    }

    public void Show(OwnedItem item)
    {
        boundItem = item;
        var data = item.Data;

        nameText.text = data.itemName;
        descriptionText.text = data.description;

        gradeBannerText.text = GradeLabel(item.Grade);
        gradeBanner.color = GradeColor(item.Grade);

        itemIcon.sprite = Resources.Load<Sprite>(data.iconPath);
        itemIcon.enabled = itemIcon.sprite != null;

        gradeBadge.sprite = ItemDatabase.GetGradeIcon(data.SlotType, item.Grade);
        gradeBadge.enabled = gradeBadge.sprite != null;

        atkText.transform.parent.gameObject.SetActive(data.attackBonus > 0);
        atkText.text = $"ATK {data.attackBonus}";

        hpText.transform.parent.gameObject.SetActive(data.hpBonus > 0);
        hpText.text = $"HP {data.hpBonus}";

        skillListText.text = string.Join("\n", data.gradeSkills);

        RefreshEquipButton();
        Open();
    }

    private void OnEquipButtonClicked()
    {
        if (boundItem.isEquipped)
            PlayerInventory.Instance.Unequip(boundItem.Data.SlotType);
        else
            PlayerInventory.Instance.Equip(boundItem);

        RefreshEquipButton();
    }

    private void RefreshEquipButton()
        => equipButtonText.text = boundItem.isEquipped ? "장착 해제" : "장비";

    private static string GradeLabel(ItemGrade grade) => grade switch
    {
        ItemGrade.General => "일반",
        ItemGrade.Super => "우수",
        ItemGrade.Rare => "레어",
        _ => grade.ToString()
    };

    private Color GradeColor(ItemGrade grade) => grade switch
    {
        ItemGrade.General => generalColor,
        ItemGrade.Super => superColor,
        ItemGrade.Rare => rareColor,
        _ => Color.white
    };
}
