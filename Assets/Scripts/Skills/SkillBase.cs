using System;

public abstract class SkillBase
{
    protected SkillData skillData;
    protected int level = 0;
    protected float cooldownTimer;

    public int Level => level;

    public int SkillId => skillData != null ? skillData.SkillId : -1;

    public void Initialize(SkillData data)
    {
        skillData = data;
        cooldownTimer = data.Cooldown;
        level = 0;
    }

    public virtual bool CanActivate()
    {
        return cooldownTimer <= 0.0f;
    }

    public void ReduceCooldown(float deltaTime)
    {
        if (cooldownTimer > 0.0f)
        {
            cooldownTimer -= deltaTime;
        }
    }

    public abstract void Activate();

    protected void ResetCooldown()
    {
        cooldownTimer = skillData != null ? skillData.Cooldown : 0.01f;
    }

    public virtual void Levelup()
    {
        level = Math.Clamp(level + 1, 1, 5);
    }

    public virtual void LevelDown()
    {
        level = Math.Clamp(level - 1, 1, 5);
    }

    protected virtual void FireProjectile()
    {
        switch (level)
        {
            case 1:
                FireLevel1();
                break;
            case 2:
                FireLevel2();
                break;
            case 3:
                FireLevel3();
                break;
            case 4:
                FireLevel4();
                break;
            case 5:
                FireLevel5();
                break;
            default:
                break;
        }
    }

    protected abstract void FireLevel1();
    protected abstract void FireLevel2();
    protected abstract void FireLevel3();
    protected abstract void FireLevel4();
    protected abstract void FireLevel5();
}