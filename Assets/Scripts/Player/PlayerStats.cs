using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField][Min(1)] private int atk = 100;    // 임시 스탯
    [SerializeField][Min(1)] private int hp = 1000;    // 임시 스탯

    // 아래 치명타, 스킬 피해, 이동 속도, 이동 속도 상한은 임시 값이다.
    [SerializeField][Min(1)] private int criticalDamage = 200;
    [SerializeField][Range(0, 100)] private int criticalChance = 5;
    [SerializeField][Min(1)] private int skillDamage = 100;
    [SerializeField][Min(1f)] private float speed = 9f;
    [SerializeField][Min(1f)] private float maxSpeed = 16f;

    [Header("Equipped (Dummy) - 인벤토리가 없는 씬에서만 사용")]
    [SerializeField] private List<DummyEquipment> dummyEquipments = new();

    [Header("Inventory Temp")]
    // 임시: ItemData에 무기 종류 필드가 추가되기 전까지 인벤토리 무기에 사용할 종류
    [SerializeField] private WeaponType inventoryWeaponType = WeaponType.Blunt;

    private bool isCalculated;
    private int finalAtk;
    private int finalHp;
    private bool hasWeapon;
    private WeaponType equippedWeaponType;
    private string equippedWeaponName;
    private bool usesInventory;

    public int FinalAtk { get { EnsureCalculated(); return finalAtk; } }
    public int FinalHp { get { EnsureCalculated(); return finalHp; } }
    public bool HasWeapon { get { EnsureCalculated(); return hasWeapon; } }
    public WeaponType EquippedWeaponType { get { EnsureCalculated(); return equippedWeaponType; } }
    public string EquippedWeaponName { get { EnsureCalculated(); return equippedWeaponName; } }
    public bool UsesInventory { get { EnsureCalculated(); return usesInventory; } }

    public int CriticalDamage => criticalDamage;
    public int CriticalChance => criticalChance;
    public int SkillDamage => skillDamage;
    public float Speed => speed;
    public float MaxSpeed => maxSpeed;

    private void Start()
    {
        // 아무도 읽지 않았더라도 Start에서 한 번은 계산해 로그로 확인
        EnsureCalculated();
    }

    private void EnsureCalculated()
    {
        if (isCalculated) return;
        isCalculated = true;

        if (PlayerInventory.Instance != null)
            CalculateFromInventory();
        else
            CalculateFromDummy();

        Debug.Log("[PlayerStats] 장비 출처: " + (usesInventory ? "인벤토리" : "더미") +
                  " / Atk: " + finalAtk + ", Hp: " + finalHp +
                  " / 무기: " + (hasWeapon ? equippedWeaponName + " (" + equippedWeaponType + ")" : "없음"));
    }

    private void CalculateFromInventory()
    {
        usesInventory = true;
        var inventory = PlayerInventory.Instance;

        int totalAtk = inventory.GetTotalStat(item => item.ScaledAttack);
        int totalHp = inventory.GetTotalStat(item => item.ScaledHp);

        // 장비 데이터에 공격력·체력 보너스 % 필드가 없어 0%로 계산 (필드 추가 시 교체)
        finalAtk = CalculateStat(atk, totalAtk, 0);
        finalHp = CalculateStat(hp, totalHp, 0);

        var weapon = inventory.GetEquipped(EquipSlotType.Weapon);
        hasWeapon = weapon != null;
        equippedWeaponName = hasWeapon ? weapon.Data.itemName : null;
        equippedWeaponType = inventoryWeaponType;
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
            }

            totalAtk += item.atk;
            totalAtkBonus += item.atkBonusPercent;
            totalHp += item.hp;
            totalHpBonus += item.hpBonusPercent;
        }

        finalAtk = CalculateStat(atk, totalAtk, totalAtkBonus);
        finalHp = CalculateStat(hp, totalHp, totalHpBonus);
    }

    private int CalculateStat(int baseStat, int addStat, int bonusPercent)
    {
        return (baseStat + addStat) * (100 + bonusPercent) / 100;
    }
}