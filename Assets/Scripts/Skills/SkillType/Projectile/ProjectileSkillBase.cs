using UnityEngine;

public abstract class ProjectileSkillBase : CooldownSkillBase
{
    protected void SpawnProjectile(Vector2 direction)
    {
        Spawn(transform.position, new ProjectileData(skillData, direction, fireTarget, DamageMultiplier));
    }

    // 발사 방향을 중심으로 totalAngle(도) 범위의 부채꼴로 count발 발사
    protected void FireSpread(int count, float totalAngle)
    {
        if(count <= 1)
        {
            SpawnProjectile(fireDirection);
            return;
        }

        float step = totalAngle / (count - 1);
        float startAngle = -totalAngle * 0.5f;

        for(int i = 0; i < count; i++)
        {
            SpawnProjectile(Rotate(fireDirection, startAngle + step * i));
        }
    }

    private static Vector2 Rotate(Vector2 vector, float degrees)
    {
        return Quaternion.Euler(0.0f, 0.0f, degrees) * vector;
    }
}
