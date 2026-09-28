using UnityEngine;

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


    [Header("Test Equipment Totals")]
    [SerializeField] private int equipmentsTotalAtkStat = 200;
    [SerializeField] private int equipmentsTotalAtkBonusStat = 20;
    [SerializeField] private int equipmentsTotalHpStat = 1050;
    [SerializeField] private int equipmentsTotalHpBonusStat = 10;

    public int FinalAtk { get; private set; }
    public int FinalHp { get; private set; }


    public int CriticalDamage => criticalDamage;
    public int SkillDamage => skillDamage;
    public float Speed => speed;
    public float MaxSpeed => maxSpeed;

    private void Awake()
    {
        FinalAtk = CalculateStat(atk, equipmentsTotalAtkStat, equipmentsTotalAtkBonusStat);
        FinalHp = CalculateStat(hp, equipmentsTotalHpStat, equipmentsTotalHpBonusStat);

        Debug.Log("Atk: " + FinalAtk + ", Hp: " + FinalHp);
    }

    private int CalculateStat(int defStat, int addStat, int bonusStat)
    {
        return ((defStat + addStat) * (100 + bonusStat) / 100);
    }
}
