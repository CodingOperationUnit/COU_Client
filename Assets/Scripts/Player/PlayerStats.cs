using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Equipped (Dummy) - 로그인 없이 실행한 테스트에서만 사용")]
    [SerializeField] private List<DummyEquipment> dummyEquipments = new();

    private bool isCalculated;
    private int finalAtk;
    private int finalHp;
    private bool hasWeapon;
    private WeaponType equippedWeaponType;
    private string equippedWeaponName;
    private int startingSkillId;
    private string equipmentSource;

    private static PlayerBaseStatData Base => GameManager.JsonData.PlayerBaseStatData;

    public int FinalAtk { get { EnsureCalculated(); return finalAtk; } }
    public int FinalHp { get { EnsureCalculated(); return finalHp; } }
    public bool HasWeapon { get { EnsureCalculated(); return hasWeapon; } }
    public WeaponType EquippedWeaponType { get { EnsureCalculated(); return equippedWeaponType; } }
    public string EquippedWeaponName { get { EnsureCalculated(); return equippedWeaponName; } }
    public int StartingSkillId { get { EnsureCalculated(); return startingSkillId; } }   // 0이면 시작 스킬 없음
    public string EquipmentSource { get { EnsureCalculated(); return equipmentSource; } } // 인벤토리 / 계정 데이터 / 더미
    public bool UsesInventory => EquipmentSource != "더미";

    public int CriticalDamage => Base.playerBaseCriticalDamage;
    public int CriticalChance => Base.playerBaseCriticalChance;
    public int SkillDamage => Base.playerBaseSkillDamage;
    public float Speed => Base.playerBaseMoveSpeed;
    public float MaxSpeed => Base.playerBaseMaxMoveSpeed;
    public float LootRadius => Base.playerBaseLootRadius;

    private void Start()
    {
        EnsureCalculated();
    }

    private void EnsureCalculated()
    {
        if (isCalculated) return;
        isCalculated = true;

        startingSkillId = WeaponSkillTable.NoSkill;

        if (IsPlayerDataLoaded() && PlayerInventory.Instance != null)
        {
            equipmentSource = "인벤토리";
            CalculateFromEquipped(GetEquippedFromInventory(PlayerInventory.Instance));
        }
        else if (IsPlayerDataLoaded())
        {
            equipmentSource = "계정 데이터";
            CalculateFromEquipped(GetEquippedFromSaveData(GameManager.PlayerData.currentData));
        }
        else
        {
            equipmentSource = "더미";
            CalculateFromDummy();
        }

        Debug.Log("[PlayerStats] 장비 출처: " + equipmentSource +
                  " / 기본 Atk: " + Base.playerBaseAttack + ", 기본 Hp: " + Base.playerBaseHp +
                  " / 최종 Atk: " + finalAtk + ", 최종 Hp: " + finalHp +
                  " / 무기: " + (hasWeapon ? equippedWeaponName + " (" + equippedWeaponType + ")" : "없음") +
                  " / 시작 스킬: " + (startingSkillId == WeaponSkillTable.NoSkill ? "없음" : startingSkillId.ToString()));
    }

    private static bool IsPlayerDataLoaded()
    {
        var dataManager = GameManager.PlayerData;
        return dataManager != null && dataManager.isPlayerDataLoaded;
    }

    // 메인·테스트 씬: 인벤토리에서 부위별 장착 장비를 모은다
    private static List<OwnedItem> GetEquippedFromInventory(PlayerInventory inventory)
    {
        var result = new List<OwnedItem>();

        foreach (EquipSlotType slot in Enum.GetValues(typeof(EquipSlotType)))
        {
            var item = inventory.GetEquipped(slot);
            if (item != null)
                result.Add(item);
        }

        return result;
    }

    // 전투 씬: profile의 장착 칸이 가리키는 장비만 OwnedItem으로 복원 (PlayerInventory.Load와 같은 방식)
    private static List<OwnedItem> GetEquippedFromSaveData(PlayerSaveData data)
    {
        var result = new List<OwnedItem>();
        if (data.profile == null || data.inventoryList == null) return result;

        var byInventoryId = data.inventoryList.ToDictionary(saved => saved.inventoryId);

        foreach (EquipSlotType slot in Enum.GetValues(typeof(EquipSlotType)))
        {
            var inventoryId = PlayerInventory.GetEquippedInventoryId(data.profile, slot);
            if (!inventoryId.HasValue || !byInventoryId.TryGetValue(inventoryId.Value, out var saved))
                continue;

            var item = new OwnedItem(saved.itemId)
            {
                level = saved.inventoryItemLevel,
                isEquipped = true
            };

            if (saved.inventoryItemGrade.HasValue)
                item.grade = saved.inventoryItemGrade.Value;

            if (item.Data.SlotType != slot)
                continue;

            result.Add(item);
        }

        return result;
    }

    private void CalculateFromEquipped(List<OwnedItem> equipped)
    {
        int totalAtk = equipped.Sum(item => item.ScaledAttack);
        int totalHp = equipped.Sum(item => item.ScaledHp);

        // 장비 데이터에 공격력·체력 보너스 % 필드가 없어 0%로 계산 (필드 추가 시 교체)
        finalAtk = CalculateStat(Base.playerBaseAttack, totalAtk, 0);
        finalHp = CalculateStat(Base.playerBaseHp, totalHp, 0);

        var weapon = equipped.Find(item => item.Data.SlotType == EquipSlotType.Weapon);
        hasWeapon = weapon != null;
        if (!hasWeapon) return;

        equippedWeaponName = weapon.Data.itemName;
        startingSkillId = WeaponSkillTable.GetSkillByItem(weapon.itemId);

        if (!WeaponSkillTable.TryGetTypeBySkill(startingSkillId, out equippedWeaponType))
        {
            equippedWeaponType = WeaponType.Blunt;
            Debug.LogWarning("[PlayerStats] 무기 매핑이 없습니다: " + equippedWeaponName + " (" + weapon.itemId + "). 스킬 없이 시작, 무기 종류는 Blunt");
        }
    }

    private void CalculateFromDummy()
    {
        int totalAtk = 0;
        int totalAtkBonus = 0;
        int totalHp = 0;
        int totalHpBonus = 0;

        HashSet<EquipSlotType> usedSlots = new();

        foreach (var item in dummyEquipments)
        {
            if (!usedSlots.Add(item.slot))
            {
                Debug.LogWarning("[PlayerStats] 중복된 부위 장비 무시: " + item.name + " (" + item.slot + ")");
                continue;
            }

            if (item.slot == EquipSlotType.Weapon)
            {
                hasWeapon = true;
                equippedWeaponName = item.name;
                equippedWeaponType = item.weaponType;
                startingSkillId = WeaponSkillTable.GetSkillByType(item.weaponType);
            }

            totalAtk += item.atk;
            totalAtkBonus += item.atkBonusPercent;
            totalHp += item.hp;
            totalHpBonus += item.hpBonusPercent;
        }

        finalAtk = CalculateStat(Base.playerBaseAttack, totalAtk, totalAtkBonus);
        finalHp = CalculateStat(Base.playerBaseHp, totalHp, totalHpBonus);
    }

    private int CalculateStat(int baseStat, int addStat, int bonusPercent)
    {
        return (baseStat + addStat) * (100 + bonusPercent) / 100;
    }
}