using System;
using UnityEngine;

public abstract class SkillProjectile : SkillAttackObject<ProjectileData>, ISkillPoolable
{
    protected Vector2 direction;
    protected Transform target;
    protected int pierceCount;
    protected int chainCount;

    // 풀에서 다시 꺼낼 때 바뀐 크기가 아니라 프리팹 원래 크기를 기준으로 삼기 위해 첫 Init에서 한 번만 저장
    private Vector3 _baseScale;
    private bool _initialized;

    protected override void OnInit(ProjectileData data)
    {
        if(!_initialized)
        {
            _initialized = true;
            _baseScale = transform.localScale;
        }

        stats = data.Stats;

        // 범위 증가 보너스는 프리팹 크기에 곱해서 스프라이트와 판정이 함께 커지게 함 (모든 투사체 공통)
        transform.localScale = _baseScale * stats.Scale;
        direction = data.Direction;
        target = data.Target;
        pierceCount = data.PierceCount;
        chainCount = data.ChainCount;
        OnLaunch();
    }

    // 타격 시 소환되면 맞은 적의 위치에서 때린 방향 그대로 날아감
    public override bool CanSpawnOnHit => true;

    public override void InitFromHit(HitSpawnData hit)
    {
        Init(new ProjectileData(hit.Stats, hit.Direction, null, 1, hit.Owner));
    }

    // 투사체가 어느 쪽을 앞으로 보는지(탄두 = 로컬 right, +X)를 정하는 곳. 프리팹은 오른쪽을 보고 만듦
    // 회전과 이동은 이 두 함수만 쓰므로 기준을 바꿀 때는 여기만 고치면 됨 (프리팹 스프라이트의 방향도 같이 맞춰야 함)
    protected void FaceDirection(Vector2 newDirection)
    {
        direction = newDirection;
        transform.right = direction;
    }

    // 로컬 좌표 기준이라 FaceDirection으로 정한 앞 방향(right)으로 전진
    protected void MoveForward(float speed)
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    protected virtual void ApplyDamage(Enemy enemy)
    {
        PlayerDamageCalculator.HitResult hit = stats.CalculateHit();

        enemy.Damaged(hit.damage, stats.SkillId, KnockbackEnabled);

        // 투사체(마우스, 텀블러, 안경, CtrlZ)의 타격음은 모두 여기서 재생됨
        PlayHitSfx();
        SpawnOnHit(enemy, stats);
    }
}
