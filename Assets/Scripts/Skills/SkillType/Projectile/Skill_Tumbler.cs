public sealed class Skill_Tumbler : ProjectileSkillBase
{
    // 탄환 사이 간격 (도)
    private const float AnglePerProjectile = 15.0f;

    protected override void Fire()
    {
        // 기본 1발, 진화(5레벨 + 에너지드링크)하면 부채꼴 3발. 개수 증가 패시브 보너스는 여기에 더해짐
        int baseCount = IsEvolved ? 3 : 1;
        int count = GetFlatStat(SkillFlatStat.ProjectileCount, baseCount);

        FireSpread(count, AnglePerProjectile * (count - 1));
    }
}
