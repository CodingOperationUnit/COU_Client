using System.Collections.Generic;

// 지정한 횟수(pierceCount)만큼 적을 때리며 관통하다가, 다 때리면 사라지는 직선 탄환
public sealed class PiercingProjectile : LinearProjectile
{
    private readonly HashSet<Enemy> _hitEnemies = new();

    private int _remainingPierce;

    protected override void OnLaunch()
    {
        base.OnLaunch();

        _remainingPierce = pierceCount;
        _hitEnemies.Clear();
    }

    protected override void OnHitEnemy(Enemy enemy)
    {
        // 이미 때린 적은 건너뜀 (콜라이더가 여러 개인 적 대비)
        if(_remainingPierce <= 0 || !_hitEnemies.Add(enemy))
        {
            return;
        }

        ApplyDamage(enemy);
        _remainingPierce--;

        if(_remainingPierce <= 0)
        {
            ReturnToPool();
        }
    }
}
