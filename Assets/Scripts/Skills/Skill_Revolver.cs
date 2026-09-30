using UnityEngine;

public sealed class Skill_Revolver : SkillBase
{
    public override void Activate()
    {
        SetFireDirection();
        FireProjectile();

        ResetCooldown();
    }

    private Transform SetFireDirection()
    {
        return null;
    }

    protected override void FireLevel1()
    {
        SpawnProjectile(dir);
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