using UnityEngine;

// 쿨타임마다 사거리 안의 적을 찾아 발동하는 스킬의 공통 베이스
public abstract class CooldownSkillBase : SkillBase
{
    protected float cooldownTimer;
    protected Vector2 fireDirection;
    protected Transform fireTarget;

    public override void Initialize(SkillData data)
    {
        base.Initialize(data);
        cooldownTimer = data.skillCooldown;
    }

    public override void Tick(float deltaTime)
    {
        ReduceCooldown(deltaTime);

        if(CanActivate())
        {
            Fire();
            ResetCooldown();
        }
    }

    // 쿨타임이 끝났고, 사거리 안에 적이 있을 때만 발동
    protected virtual bool CanActivate()
    {
        if(cooldownTimer > 0.0f)
        {
            return false;
        }

        return FindFireDirection();
    }

    // 실제 발동 동작. 레벨별 분기는 각 스킬이 필요한 만큼만 구현
    protected abstract void Fire();

    // 사거리 안의 적을 찾아서 발사 방향(fireDirection)을 정함. 적이 없으면 false
    private bool FindFireDirection()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, skillData.skillRange, LayerMask.GetMask("Enemy"));

        if(enemies.Length == 0)
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

    private void ReduceCooldown(float deltaTime)
    {
        if(cooldownTimer > 0.0f)
        {
            cooldownTimer -= deltaTime;
        }
    }

    protected void ResetCooldown()
    {
        cooldownTimer = skillData != null ? skillData.skillCooldown * CooldownMultiplier : 0.01f;
    }
}
