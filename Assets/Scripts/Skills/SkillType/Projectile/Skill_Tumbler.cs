public sealed class Skill_Tumbler : ProjectileSkillBase
{
    // 탄환 사이 간격 (도)
    private const float AnglePerProjectile = 15.0f;

    protected override void Fire()
    {
        // 기본 1발. 개수 증가 패시브 보너스만 더해짐
        int count = GetFlatStat(SkillFlatStat.ProjectileCount, 1);

        FireSpread(count, AnglePerProjectile * (count - 1));
    }
}
