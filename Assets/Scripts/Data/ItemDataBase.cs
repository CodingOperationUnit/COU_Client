using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class ItemDatabase
{
    private static Dictionary<long, ItemData> items;

    public static void Load()
    {
        if (items != null) return;

        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(GameConstants.Paths.ItemData_Json_Path);

            if (jsonFile == null)
            {
                Debug.LogError($"아이템 JSON 파일이 없습니다: {GameConstants.Paths.ItemData_Json_Path}");
                return;
            }

            JObject root = JObject.Parse(jsonFile.text);
            JArray rows = root["datas"] as JArray;

            if (rows == null || rows.Count == 0)
            {
                Debug.LogError("아이템 데이터 목록이 비어 있습니다.");
                return;
            }

            var loadedItems = new Dictionary<long, ItemData>();
            foreach (JToken row in rows)
            {
                if (!(row is JObject))
                {
                    throw new InvalidOperationException("아이템 데이터 항목이 객체 형식이 아닙니다.");
                }

                ItemData data = row.ToObject<ItemData>();

                if (data == null || data.itemId <= 0)
                {
                    throw new InvalidOperationException("아이템 데이터 또는 ID가 올바르지 않습니다.");
                }

                if (loadedItems.ContainsKey(data.itemId))
                {
                    throw new InvalidOperationException($"중복된 아이템 ID: {data.itemId}");
                }

                data.OnLoaded();
                loadedItems.Add(data.itemId, data);
            }

            // 전체 로드가 성공했을 때만 반영
            items = loadedItems;
        }
        catch (Exception exception)
        {
            Debug.LogError($"아이템 데이터 로드 실패: {exception.Message}");
        }
    }

    public static ItemData Get(long itemId) => items[itemId];

    public static IEnumerable<ItemData> GetAll() => items.Values;

    public static Sprite GetGradeIcon(EquipSlotType slotType, ItemGrade grade)
    {
        var folder = slotType == EquipSlotType.Weapon ? "Weapon" : "Equip";
        return Resources.Load<Sprite>($"Equip/Inven/Item/{folder}/Grade/{grade}");
    }

}
