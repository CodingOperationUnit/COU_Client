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
    
    // // 골드 테스트용
    // [SerializeField] private int gold = 500000;
    // public int Gold => gold;
    //
    // // 보석 테스트용
    // [SerializeField] private int gem = 0;
    // public int Gem => gem;

    public event Action OnInventoryChanged;

    public IReadOnlyList<OwnedItem> Items => items;


    // private const string SaveFileName = "inventory_save.json";
    // private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    // [Serializable]
    // private class SaveData
    // {
    //     public int gold;
    //     public int gem;
    //     public OwnedItem[] items;
    // }

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

    private void Start()
    {
        ClaimBattleResult();
    }

    // 직전 전투 결과(골드, 계정 경험치, 보상상자)를 지급하고 비운다
    private void ClaimBattleResult()
    {
        var result = BattleResult.Last;
        if (result == null || !GameManager.PlayerData.isPlayerDataLoaded)
            return;

        BattleResult.Last = null;

        var data = GameManager.PlayerData.currentData;
        data.gold += result.Gold;
        data.accountExp += result.AccountExp;

        var record = data.stageRecordList.FirstOrDefault(r => r.stageID == result.StageID);
        if (record == null)
        {
            record = new StageRecordSaveData { stageID = result.StageID };
            data.stageRecordList.Add(record);
        }
        record.isCleared |= result.Victory;
        record.bestSurvivalSeconds = Mathf.Max(record.bestSurvivalSeconds, result.Seconds);

        // 보상상자: 최저 등급 장비를 상자 개수만큼 무작위 지급
        var rewards = new List<OwnedItem>();
        var pool = ItemDatabase.GetAll().Where(item => item.Grade == ItemGrade.General).ToList();
        if (pool.Count > 0)
        {
            for (var i = 0; i < result.RewardBoxes; i++)
                rewards.Add(new OwnedItem(pool[UnityEngine.Random.Range(0, pool.Count)].itemId));
        }
        items.AddRange(rewards);

        PersistAndNotify();

        if (rewards.Count > 0)
            UIManager.Instance.Get<RewardBoxResultPopup>().Show(rewards);
    }

    // private void OnApplicationQuit() => Save();
    //
    // private void OnApplicationPause(bool pause)
    // {
    //     if (pause) Save();
    // }
    
    public OwnedItem AddItem(long itemId)
    {
        var item = new OwnedItem(itemId);
        items.Add(item);
        // OnInventoryChanged?.Invoke();
        // Save();
        PersistAndNotify();
        return item;
    }

    public void AddGem(int amount)
    {
        // gem += amount;
        // OnInventoryChanged?.Invoke();
        // Save();

        GameManager.PlayerData.currentData.gem += amount;
        PersistAndNotify();
    }

    public void AddGold(int amount)
    {
        // gold += amount;
        // OnInventoryChanged?.Invoke();
        // Save();

        GameManager.PlayerData.currentData.gold += amount;
        PersistAndNotify();
    }

    public bool TrySpendGem(int amount)
    {
        if (Gem < amount)
            return false;

        GameManager.PlayerData.currentData.gold -= amount;
        // OnInventoryChanged?.Invoke();
        // Save();
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
        // OnInventoryChanged?.Invoke();
        // Save();
        PersistAndNotify();
    }

    public void Unequip(EquipSlotType slot)
    {
        if (!equipped.TryGetValue(slot, out var item))
            return;

        item.isEquipped = false;
        equipped.Remove(slot);
        // OnInventoryChanged?.Invoke();
        // Save();
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
        // OnInventoryChanged?.Invoke();
        // Save();
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

    public int GetTotalStat(Func<OwnedItem, int> selector)
        => equipped.Values.Sum(selector);


    // OwnedItem 리스트 -> PlayerSaveData,equipmentList로 되돌려서 계정 JSON에 저장
    public void Save()
    {
        // var data = new SaveData { gold = gold, gem = gem, items = items.ToArray() };
        // var json = JsonUtility.ToJson(data, true);
        // File.WriteAllText(SavePath, json);

        GameManager.PlayerData.currentData.equipmentList = items.Select(item => new EquipmentSaveData()
        {
            instanceId = item.instanceId,
            itemId = item.itemId,
            level = item.level,
            isEquipped = item.isEquipped
        }).ToList();

        GameManager.LocalSaveLoad.SaveCurrentPlayerData();
    }

    public void Load()
    {
        // if (!File.Exists(SavePath))
        //     return;
        //
        // var json = File.ReadAllText(SavePath);
        // if (string.IsNullOrEmpty(json))
        //     return;
        //
        // var data = JsonUtility.FromJson<SaveData>(json);
        // if (data == null)
        //     return;
        //
        // gold = data.gold;
        // gem = data.gem;
        //
        // items.Clear();
        // equipped.Clear();
        //
        // if (data.items == null)
        //     return;
        //
        // foreach (var item in data.items)
        // {
        //     items.Add(item);
        //     if (item.isEquipped)
        //         equipped[item.Data.SlotType] = item;
        // }
        
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
                isEquipped = saved.isEquipped
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
