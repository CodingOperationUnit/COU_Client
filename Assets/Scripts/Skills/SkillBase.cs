using System;
using UnityEngine;

public abstract class SkillBase
{
    protected SkillData skillData;
    protected int level = 0;
    protected float cooldownTimer;
    protected Vector2 fireDirection;
    protected Transform fireTarget;

    protected SkillProjectile projectilePrefab;
    protected Transform transform;
    protected PlayerMovement movement;
    protected PlayerStats stats;

    public int Level => level;

    public int SkillId => skillData != null ? skillData.ID : -1;
    public float Range => skillData != null ? skillData.Range : 0.0f;

    public void Initialize(SkillData data)
    {
        skillData = data;
        cooldownTimer = data.Cooldown;
        level = 0;
    }

    public void SetSkillController(SkillController controller)
    {
        transform = controller.transform;
        movement = controller.PlayerMovement;
        stats = controller.PlayerStats;

    }

    public void SetPrefab(SkillProjectile prefab)
    {
        projectilePrefab = prefab;
    }

    // 쿨타임이 끝났고, 사거리 안에 적이 있을 때만 발동
    public virtual bool CanActivate()
    {
        if(cooldownTimer > 0.0f)
        {
            return false;
        }

        return FindFireDirection();
    }

    // 사거리 안의 적을 찾아서 발사 방향(fireDirection)을 정함. 적이 없으면 false
    private bool FindFireDirection()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, skillData.Range, LayerMask.GetMask("Enemy"));

        if (enemies.Length == 0)
        {
            return false;
        }

        // Forward: 플레이어가 바라보는 방향
        if(skillData.TargetType == SkillTargetType.Forward)
        {
            fireDirection = movement.FacingDirection;
            fireTarget = null;
            return true;
        }

        // Nearest: 사거리 안에서 가장 가까운 적 방향
        Collider2D nearest = enemies[0];
        float minDistance = Vector2.Distance(transform.position, nearest.transform.position);

        foreach(Collider2D enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if(distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy;
            }
        }

        fireDirection = (nearest.transform.position - transform.position).normalized;
        fireTarget = nearest.transform;
        return true;
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

    protected void SpawnProjectile(Vector2 direction)
    {
        if(projectilePrefab == null)
        {
            Debug.LogWarning($"[SkillBase] 발사체 프리팹이 없습니다: {SkillId}");
            return;
        }

        SkillObjectPool.Instance.Get(projectilePrefab.gameObject, transform.position, Quaternion.identity, instance =>
        {
            if (instance.TryGetComponent(out SkillProjectile projectile))
            {
                projectile.Init(skillData, new ProjectileLaunchInfo(direction, fireTarget));
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