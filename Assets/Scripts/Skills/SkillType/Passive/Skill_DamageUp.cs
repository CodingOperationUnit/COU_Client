// 대미지 증가 패시브 (퍼센트). 임시 수치: 레벨당 +10%
public sealed class Skill_DamageUp : PassiveSkillBase
{
    private const float RatioPerLevel = 0.1f;

    protected override void ApplyEffect()
    {
        modifiers.SetPercent(this, SkillPercentStat.Damage, RatioPerLevel * level);
    }
}
