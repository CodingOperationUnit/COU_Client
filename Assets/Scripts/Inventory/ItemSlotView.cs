using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotView : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image gradeIcon;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text levelText;
    
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private GameObject checkOverlay;

    private OwnedItem boundItem;
    private SynthesisWindow synthesisTarget;

    private void Awake()
    {
        button.onClick.AddListener(OnClicked);
    }

    public void Setup(OwnedItem item, SynthesisWindow synthesisTarget = null)
    {
        boundItem = item;
        this.synthesisTarget = synthesisTarget;
        var data = item.Data;

        itemIcon.sprite = Resources.Load<Sprite>(data.iconPath);
        itemIcon.enabled = itemIcon.sprite != null;

        gradeIcon.sprite = ItemDatabase.GetGradeIcon(data.SlotType, item.Grade);
        gradeIcon.enabled = gradeIcon.sprite != null;

        if (levelText != null)
            levelText.text = $"Lv{item.level}";

        RefreshSynthesisOverlay();
    }

    private void RefreshSynthesisOverlay()
    {
        if (lockOverlay == null && checkOverlay == null)
            return;

        if (synthesisTarget == null)
        {
            if (lockOverlay != null) lockOverlay.SetActive(false);
            if (checkOverlay != null) checkOverlay.SetActive(false);
            return;
        }

        var bound = synthesisTarget.BoundItem;
        var isMatch = bound != null && bound.itemId == boundItem.itemId;

        // 오버레이 ㅈ게ㅓ
        if (lockOverlay != null) lockOverlay.SetActive(false);
        if (checkOverlay != null) checkOverlay.SetActive(isMatch);
    }

    private void OnClicked()
    {
        if (synthesisTarget != null)
            synthesisTarget.Show(boundItem);
        else
            UIManager.Instance.Get<EquipDetailPopUp>().Show(boundItem);
    }
}
