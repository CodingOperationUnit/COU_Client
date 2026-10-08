public sealed class Skill_Katana : ProjectileSkillBase
{
    protected override void Fire()
    {
        // 칼날은 모든 레벨에서 적을 계속 관통
        // 3레벨: 앞/뒤, 5레벨: 앞/우/뒤/좌
        int baseCount = level >= 5 ? 4 : level >= 3 ? 2 : 1;

        FireRadial(GetFlatStat(SkillFlatStat.ProjectileCount, baseCount), InfinitePierce);
    }
}
