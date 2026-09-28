using UnityEngine;
using UnityEngine.UI;

public class ItemSlotView : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image gradeIcon;
    [SerializeField] private Button button;

    private OwnedItem boundItem;

    private void Awake()
    {
        button.onClick.AddListener(OnClicked);
    }

    public void Setup(OwnedItem item)
    {
        boundItem = item;
        var data = item.Data;

        itemIcon.sprite = Resources.Load<Sprite>(data.iconPath);
        itemIcon.enabled = itemIcon.sprite != null;

        gradeIcon.sprite = ItemDatabase.GetGradeIcon(data.SlotType, item.Grade);
        gradeIcon.enabled = gradeIcon.sprite != null;
    }

    private void OnClicked()
        => UIManager.Instance.Get<EquipDetailPopUp>().Show(boundItem);
}
