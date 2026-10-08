public sealed class Skill_Shuriken : ProjectileSkillBase
{
    // 탄환 사이 간격 (도)
    private const float AnglePerProjectile = 15.0f;

    protected override void Fire()
    {
        // 3레벨: 3발, 5레벨: 5발
        int baseCount = level >= 5 ? 5 : level >= 3 ? 3 : 1;
        int count = GetFlatStat(SkillFlatStat.ProjectileCount, baseCount);

        FireSpread(count, AnglePerProjectile * (count - 1));
    }
}
