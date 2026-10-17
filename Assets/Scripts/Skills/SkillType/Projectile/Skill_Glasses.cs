public sealed class Skill_Glasses : ProjectileSkillBase
{
    protected override void Fire()
    {
        // 3레벨: 3명 관통, 5레벨: 5명 관통
        int basePierce = level >= 5 ? 5 : level >= 3 ? 3 : 1;

        // SFX

        // 관통 보너스는 SpawnProjectile에서 더해짐
        SpawnProjectile(fireDirection, basePierce);
    }
}
