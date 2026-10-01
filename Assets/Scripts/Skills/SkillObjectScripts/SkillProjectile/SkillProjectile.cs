using System;
using UnityEngine;

public abstract class SkillProjectile : SkillObject<ProjectileData>, ISkillPoolable
{
    protected Vector2 direction;
    protected Transform target;
    protected float damageMultiplier;

    protected override void OnInit(ProjectileData data)
    {
        skillData = data.Data;
        direction = data.Direction;
        target = data.Target;
        damageMultiplier = data.DamageMultiplier;
        OnLaunch();
    }

    protected virtual void ApplyDamage(Enemy enemy)
    {
        enemy.Damaged(Mathf.RoundToInt(skillData.Damage * damageMultiplier));
    }
}