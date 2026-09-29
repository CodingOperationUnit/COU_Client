using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private readonly List<OwnedItem> items = new();
    private readonly Dictionary<EquipSlotType, OwnedItem> equipped = new();

    // 골드 테스트용
    [SerializeField] private int gold = 500000;
    public int Gold => gold;

    public event Action OnInventoryChanged;

    public IReadOnlyList<OwnedItem> Items => items;

    // 로컬 저장 키. 
    private const string SaveKey = "PlayerInventory.Save";

    [Serializable]
    private class SaveData
    {
        public int gold;
        public OwnedItem[] items;
    }

    private void Awake()
    {
        Instance = this;
        ItemDatabase.Load();
        Load();
    }

    private void OnApplicationQuit() => Save();

    private void OnApplicationPause(bool pause)
    {
        if (pause) Save();
    }

    public void AddItem(long itemId)
    {
        items.Add(new OwnedItem(itemId));
        OnInventoryChanged?.Invoke();
        Save();
    }

    public OwnedItem GetEquipped(EquipSlotType slot)
        => equipped.GetValueOrDefault(slot);

    public void Equip(OwnedItem item)
    {
        var slot = item.Data.SlotType;

        if (equipped.TryGetValue(slot, out var current))
            current.isEquipped = false;

        item.isEquipped = true;
        equipped[slot] = item;
        OnInventoryChanged?.Invoke();
        Save();
    }

    public void Unequip(EquipSlotType slot)
    {
        if (!equipped.TryGetValue(slot, out var item))
            return;

        item.isEquipped = false;
        equipped.Remove(slot);
        OnInventoryChanged?.Invoke();
        Save();
    }

    //단일 레벨업
    public bool TryLevelUp(OwnedItem item)
    {
        if (item == null || !item.CanLevelUp)
            return false;

        var cost = item.NextLevelUpCost;
        if (gold < cost)
            return false;

        gold -= cost;
        item.level++;
        OnInventoryChanged?.Invoke();
        Save();
        return true;
    }

    // 일괄 레벨업
    public int BatchLevelUp(OwnedItem item)
    {
        var levelsGained = 0;
        while (TryLevelUp(item))
            levelsGained++;
        return levelsGained;
    }

    public int GetTotalStat(Func<OwnedItem, int> selector)
        => equipped.Values.Sum(selector);


    public void Save()
    {
        var data = new SaveData { gold = gold, items = items.ToArray() };
        var json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }


    public void Load()
    {
        var json = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (string.IsNullOrEmpty(json))
            return;

        var data = JsonUtility.FromJson<SaveData>(json);
        if (data == null)
            return;

        gold = data.gold;

        items.Clear();
        equipped.Clear();

        if (data.items == null)
            return;

        foreach (var item in data.items)
        {
            items.Add(item);
            if (item.isEquipped)
                equipped[item.Data.SlotType] = item;
        }
    }

    // 테스트
    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        items.Clear();
        equipped.Clear();
        OnInventoryChanged?.Invoke();
    }

    [ContextMenu("골드 100000 추가")]
    public void DebugAddGold()
    {
        gold += 100000;
        OnInventoryChanged?.Invoke();
        Save();
    }
}
