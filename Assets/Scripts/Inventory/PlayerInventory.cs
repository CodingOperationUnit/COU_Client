using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private readonly List<OwnedItem> items = new();
    private readonly Dictionary<EquipSlotType, OwnedItem> equipped = new();

    // 골드와 보석을 PlayerSaveData에서 읽어오도록 구현
    public int Gold => GameManager.PlayerData.currentData.gold;
    public int Gem => GameManager.PlayerData.currentData.gem;

    public event Action OnInventoryChanged;

    public IReadOnlyList<OwnedItem> Items => items;

    private void Awake()
    {
        Instance = this;
        ItemDatabase.Load();

        if (!GameManager.PlayerData.isPlayerDataLoaded)
        {
            Debug.LogWarning("[PlayerInventory] 로그인 데이터가 없는 상태에서 실행되었습니다. " 
                             + "로그인 씬을 거치지 않은 테스트 씬인지 확인하세요.");
            return;
        }
        
        Load();
    }
    
    public OwnedItem AddItem(long itemId)
    {
        var item = new OwnedItem(itemId);
        items.Add(item);
        PersistAndNotify();
        return item;
    }

    public void AddGem(int amount)
    {
        GameManager.PlayerData.currentData.gem += amount;
        PersistAndNotify();
    }

    public void AddGold(int amount)
    {
        GameManager.PlayerData.currentData.gold += amount;
        PersistAndNotify();
    }

    public bool TrySpendGem(int amount)
    {
        if (Gem < amount)
            return false;

        GameManager.PlayerData.currentData.gold -= amount;
        PersistAndNotify();
        return true;
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
        PersistAndNotify();
    }

    public void Unequip(EquipSlotType slot)
    {
        if (!equipped.TryGetValue(slot, out var item))
            return;

        item.isEquipped = false;
        equipped.Remove(slot);
        PersistAndNotify();
    }

    //단일 레벨업
    public bool TryLevelUp(OwnedItem item)
    {
        if (item == null || !item.CanLevelUp)
            return false;

        var cost = item.NextLevelUpCost;
        if (Gold < cost)
            return false;

        GameManager.PlayerData.currentData.gold -= cost;
        item.level++;
        PersistAndNotify();
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

    // 합성 재료
    public int CountSynthesisMaterials(OwnedItem item)
        => items.Count(i => i != item && i.itemId == item.itemId && i.Grade == item.Grade);

    public bool CanSynthesize(OwnedItem item)
        => item != null
        && item.CanSynthesize
        && CountSynthesisMaterials(item) >= ItemLevelConfig.SynthesisMaterialCount;


    // 단일 합성 (같은 재료 두개)
    public bool TrySynthesize(OwnedItem item)
    {
        if (!CanSynthesize(item))
            return false;

        var materials = items
            .Where(i => i != item && i.itemId == item.itemId && i.Grade == item.Grade)
            .Take(ItemLevelConfig.SynthesisMaterialCount)
            .ToList();

        if (materials.Count < ItemLevelConfig.SynthesisMaterialCount)
            return false;

        foreach (var material in materials)
        {
            if (material.isEquipped)
                Unequip(material.Data.SlotType);

            items.Remove(material);
        }

        item.grade = item.NextGrade;
        OnInventoryChanged?.Invoke();
        Save();
        return true;
    }

    // 일괄 합성
    public int BatchSynthesize(OwnedItem item)
    {
        var tiersGained = 0;
        while (TrySynthesize(item))
            tiersGained++;
        return tiersGained;
    }

    public int GetTotalStat(Func<OwnedItem, int> selector)
        => equipped.Values.Sum(selector);


    // OwnedItem 리스트 -> PlayerSaveData,equipmentList로 되돌려서 계정 JSON에 저장
    public void Save()
    {
        GameManager.PlayerData.currentData.equipmentList = items.Select(item => new EquipmentSaveData()
        {
            instanceId = item.instanceId,
            itemId = item.itemId,
            level = item.level,
            isEquipped = item.isEquipped,
            grade = item.grade
        }).ToList();

        GameManager.LocalSaveLoad.SaveCurrentPlayerData();
    }

    public void Load()
    {
        items.Clear();
        equipped.Clear();

        var savedList = GameManager.PlayerData.currentData.equipmentList;
        if (savedList == null) return;

        foreach (var saved in savedList)
        {
            var item = new OwnedItem(saved.itemId)
            {
                instanceId = saved.instanceId,
                level = saved.level,
                isEquipped = saved.isEquipped,
                grade = saved.grade
            };
            
            items.Add(item);

            if (item.isEquipped)
                equipped[item.Data.SlotType] = item;
        }
    }

    [ContextMenu("인벤토리 초기화")]
    public void ClearSave()
    {
        items.Clear();
        equipped.Clear();
        PersistAndNotify();
    }

    [ContextMenu("골드/보석 초기화")]
    public void ResetCurrency()
    {
        GameManager.PlayerData.currentData.gold = 0;
        GameManager.PlayerData.currentData.gem = 0;
        PersistAndNotify();
    }
    
    private void PersistAndNotify()
    {
        Save();
        OnInventoryChanged?.Invoke();
        GameManager.PlayerData.NotifyPlayerDataChanged();
    }
}
