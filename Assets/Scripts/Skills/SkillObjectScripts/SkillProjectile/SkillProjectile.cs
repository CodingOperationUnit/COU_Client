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

    protected virtual void ApplyDamage(Enemy enemy)
    {
        PlayerDamageCalculator.HitResult hit = stats.CalculateHit();

        enemy.Damaged(hit.damage, stats.SkillId);
    }
}
