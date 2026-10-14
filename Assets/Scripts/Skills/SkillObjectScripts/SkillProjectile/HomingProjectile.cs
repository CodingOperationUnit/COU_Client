using UnityEngine;

// 지정된 타겟을 수명이 끝날 때까지 쫓아가는 탄환. 타겟이 사라지면 마지막 방향으로 직진
public sealed class HomingProjectile : LinearProjectile
{
    private Enemy _targetEnemy;

    protected override void OnLaunch()
    {
        base.OnLaunch();

        _targetEnemy = null;

        if(target != null)
        {
            target.TryGetComponent(out _targetEnemy);
        }
    }

    protected override void Movement()
    {
        if(IsTargetAlive())
        {
            Vector2 toTarget = _targetEnemy.transform.position - transform.position;

            if(toTarget.sqrMagnitude > 0.0001f)
            {
                direction = toTarget.normalized;
                transform.up = direction;
            }
        }

        base.Movement();
    }

    // 죽었거나 풀로 반환된 적은 따라가지 않음
    private bool IsTargetAlive()
    {
        return _targetEnemy != null && !_targetEnemy.isDead && _targetEnemy.gameObject.activeInHierarchy;
    }
}
