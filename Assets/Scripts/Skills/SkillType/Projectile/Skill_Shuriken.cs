public sealed class Skill_Shuriken : ProjectileSkillBase
{
    protected override void Fire()
    {
        SpawnProjectile(fireDirection);
    }
}
