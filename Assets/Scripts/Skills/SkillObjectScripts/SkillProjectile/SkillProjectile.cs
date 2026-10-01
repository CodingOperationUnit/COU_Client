using System;
using UnityEngine;

public abstract class SkillProjectile : SkillObject<ProjectileData>, ISkillPoolable
{
    protected Vector2 direction;
    protected Transform target;

    protected override void OnInit(ProjectileData data)
    {
        skillData = data.Data;
        direction = data.Direction;
        target = data.Target;
        OnLaunch();
    }

    protected abstract void ApplyDamage(Enemy enemy);
}
