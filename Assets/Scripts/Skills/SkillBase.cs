public abstract class SkillBase
{
    protected SkillData skillData;
    protected float cooldownTimer;

    public int SkillId => skillData != null ? skillData.SkillId : -1;

    public void Initialize( SkillData data )
    {
        skillData = data;
        cooldownTimer = 0f;
    }

    public virtual bool CanActivate()
    {
        return cooldownTimer <= 0f;
    }

    public void ReduceCooldown( float deltaTime )
    {
        if( cooldownTimer > 0f )
        {
            cooldownTimer -= deltaTime;
        }
    }

    public abstract void Activate();

    protected void ResetCooldown()
    {
        cooldownTimer = skillData != null ? skillData.Cooldown : 0f;
    }
}
