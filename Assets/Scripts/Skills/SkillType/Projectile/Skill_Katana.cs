public sealed class Skill_Katana : ProjectileSkillBase
{
    protected override void Fire()
    {
        SpawnProjectile(fireDirection);
    }
}
