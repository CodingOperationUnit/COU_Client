using UnityEngine;

// 지정된 타겟을 쫓아가는 탄환. 타겟이 사라지면 마지막 방향으로 직진
// 추적, 관통, 연쇄 처리는 LinearProjectile이 담당하고 여기서는 처음 쫓을 적만 정함
public sealed class HomingProjectile : LinearProjectile
{
    protected override void OnLaunch()
    {
        base.OnLaunch();

        if(target != null && target.TryGetComponent(out Enemy targetEnemy))
        {
            homingTarget = targetEnemy;
        }
    }
}
