using UnityEngine;

public sealed class Skill_Katana : SkillBase
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
        SpawnProjectile(movement.FacingDirection);
    }

    protected override void FireLevel2()
    {
        throw new System.NotImplementedException();
    }

    protected override void FireLevel3()
    {
        throw new System.NotImplementedException();
    }

    protected override void FireLevel4()
    {
        throw new System.NotImplementedException();
    }

    protected override void FireLevel5()
    {
        throw new System.NotImplementedException();
    }
}