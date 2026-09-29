using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] [Min(1)] private int atk = 100; // 임시 스탯
    [SerializeField] [Min(1)] private int hp = 1000; // 임시 스탯

    // 아래 치명타 피해, 스킬 피해, 이동 속도, 이동 속도 상한은 임시 코드이다.
    [SerializeField][Min(1)] private int criticalDamage = 200;
    [SerializeField][Min(1)] private int skillDamage = 100;
    [SerializeField][Min(1f)] private float speed = 9f;
    [SerializeField][Min(1f)] private float maxSpeed = 16f;

    [Header("Equipped (Dummy)")]
    [SerializeField] private List<DummyEquipment> dummyEquipments = new();

    public int FinalAtk { get; private set; }
    public int FinalHp { get; private set; }


    public int CriticalDamage => criticalDamage;
    public int SkillDamage => skillDamage;
    public float Speed => speed;
    public float MaxSpeed => maxSpeed;

    private void Awake()
    {
        int totalAtk = 0;
        int totalAtkBonus = 0;
        int totalHp = 0;
        int totalHpBonus = 0;

        HashSet<EquipSlotType> check = new();

        foreach (var item in dummyEquipments)
        {
            if (!check.Add(item.slot))
            {
                Debug.LogWarning("중복된 장비");
                continue;
            }

            totalAtk += item.atk;
            totalAtkBonus += item.atkBonusPercent;
            totalHp += item.hp;
            totalHpBonus += item.hpBonusPercent;
        }

        FinalAtk = CalculateStat(atk, totalAtk, totalAtkBonus);
        FinalHp = CalculateStat(hp, totalHp, totalHpBonus);

        Debug.Log("Atk: " + FinalAtk + ", Hp: " + FinalHp);
    }

    private int CalculateStat(int defStat, int addStat, int bonusStat)
    {
        return ((defStat + addStat) * (100 + bonusStat) / 100);
    }
}
