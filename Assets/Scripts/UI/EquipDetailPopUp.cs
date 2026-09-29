using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipDetailPopUp : UIPopup
{
    [SerializeField] private Image gradeBanner;
    [SerializeField] private TMP_Text gradeBannerText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image gradeIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text atkText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text skillListText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private Button equipButton;
    [SerializeField] private TMP_Text equipButtonText;
    [SerializeField] private Button levelUpButton;
    [SerializeField] private TMP_Text levelUpButtonText;
    [SerializeField] private Button batchLevelUpButton;
    [SerializeField] private TMP_Text batchLevelUpButtonText;
    [SerializeField] private Button closeButton;

    [SerializeField] private Color generalColor;
    [SerializeField] private Color superColor;
    [SerializeField] private Color rareColor;
    [SerializeField] private Color skillBulletColor = new(0.31f, 0.76f, 0.97f);

    private OwnedItem boundItem;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        equipButton.onClick.AddListener(OnEquipButtonClicked);
        levelUpButton.onClick.AddListener(OnLevelUpButtonClicked);
        batchLevelUpButton.onClick.AddListener(OnBatchLevelUpButtonClicked);
    }

    public void Show(OwnedItem item)
    {
        boundItem = item;
        Refresh();
        Open();
    }

    private void Refresh()
    {
        var data = boundItem.Data;

        nameText.text = data.itemName;
        descriptionText.text = data.description;

        gradeBannerText.text = GradeLabel(boundItem.Grade);
        gradeBanner.color = GradeColor(boundItem.Grade);

        itemIcon.sprite = Resources.Load<Sprite>(data.iconPath);
        itemIcon.enabled = itemIcon.sprite != null;

        // 아이템슬롯(ItemSlotView)과 동일하게, 아이콘 뒤 프레임을 등급별 아이콘/테두리로 교체.
        gradeIcon.sprite = ItemDatabase.GetGradeIcon(data.SlotType, boundItem.Grade);
        gradeIcon.enabled = gradeIcon.sprite != null;

        // 공격력이 있으면 공격력만, 체력이 있으면 체력만 (둘 다 있으면 둘 다) 표시.
        // 레벨업으로 올라간 실제 스탯(ScaledAttack/ScaledHp)을 그대로 반영한다.
        atkText.transform.parent.gameObject.SetActive(data.attackBonus > 0);
        atkText.text = $"ATK {boundItem.ScaledAttack}";

        hpText.transform.parent.gameObject.SetActive(data.hpBonus > 0);
        hpText.text = $"HP {boundItem.ScaledHp}";

        levelText.text = $"레벨: {boundItem.level}/{ItemLevelConfig.MaxLevel}";

        skillListText.text = string.Join("\n", data.gradeSkills.Select(FormatSkillLine));

        RefreshLevelUpArea();
        RefreshEquipButton();
    }

    private string FormatSkillLine(string skill)
        => $"<color=#{ColorUtility.ToHtmlStringRGB(skillBulletColor)}>■</color> {skill}";

    private void RefreshLevelUpArea()
    {
        var playerGold = PlayerInventory.Instance.Gold;

        if (boundItem.CanLevelUp)
        {
            var cost = boundItem.NextLevelUpCost;
            goldText.text = $"{FormatGold(playerGold)}/{FormatGold(cost)}";

            var canAfford = playerGold >= cost;
            levelUpButton.interactable = canAfford;
            batchLevelUpButton.interactable = canAfford;
            levelUpButtonText.text = "레벨업";
        }
        else
        {
            goldText.text = $"{FormatGold(playerGold)} (MAX)";
            levelUpButton.interactable = false;
            batchLevelUpButton.interactable = false;
            levelUpButtonText.text = "MAX";
        }

        batchLevelUpButtonText.text = "일괄 레벨업";
    }

    private static string FormatGold(int amount)
        => amount >= 10000 ? $"{amount / 1000f:0.#}K" : amount.ToString();

    private void OnLevelUpButtonClicked()
    {
        if (PlayerInventory.Instance.TryLevelUp(boundItem))
            Refresh();
    }

    private void OnBatchLevelUpButtonClicked()
    {
        if (PlayerInventory.Instance.BatchLevelUp(boundItem) > 0)
            Refresh();
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
        => equipButtonText.text = boundItem.isEquipped ? "장착 해제" : "장착";

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
