using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private readonly List<OwnedItem> items = new();
    private readonly Dictionary<EquipSlotType, OwnedItem> equipped = new();

    // OwnedItem.instanceId(메모리용 GUID) → InventoryData 저장용 값
    private readonly Dictionary<string, int> inventoryIds = new();
    private readonly Dictionary<string, DateTime> acquiredAts = new();

    private static PlayerSaveData Data => GameManager.PlayerData.currentData;

    // 재화를 PlayerSaveData.currency에서 읽어오도록 구현
    public int Gold => Data.currency.currencyGold;
    public int Gem => Data.currency.currencyGem;
    public int Stamina => Data.currency.currencyEnergy;

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

    private void Start()
    {
        ClaimBattleResult();
    }

    // 직전 전투 결과(골드, 계정 경험치, 스테이지 진행, 보상상자)를 지급하고 비운다
    private void ClaimBattleResult()
    {
        var result = BattleResult.Last;
        if (result == null || !GameManager.PlayerData.isPlayerDataLoaded)
            return;

        BattleResult.Last = null;

        var data = Data;
        data.currency.currencyGold += result.Gold;
        data.profile.accountExp += result.AccountExp;

        // 계정 레벨업: 필요 경험치를 채울 때마다 차감하고 레벨을 올린다. 최대 레벨에서 멈춘다
        var accountConst = GameManager.JsonData.AccountConstData;
        while (data.profile.accountLevel < accountConst.maxAccountLevel
               && data.profile.accountExp >= accountConst.GetRequiredExp(data.profile.accountLevel))
        {
            data.profile.accountExp -= accountConst.GetRequiredExp(data.profile.accountLevel);
            data.profile.accountLevel++;
        }

        // 스테이지 진행: 순서대로 해금되므로 최고 클리어 스테이지 하나만 갱신 (데이터 공통 규칙 9번)
        var progress = data.stageProgress;
        progress.currentStageId = result.StageId;
        if (result.Victory
            && (!progress.maxClearedStageId.HasValue || result.StageId > progress.maxClearedStageId.Value))
        {
            progress.maxClearedStageId = result.StageId;
        }
        // bestSurvivalSeconds: 데이터 정의서에 필드가 없어 저장하지 않음 (팀 결정 대기)

        // 보상상자: 상자마다 스테이지의 등급 가중치로 등급을 뽑고, 그 등급이 기본 등급인 장비를 무작위 지급
        var rewards = new List<OwnedItem>();
        var stage = GameManager.JsonData.GetStageDataFromJson(result.StageId);
        for (var i = 0; i < result.RewardBoxes; i++)
        {
            var grade = stage.RollRewardBoxGrade();
            var pool = ItemDatabase.GetAll().Where(item => item.Grade == grade).ToList();
            if (pool.Count > 0)
                rewards.Add(CreateNewItem(pool[UnityEngine.Random.Range(0, pool.Count)].itemId));
        }
        items.AddRange(rewards);

        PersistAndNotify();

        if (rewards.Count > 0)
            UIManager.Instance.Get<RewardBoxResultPopup>().Show(rewards);
    }

    public OwnedItem AddItem(long itemId)
    {
        var item = CreateNewItem(itemId);
        items.Add(item);
        PersistAndNotify();
        return item;
    }

    public void AddGem(int amount)
    {
        Data.currency.currencyGem += amount;
        PersistAndNotify();
    }

    public void AddGold(int amount)
    {
        Data.currency.currencyGold += amount;
        PersistAndNotify();
    }

    public bool TrySpendGem(int amount)
    {
        if (Gem < amount)
            return false;

        // 기존 코드는 gold를 차감하던 버그가 있어 gem 차감으로 수정
        Data.currency.currencyGem -= amount;
        PersistAndNotify();
        return true;
    }

    // 스태미나 = 데이터 정의서의 currencyEnergy
    public bool TrySpendStamina(int amount)
    {
        if (Data.currency.currencyEnergy < amount)
            return false;

        Data.currency.currencyEnergy -= amount;
        Data.currency.currencyEnergyUpdatedAt = DateTime.Now;
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

        Data.currency.currencyGold -= cost;
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

    // 합성 재료: 같은 아이템, 같은 등급, 장착하지 않은 장비 (서버 findMaterials와 동일)
    public int CountSynthesisMaterials(OwnedItem item)
        => items.Count(i => i != item && !i.isEquipped && i.itemId == item.itemId && i.Grade == item.Grade);

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
            inventoryIds.Remove(material.instanceId);
            acquiredAts.Remove(material.instanceId);
        }

        // 주의: Inventory에 등급 필드가 없어 합성 등급은 재로그인 시 기본 등급으로 돌아간다 (팀 결정 대기)
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

    // OwnedItem 리스트 → PlayerSaveData.inventoryList + profile 장착 칸으로 변환해 JSON에 저장
    public void Save()
    {
        var data = Data;
        long playerId = data.profile.playerId;

        data.inventoryList = items.Select(item => new InventoryData
        {
            inventoryId = inventoryIds[item.instanceId],
            playerId = playerId,
            itemId = (int)item.itemId,
            inventoryItemLevel = item.level,
            inventoryAcquiredAt = acquiredAts[item.instanceId],
            inventoryItemGrade = item.grade
        }).ToList();

        foreach (EquipSlotType slot in Enum.GetValues(typeof(EquipSlotType)))
        {
            int? inventoryId = equipped.TryGetValue(slot, out var item) ? inventoryIds[item.instanceId] : null;
            SetEquippedInventoryId(data.profile, slot, inventoryId);
        }

        // GameManager.LocalSaveLoad.SaveCurrentPlayerData();
    }

    // PlayerSaveData.inventoryList + profile 장착 칸 → OwnedItem 리스트로 복원
    public void Load()
    {
        items.Clear();
        equipped.Clear();
        inventoryIds.Clear();
        acquiredAts.Clear();

        var data = Data;
        if (data.inventoryList == null) return;

        var byInventoryId = new Dictionary<int, OwnedItem>();

        foreach (var saved in data.inventoryList)
        {
            if (!IsKnownItem(saved.itemId))
            {
                Debug.LogWarning($"[PlayerInventory] 아이템 데이터에 없는 itemId {saved.itemId}는 건너뜁니다.");
                continue;
            }

            var item = new OwnedItem(saved.itemId)
            {
                level = saved.inventoryItemLevel
            };

            // 저장된 등급이 있으면 복원 (없으면 생성자에서 정한 아이템 기본 등급 유지)
            if (saved.inventoryItemGrade.HasValue)
                item.grade = saved.inventoryItemGrade.Value;

            items.Add(item);
            inventoryIds[item.instanceId] = saved.inventoryId;
            acquiredAts[item.instanceId] = saved.inventoryAcquiredAt;
            byInventoryId[saved.inventoryId] = item;
        }

        foreach (EquipSlotType slot in Enum.GetValues(typeof(EquipSlotType)))
        {
            var inventoryId = GetEquippedInventoryId(data.profile, slot);
            if (!inventoryId.HasValue || !byInventoryId.TryGetValue(inventoryId.Value, out var item))
                continue;

            // 장착 칸과 슬롯 타입이 다르면 무시 (테이블 관계 정리 3번 제약)
            if (item.Data.SlotType != slot)
                continue;

            item.isEquipped = true;
            equipped[slot] = item;
        }
    }

    [ContextMenu("인벤토리 초기화")]
    public void ClearSave()
    {
        items.Clear();
        equipped.Clear();
        inventoryIds.Clear();
        acquiredAts.Clear();
        PersistAndNotify();
    }

    [ContextMenu("골드/보석 초기화")]
    public void ResetCurrency()
    {
        Data.currency.currencyGold = 0;
        Data.currency.currencyGem = 0;
        PersistAndNotify();
    }

    private void PersistAndNotify()
    {
        Save();
        OnInventoryChanged?.Invoke();
        GameManager.PlayerData.NotifyPlayerDataChanged();
    }

    // 새로 얻은 장비에 로컬 inventoryId를 부여 (서버 연동 시 서버의 Auto Increment로 대체)
    private OwnedItem CreateNewItem(long itemId)
    {
        var item = new OwnedItem(itemId);
        int nextId = inventoryIds.Count == 0 ? 1 : inventoryIds.Values.Max() + 1;

        inventoryIds[item.instanceId] = nextId;
        acquiredAts[item.instanceId] = DateTime.Now;
        return item;
    }

    private static bool IsKnownItem(long itemId)
        => ItemDatabase.GetAll().Any(data => data.itemId == itemId);

    // PlayerProfile의 슬롯별 장착 칸 읽기/쓰기 (PlayerStats에서도 사용)
    public static int? GetEquippedInventoryId(PlayerProfileData profile, EquipSlotType slot) => slot switch
    {
        EquipSlotType.Weapon => profile.equippedWeaponInventoryId,
        EquipSlotType.Armor => profile.equippedArmorInventoryId,
        EquipSlotType.Belt => profile.equippedBeltInventoryId,
        EquipSlotType.Gloves => profile.equippedGlovesInventoryId,
        EquipSlotType.Necklace => profile.equippedNecklaceInventoryId,
        EquipSlotType.Shoes => profile.equippedShoesInventoryId,
        _ => null
    };

    private static void SetEquippedInventoryId(PlayerProfileData profile, EquipSlotType slot, int? inventoryId)
    {
        switch (slot)
        {
            case EquipSlotType.Weapon: profile.equippedWeaponInventoryId = inventoryId; break;
            case EquipSlotType.Armor: profile.equippedArmorInventoryId = inventoryId; break;
            case EquipSlotType.Belt: profile.equippedBeltInventoryId = inventoryId; break;
            case EquipSlotType.Gloves: profile.equippedGlovesInventoryId = inventoryId; break;
            case EquipSlotType.Necklace: profile.equippedNecklaceInventoryId = inventoryId; break;
            case EquipSlotType.Shoes: profile.equippedShoesInventoryId = inventoryId; break;
        }
    }
    
    // ===== 서버 응답 반영: 서버가 계산·저장한 결과로 덮어쓴다 (클라에서 계산하지 않음) =====

    public long GetInventoryId(OwnedItem item)
        => item != null && inventoryIds.TryGetValue(item.instanceId, out var id) ? id : 0;
    
    public void ApplyEquip(EquipResponse response)
    {
        if (response.unequipped != null)
            SetEquippedState(response.unequipped.inventoryId, false);
    
        if (response.equipped != null)
            SetEquippedState(response.equipped.inventoryId, response.equipped.isEquipped);
    
        SyncAndNotify();
    }
    
    public void ApplyUnequip(EquipmentResponse response)
    {
        SetEquippedState(response.inventoryId, response.isEquipped);
        SyncAndNotify();
    }
    
    public void ApplyLevelUp(long inventoryId, int level, int currencyGold)
    {
        var item = FindByInventoryId(inventoryId);
        if (item != null)
            item.level = level;
    
        Data.currency.currencyGold = currencyGold;   // 빼기가 아니라 서버 값으로 덮어쓰기
        SyncAndNotify();
    }
    
    public void ApplySynthesis(SynthesizeResponse response)
    {
        // 재료는 서버가 고른 것만 제거
        if (response.consumedInventoryIds != null)
        {
            foreach (var consumedId in response.consumedInventoryIds)
                RemoveByInventoryId(consumedId);
        }
    
        var item = FindByInventoryId(response.inventoryId);
        if (item != null && response.inventoryItemGrade.HasValue)
            item.grade = response.inventoryItemGrade.Value;
    
        SetEquippedState(response.inventoryId, response.isEquipped);
        SyncAndNotify();
    }
    
    private OwnedItem FindByInventoryId(long inventoryId)
        => items.FirstOrDefault(i => inventoryIds.TryGetValue(i.instanceId, out var id) && id == inventoryId);
    
    private void SetEquippedState(long inventoryId, bool isEquipped)
    {
        var item = FindByInventoryId(inventoryId);
        if (item == null)
        {
            Debug.LogWarning($"[PlayerInventory] inventoryId {inventoryId} 장비를 찾지 못했습니다.");
            return;
        }
    
        var slot = item.Data.SlotType;
    
        if (isEquipped)
        {
            if (equipped.TryGetValue(slot, out var current) && current != item)
                current.isEquipped = false;
    
            item.isEquipped = true;
            equipped[slot] = item;
        }
        else
        {
            item.isEquipped = false;
            if (equipped.TryGetValue(slot, out var current) && current == item)
                equipped.Remove(slot);
        }
    }
    
    private void RemoveByInventoryId(long inventoryId)
    {
        var item = FindByInventoryId(inventoryId);
        if (item == null) return;
    
        if (item.isEquipped)
            SetEquippedState(inventoryId, false);
    
        items.Remove(item);
        inventoryIds.Remove(item.instanceId);
        acquiredAts.Remove(item.instanceId);
    }
    
    // 서버가 이미 저장했으므로 PlayerSaveData만 맞추고 화면을 갱신한다
    private void SyncAndNotify()
    {
        Save();   // OwnedItem → PlayerSaveData.inventoryList + 장착 칸 (전투 씬 PlayerStats가 이 값을 읽음)
        OnInventoryChanged?.Invoke();
        GameManager.PlayerData.NotifyPlayerDataChanged();
    }
}