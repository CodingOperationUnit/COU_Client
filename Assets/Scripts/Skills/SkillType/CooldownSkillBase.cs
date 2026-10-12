using System;
using UnityEngine;

// 쿨타임마다 사거리 안의 적을 찾아 발동하는 스킬의 공통 베이스
public abstract class CooldownSkillBase : SkillBase
{
    protected float cooldownTimer;
    protected Vector2 fireDirection;
    protected Transform fireTarget;
    public event Action<CooldownSkillBase> OnFired;
    public event Action<CooldownSkillBase> OnFireStarted;
    public float FireDelay { get; set; }
    private bool isFirePending;
    private float fireDelayTimer;

    public override void Initialize(SkillData data)
    {
        base.Initialize(data);
        cooldownTimer = data.skillCooldown;
    }

    public override void Tick(float deltaTime)
    {
        // 발사 대기 중: 대기 시간이 끝나면 발사
        if (isFirePending)
        {
            fireDelayTimer -= deltaTime;

            if (fireDelayTimer > 0.0f)
            {
                return;
            }

            isFirePending = false;
            FireNow();
            return;
        }

        ReduceCooldown(deltaTime);

        if (CanActivate())
        {
            OnFireStarted?.Invoke(this);

            if (FireDelay > 0.0f)
            {
                isFirePending = true;
                fireDelayTimer = FireDelay;
                return;
            }

            FireNow();
        }
    }

    // 실제 발사 + 쿨타임 초기화 + 알림
    private void FireNow()
    {
        // 대기 중 대상이 사라졌으면 처음 조준한 방향으로만 발사
        if (fireTarget != null && !fireTarget.gameObject.activeInHierarchy)
        {
            fireTarget = null;
        }

        Fire();
        ResetCooldown();

        // 선딜레이만큼 쿨타임을 줄여 발사 간격을 기존과 같게 유지
        if (FireDelay > 0.0f)
        {
            cooldownTimer = Mathf.Max(cooldownTimer - FireDelay, 0.01f);
        }

        OnFired?.Invoke(this);
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
        if(skillData == null)
        {
            cooldownTimer = 0.01f;
            return;
        }

        // 쿨타임 감소 패시브가 겹쳐도 0 이하로 내려가 매 프레임 발동하지 않도록 하한을 둠
        cooldownTimer = Mathf.Max(GetPercentStat(SkillPercentStat.Cooldown, skillData.skillCooldown * CooldownMultiplier), 0.01f);
    }
}
