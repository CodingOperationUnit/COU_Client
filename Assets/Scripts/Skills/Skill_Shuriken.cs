using UnityEngine;

public sealed class Skill_Shuriken : SkillBase
{
    public override void Activate()
    {
        FindNearestTarget();
        FireProjectile();

        ResetCooldown();
    }

    private Transform FindNearestTarget()
    {
        return null;
    }

    protected override void FireLevel1()
    {

    }

    protected override void FireLevel2()
    {

    }

    protected override void FireLevel3()
    {

    }

    protected override void FireLevel4()
    {

    }

    protected override void FireLevel5()
    {

    }
}