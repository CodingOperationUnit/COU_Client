using System;
using UnityEngine;

public abstract class SkillBase
{
    protected SkillData skillData;
    protected int level = 0;
    protected float cooldownTimer;
    protected SkillProjectile projectilePrefab;
    protected Transform owner;

    // 테스트
    public Vector2 dir;

    public int Level => level;

    public int SkillId => skillData != null ? skillData.ID : -1;

    public void Initialize(SkillData data)
    {
        skillData = data;
        cooldownTimer = data.Cooldown;
        level = 0;
    }

    // 테스트
    public void Initialize_Dir(Vector2 dir)
    {
        this.dir = dir;
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

    public void SetOwner(Transform ownerTransform)
    {
        owner = ownerTransform;
    }

    public void SetProjectilePrefab(SkillProjectile prefab)
    {
        projectilePrefab = prefab;
    }

    protected void SpawnProjectile(Vector2 direction)
    {
        if(projectilePrefab == null)
        {
            Debug.LogWarning($"[SkillBase] 발사체 프리팹이 없습니다: {SkillId}");
            return;
        }

        Vector3 position = owner != null ? owner.position : Vector3.zero;
        Vector3 forward = direction != Vector2.zero ? new Vector3(direction.x, 0.0f, direction.y) : Vector3.forward;
        float damage = skillData.Damage;

        SkillObjectPool.Instance.Get(projectilePrefab.gameObject, position, Quaternion.LookRotation(forward), instance =>
        {
            if(instance.TryGetComponent(out SkillProjectile projectile))
            {
                projectile.Init(damage);
            }
        });
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