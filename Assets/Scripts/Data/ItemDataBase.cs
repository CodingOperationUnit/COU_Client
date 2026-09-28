using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    private static Dictionary<string, ItemData> items;

    public static void Load()
    {
        if (items != null) return;

        var json = Resources.Load<TextAsset>("Data/items").text;
        var wrapper = JsonUtility.FromJson<ItemDataListWrapper>(json);

        items = new Dictionary<string, ItemData>();
        foreach (var item in wrapper.items)
            items[item.itemId] = item;
    }

    public static ItemData Get(string itemId) => items[itemId];

    public static Sprite GetGradeIcon(ItemGrade grade)
        => Resources.Load<Sprite>($"Grade/{grade}");
}
