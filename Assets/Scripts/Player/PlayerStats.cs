using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Equipped (Dummy) - 인벤토리가 없는 씬에서만 사용")]
    [SerializeField] private List<DummyEquipment> dummyEquipments = new();

    private bool isCalculated;
    private int finalAtk;
    private int finalHp;
    private bool hasWeapon;
    private WeaponType equippedWeaponType;
    private string equippedWeaponName;
    private int startingSkillId;
    private bool usesInventory;

    private static PlayerBaseStatData Base => PlayerDatabase.BaseStats;

    public int FinalAtk { get { EnsureCalculated(); return finalAtk; } }
    public int FinalHp { get { EnsureCalculated(); return finalHp; } }
    public bool HasWeapon { get { EnsureCalculated(); return hasWeapon; } }
    public WeaponType EquippedWeaponType { get { EnsureCalculated(); return equippedWeaponType; } }
    public string EquippedWeaponName { get { EnsureCalculated(); return equippedWeaponName; } }
    public int StartingSkillId { get { EnsureCalculated(); return startingSkillId; } }   // 0이면 시작 스킬 없음
    public bool UsesInventory { get { EnsureCalculated(); return usesInventory; } }

    public int CriticalDamage => Base.criticalDamage;
    public int CriticalChance => Base.criticalChance;
    public int SkillDamage => Base.skillDamage;
    public float Speed => Base.moveSpeed;
    public float MaxSpeed => Base.maxMoveSpeed;
    public float LootRadius => Base.lootRadius;

    private void Start()
    {
        EnsureCalculated();
    }

    private void EnsureCalculated()
    {
        if (isCalculated) return;
        isCalculated = true;

        startingSkillId = WeaponSkillTable.NoSkill;

        if (PlayerInventory.Instance != null)
            CalculateFromInventory();
        else
            CalculateFromDummy();

        Debug.Log("[PlayerStats] 장비 출처: " + (usesInventory ? "인벤토리" : "더미") +
                  " / 기본 Atk: " + Base.attack + ", 기본 Hp: " + Base.hp +
                  " / 최종 Atk: " + finalAtk + ", 최종 Hp: " + finalHp +
                  " / 무기: " + (hasWeapon ? equippedWeaponName + " (" + equippedWeaponType + ")" : "없음") +
                  " / 시작 스킬: " + (startingSkillId == WeaponSkillTable.NoSkill ? "없음" : startingSkillId.ToString()));
    }

    private void CalculateFromInventory()
    {
        usesInventory = true;
        var inventory = PlayerInventory.Instance;

        int totalAtk = inventory.GetTotalStat(item => item.ScaledAttack);
        int totalHp = inventory.GetTotalStat(item => item.ScaledHp);

        // 장비 데이터에 공격력·체력 보너스 % 필드가 없어 0%로 계산 (필드 추가 시 교체)
        finalAtk = CalculateStat(Base.attack, totalAtk, 0);
        finalHp = CalculateStat(Base.hp, totalHp, 0);

        var weapon = inventory.GetEquipped(EquipSlotType.Weapon);
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
        usesInventory = false;

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

        finalAtk = CalculateStat(Base.attack, totalAtk, totalAtkBonus);
        finalHp = CalculateStat(Base.hp, totalHp, totalHpBonus);
    }

    private int CalculateStat(int baseStat, int addStat, int bonusPercent)
    {
        return (baseStat + addStat) * (100 + bonusPercent) / 100;
    }
}