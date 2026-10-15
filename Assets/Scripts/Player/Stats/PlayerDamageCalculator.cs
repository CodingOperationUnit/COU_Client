using System;
using UnityEngine;

// 플레이어가 주는 피해의 최종 계산 (공격력, 스킬 피해 스탯, 치명타)
// 스킬은 자기 몫인 "스킬 배율"(skillDamage × 레벨 배율 × (1 + 패시브%))만 넘기고, 맞힐 때 이 함수를 부른다
public static class PlayerDamageCalculator
{
    // true: 기본 공격력일 때 피해 = 스킬 배율 (몬스터 체력 기획 전까지)
    // false: 피해 = 공격력 × 스킬 배율 (몬스터 체력 기획 후 전환)
    public static readonly bool UseRelativeAttack = false;

    public readonly struct HitResult
    {
        public readonly int damage;
        public readonly bool isCritical;

        public HitResult(int damage, bool isCritical)
        {
            this.damage = damage;
            this.isCritical = isCritical;
        }
    }

    // 최종 피해 = 스킬 배율 × 공격력 기준값 × (스킬 피해% ÷ 100) → 치명타 → 반올림, 최소 1
    public static HitResult Calculate(float skillMultiplier, PlayerStats stats)
    {
        if (stats == null || skillMultiplier <= 0f)
            return new HitResult(0, false);

        float damage = GetBaseDamage(skillMultiplier, stats);

        bool isCritical = RollCritical(stats.CriticalChance);
        if (isCritical)
            damage *= stats.CriticalDamage / 100f;

        return new HitResult(Mathf.Max(1, Mathf.RoundToInt(damage)), isCritical);
    }

    // 치명타 판정 없이 일반·치명타 피해를 미리 계산 (로그·UI 표시용, Calculate와 같은 공식)
    public static (int normal, int critical) Preview(float skillMultiplier, PlayerStats stats)
    {
        if (stats == null || skillMultiplier <= 0f)
            return (0, 0);

        float damage = GetBaseDamage(skillMultiplier, stats);
        int normal = Mathf.Max(1, Mathf.RoundToInt(damage));
        int critical = Mathf.Max(1, Mathf.RoundToInt(damage * stats.CriticalDamage / 100f));
        return (normal, critical);
    }

    // 치명타 전 피해 (반올림 전)
    private static float GetBaseDamage(float skillMultiplier, PlayerStats stats)
        => skillMultiplier * GetAttackFactor(stats) * (stats.SkillDamage / 100f);

    private static float GetAttackFactor(PlayerStats stats)
    {
        if (!UseRelativeAttack)
            return stats.FinalAtk;

        int baseAttack = GameManager.JsonData.PlayerBaseStatData.playerBaseAttack;
        return baseAttack > 0 ? stats.FinalAtk / (float)baseAttack : 1f;
    }

    // 치명타 확률(%)로 판정. 0 이하면 없음, 100 이상이면 항상
    private static bool RollCritical(int chance)
    {
        if (chance <= 0) return false;
        if (chance >= 100) return true;
        return UnityEngine.Random.Range(0, 100) < chance;
    }

    // ===== 받는 피해 =====

    // 방어력 기준값: 방어력이 이 값이면 받는 피해가 절반
    public const int DefenseConstant = 100;

    // 피해가 들어왔을 때 최소로 받는 피해
    public const int MinReceivedDamage = 1;

    // 받는 피해 = 피해 × 기준값 ÷ (기준값 + 방어력), 반올림(0.5는 짝수 쪽), 최소 1
    public static int ApplyDefense(int damage, int defense)
    {
        if (damage <= 0) return 0;
        if (defense <= 0) return damage;

        decimal reduced = (decimal)damage * DefenseConstant / (DefenseConstant + defense);
        int result = (int)Math.Round(reduced, MidpointRounding.ToEven);

        return Math.Max(result, MinReceivedDamage);
    }
}