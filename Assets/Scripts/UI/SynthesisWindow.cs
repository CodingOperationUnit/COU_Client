using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 합성창
public class SynthesisWindow : UIView
{
    [SerializeField] private Image resultItemIcon;
    [SerializeField] private Image resultGradeIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text gradeBannerText;
    [SerializeField] private Image gradeBanner;

    [SerializeField] private TMP_Text maxLevelText;
    [SerializeField] private TMP_Text statBonusText;

    [SerializeField] private Image materialItemIcon;
    [SerializeField] private TMP_Text materialCountText;

    [SerializeField] private Image beforeGradeIcon;
    [SerializeField] private Image beforeItemIcon;
    [SerializeField] private Image materialSlot1GradeIcon;
    [SerializeField] private Image materialSlot1Icon;
    [SerializeField] private Image materialSlot2GradeIcon;
    [SerializeField] private Image materialSlot2Icon;

    [SerializeField] private Button synthesizeButton;
    [SerializeField] private TMP_Text synthesizeButtonText;
    [SerializeField] private Button batchSynthesizeButton;
    [SerializeField] private TMP_Text batchSynthesizeButtonText;
    [SerializeField] private Button backButton;

    [SerializeField] private Color generalColor;
    [SerializeField] private Color superColor;
    [SerializeField] private Color rareColor;

    private OwnedItem boundItem;
    public OwnedItem BoundItem => boundItem;

    public event System.Action OnShown;

    private void Awake()
    {
        backButton.onClick.AddListener(Close);
        synthesizeButton.onClick.AddListener(OnSynthesizeButtonClicked);
        batchSynthesizeButton.onClick.AddListener(OnBatchSynthesizeButtonClicked);
    }

    public void Show(OwnedItem item)
    {
        boundItem = item;
        Open();
        try
        {
            Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
        OnShown?.Invoke();
    }

    private void Refresh()
    {
        if (boundItem == null)
            return;

        var data = boundItem.Data;
        var currentIconSprite = Resources.Load<Sprite>(data.iconPath);
        var currentGradeSprite = ItemDatabase.GetGradeIcon(data.SlotType, boundItem.Grade);

        nameText.text = data.itemName;

        gradeBannerText.text = GradeLabel(boundItem.Grade);
        gradeBanner.color = GradeColor(boundItem.Grade);

        var previewGrade = boundItem.CanSynthesize ? boundItem.NextGrade : boundItem.Grade;
        var previewGradeSprite = ItemDatabase.GetGradeIcon(data.SlotType, previewGrade);

        resultItemIcon.sprite = currentIconSprite;
        resultItemIcon.enabled = resultItemIcon.sprite != null;

        resultGradeIcon.sprite = previewGradeSprite;
        resultGradeIcon.enabled = resultGradeIcon.sprite != null;

        materialItemIcon.sprite = currentIconSprite;
        materialItemIcon.enabled = materialItemIcon.sprite != null;

        var owned = PlayerInventory.Instance.CountSynthesisMaterials(boundItem);
        var required = ItemLevelConfig.SynthesisMaterialCount;
        materialCountText.text = $"필수 자료: {required}x {GradeLabel(boundItem.Grade)} {data.itemName} (보유 {owned}/{required})";

        beforeGradeIcon.sprite = currentGradeSprite;
        beforeGradeIcon.enabled = beforeGradeIcon.sprite != null;
        beforeItemIcon.sprite = currentIconSprite;
        beforeItemIcon.enabled = beforeItemIcon.sprite != null;

        materialSlot1GradeIcon.sprite = currentGradeSprite;
        materialSlot1GradeIcon.enabled = materialSlot1GradeIcon.sprite != null;
        materialSlot1Icon.sprite = currentIconSprite;
        materialSlot1Icon.enabled = materialSlot1Icon.sprite != null;

        materialSlot2GradeIcon.sprite = currentGradeSprite;
        materialSlot2GradeIcon.enabled = materialSlot2GradeIcon.sprite != null;
        materialSlot2Icon.sprite = currentIconSprite;
        materialSlot2Icon.enabled = materialSlot2Icon.sprite != null;

        if (boundItem.CanSynthesize)
        {
            var next = boundItem.NextGrade;
            maxLevelText.text = $"등급 {GradeLabel(boundItem.Grade)} → {GradeLabel(next)}";

            var bonusPercent = Mathf.RoundToInt((ItemLevelConfig.GetGradeMultiplier(next) - 1f) * 100f);
            statBonusText.text = $"스탯 +{bonusPercent}%";

            var canSynthesize = PlayerInventory.Instance.CanSynthesize(boundItem);
            synthesizeButton.interactable = canSynthesize;
            batchSynthesizeButton.interactable = canSynthesize;
            synthesizeButtonText.text = "합성";
        }
        else
        {
            maxLevelText.text = $"등급 {GradeLabel(boundItem.Grade)} (MAX)";
            statBonusText.text = string.Empty;
            synthesizeButton.interactable = false;
            batchSynthesizeButton.interactable = false;
            synthesizeButtonText.text = "MAX";
        }

        batchSynthesizeButtonText.text = "일괄 합성";
    }

    private void OnSynthesizeButtonClicked()
    {
        if (PlayerInventory.Instance.TrySynthesize(boundItem))
            Refresh();
    }

    private void OnBatchSynthesizeButtonClicked()
    {
        if (PlayerInventory.Instance.BatchSynthesize(boundItem) > 0)
            Refresh();
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
