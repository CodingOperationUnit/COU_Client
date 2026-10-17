public sealed class Skill_Monitor : ProjectileSkillBase
{
    protected override void Fire()
    {
        // 모니터는 모든 레벨에서 적을 계속 관통. 기본 1발, 개수 증가 패시브 보너스만 더해짐
        // SFX\

        FireRadial(GetFlatStat(SkillFlatStat.ProjectileCount, 1), InfinitePierce);
    }
}
