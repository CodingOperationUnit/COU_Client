public sealed class Skill_Revolver : ProjectileSkillBase
{
    protected override void Fire()
    {
        // 3레벨: 3명 관통, 5레벨: 5명 관통
        int pierceCount = level >= 5 ? 5 : level >= 3 ? 3 : 1;

        SpawnProjectile(fireDirection, pierceCount);
    }
}
