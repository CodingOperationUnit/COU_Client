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

    public static Sprite GetGradeIcon(EquipSlotType slotType, ItemGrade grade)
        => Resources.Load<Sprite>($"Equip/Inven/Item/{slotType}/Grade/{grade}");
}
