using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SupplyResultPopup : UIPopup
{
    [SerializeField] private Button dimButton;
    [SerializeField] private TMP_Text gradeText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image gradeFrameImage;
    [SerializeField] private Image itemIconImage;

    [SerializeField] private Color generalColor;
    [SerializeField] private Color superColor;
    [SerializeField] private Color rareColor;

    private void Awake()
    {
        dimButton.onClick.AddListener(Close);
    }

    public void Show(OwnedItem item)
    {
        var data = item.Data;
        var grade = item.Grade;

        gradeText.text = GradeLabel(grade);
        gradeText.color = GradeColor(grade);
        nameText.text = data.itemName;
        nameText.color = GradeColor(grade);

        itemIconImage.sprite = Resources.Load<Sprite>(data.iconPath);
        itemIconImage.enabled = itemIconImage.sprite != null;

        gradeFrameImage.sprite = ItemDatabase.GetGradeIcon(data.SlotType, grade);
        gradeFrameImage.enabled = gradeFrameImage.sprite != null;

        Open();
    }

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
