using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CharacterEquipView : MonoBehaviour
{
    [Serializable]
    private class SlotUI
    {
        public EquipSlotType slotType;
        public Image frame;  
        public Image icon; 
        public Button button;

        [NonSerialized] public Sprite defaultFrameSprite;
        [NonSerialized] public bool cached;
    }

    [SerializeField] private SlotUI[] slots;
    [SerializeField] private TMP_Text atkValueText;
    [SerializeField] private TMP_Text hpValueText;

    // 장비를 장착하지 않았을 때 모든 부위 슬롯에 공통으로 띄우는 기본 아이콘.
    private const string EmptySlotIconPath = "Equip/Inven/Item/Weapon/Weapon";
    private Sprite emptySlotSprite;

    // 아이콘 크기
    [SerializeField] private Vector2 equippedIconSize = new(75f, 75f);
    [SerializeField] private Vector2 emptySlotIconSize = new(85f, 85f);

    private static readonly System.Collections.Generic.Dictionary<EquipSlotType, Vector2> SlotGridPosition = new()
    {
        { EquipSlotType.Weapon,   new Vector2(-380f, 240f) },
        { EquipSlotType.Armor,    new Vector2(380f, 240f) },
        { EquipSlotType.Necklace, new Vector2(-380f, 30f) },
        { EquipSlotType.Belt,     new Vector2(380f, 30f) },
        { EquipSlotType.Gloves,   new Vector2(-380f, -180f) },
        { EquipSlotType.Shoes,    new Vector2(380f, -180f) },
    };

    private void Awake()
    {
        emptySlotSprite = Resources.Load<Sprite>(EmptySlotIconPath);

        foreach (var slot in slots)
        {
            if (slot.button != null && SlotGridPosition.TryGetValue(slot.slotType, out var pos))
            {
                var slotRect = slot.button.GetComponent<RectTransform>();
                if (slotRect != null)
                    slotRect.anchoredPosition = pos;
            }

            if (slot.button != null && slot.button.targetGraphic is Image bg)
            {
                var c = bg.color;
                c.a = 0f;
                bg.color = c;
            }

            if (slot.icon != null)
                slot.icon.color = Color.white;

            if (slot.button == null) continue;

            var captured = slot; 
            slot.button.onClick.AddListener(() => OnSlotClicked(captured));
        }
    }

    private void Start()
    {
        if (PlayerInventory.Instance == null) return;

        PlayerInventory.Instance.OnInventoryChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnInventoryChanged -= Refresh;
    }

    private void OnSlotClicked(SlotUI slot)
    {
        var equippedItem = PlayerInventory.Instance.GetEquipped(slot.slotType);
        if (equippedItem == null) return; 

        UIManager.Instance.Get<EquipDetailPopUp>().Show(equippedItem);
    }

    private void Refresh()
    {
        foreach (var slot in slots)
            RefreshSlot(slot);

        if (atkValueText != null)
            atkValueText.text = PlayerInventory.Instance.GetTotalStat(o => o.ScaledAttack).ToString();
        if (hpValueText != null)
            hpValueText.text = PlayerInventory.Instance.GetTotalStat(o => o.ScaledHp).ToString();
    }

    private void RefreshSlot(SlotUI slot)
    {
        if (!slot.cached)
        {
            slot.defaultFrameSprite = slot.frame != null ? slot.frame.sprite : null;
            slot.cached = true;
        }

        var equippedItem = PlayerInventory.Instance.GetEquipped(slot.slotType);

        if (equippedItem != null)
        {
            var data = equippedItem.Data;

            if (slot.icon != null)
            {
                slot.icon.sprite = Resources.Load<Sprite>(data.iconPath);
                slot.icon.enabled = slot.icon.sprite != null;
                slot.icon.rectTransform.sizeDelta = equippedIconSize;
            }

            if (slot.frame != null)
            {
                var gradeSprite = ItemDatabase.GetGradeIcon(slot.slotType, equippedItem.Grade);
                slot.frame.sprite = gradeSprite != null ? gradeSprite : slot.defaultFrameSprite;
                slot.frame.enabled = slot.frame.sprite != null;
            }
        }
        else
        {

            if (slot.icon != null)
            {
                slot.icon.sprite = emptySlotSprite;
                slot.icon.enabled = emptySlotSprite != null;
                slot.icon.rectTransform.sizeDelta = emptySlotIconSize;
            }

            if (slot.frame != null)
            {
                slot.frame.sprite = slot.defaultFrameSprite;
                slot.frame.enabled = slot.frame.sprite != null;
            }
        }
    }
}
