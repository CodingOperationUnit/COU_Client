public sealed class Skill_Revolver : ProjectileSkillBase
{
    protected override void Fire()
    {
        SpawnProjectile(fireDirection);
    }
}
