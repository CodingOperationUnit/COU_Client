using UnityEngine;

// 플레이어가 주는 피해의 최종 계산 (공격력, 스킬 피해 스탯, 치명타)
// 스킬은 자기 몫인 "스킬 배율"(skillDamage × 레벨 배율 × (1 + 패시브%))만 넘기고, 맞힐 때 이 함수를 부른다
public static class PlayerDamageCalculator
{
    // true: 기본 공격력일 때 피해 = 스킬 배율 (몬스터 체력 기획 전까지)
    // false: 피해 = 공격력 × 스킬 배율 (몬스터 체력 기획 후 전환)
    public static readonly bool UseRelativeAttack = true;

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

        float damage = skillMultiplier * GetAttackFactor(stats) * (stats.SkillDamage / 100f);

        bool isCritical = RollCritical(stats.CriticalChance);
        if (isCritical)
            damage *= stats.CriticalDamage / 100f;

        return new HitResult(Mathf.Max(1, Mathf.RoundToInt(damage)), isCritical);
    }

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
        return Random.Range(0, 100) < chance;
    }
}