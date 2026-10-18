using System;
using UnityEngine;

public abstract class SkillProjectile : SkillObject<ProjectileData>, ISkillPoolable
{
    protected Vector2 direction;
    protected Transform target;
    protected int pierceCount;
    protected int chainCount;

    protected override void OnInit(ProjectileData data)
    {
        stats = data.Stats;
        direction = data.Direction;
        target = data.Target;
        pierceCount = data.PierceCount;
        chainCount = data.ChainCount;
        OnLaunch();
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

        enemy.Damaged(hit.damage, stats.SkillId);

        // 투사체(마우스, 텀블러, 안경, CtrlZ)의 타격음은 모두 여기서 재생됨
        PlayHitSfx();
    }
}
