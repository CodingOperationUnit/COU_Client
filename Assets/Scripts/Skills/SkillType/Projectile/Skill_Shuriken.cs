public sealed class Skill_Shuriken : ProjectileSkillBase
{
    protected override void Fire()
    {
        if (3 <= level && level < 5) FireSpread(3, 30.0f);
        else if (5 <= level) FireSpread(5, 60.0f);
        else
        {
            SpawnProjectile(fireDirection);
        }
    }
}
