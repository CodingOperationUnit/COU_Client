public sealed class Skill_Glasses : ProjectileSkillBase
{
    protected override void Fire()
    {
        // SFX

        // 기본 관통 1. 관통 보너스(레벨 효과, 패시브)는 SpawnProjectile에서 더해짐
        SpawnProjectile(fireDirection);
    }
}
