using UnityEngine;

public sealed class Skill_Revolver : SkillBase
{
    public override void Activate()
    {
        FireProjectile();

        ResetCooldown();
    }

    protected override void FireLevel1()
    {
        SpawnProjectile(fireDirection);
    }

    protected override void FireLevel2()
    {
        FireLevel1();
    }

    protected override void FireLevel3()
    {
        FireLevel1();
    }

    protected override void FireLevel4()
    {
        FireLevel1();
    }

    protected override void FireLevel5()
    {
        FireLevel1();
    }
}