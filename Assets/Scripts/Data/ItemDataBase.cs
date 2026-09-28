using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    private static Dictionary<long, ItemData> items;

    public static void Load()
    {
        if (items != null) return;

        var json = Resources.Load<TextAsset>("Data/items").text;
        var wrapper = JsonUtility.FromJson<ItemDataListWrapper>(json);

        items = new Dictionary<long, ItemData>();
        foreach (var item in wrapper.items)
            items[item.itemId] = item;
    }

    public static ItemData Get(long itemId) => items[itemId];

    // 무기는 Weapon 폴더에 등급 아이콘이 따로 있고,
    // 나머지 5개 부위(방어구/벨트/장갑/목걸이/신발)는 Equip 폴더 안에 공용으로 모여있음
    public static Sprite GetGradeIcon(EquipSlotType slotType, ItemGrade grade)
    {
        var folder = slotType == EquipSlotType.Weapon ? "Weapon" : "Equip";
        return Resources.Load<Sprite>($"Equip/Inven/Item/{folder}/Grade/{grade}");
    }
}
