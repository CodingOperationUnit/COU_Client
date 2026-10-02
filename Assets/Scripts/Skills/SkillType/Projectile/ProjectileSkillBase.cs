using UnityEngine;

public abstract class ProjectileSkillBase : CooldownSkillBase
{
    // 수명이 다할 때까지 사실상 모든 적을 관통
    protected const int InfinitePierce = int.MaxValue;

    // pierceCount: 이 탄환이 맞출 수 있는 최대 적 수 (기본 1 = 첫 적을 맞추면 사라짐)
    protected void SpawnProjectile(Vector2 direction, int pierceCount = 1)
    {
        Spawn(transform.position, new ProjectileData(skillData, direction, fireTarget, DamageMultiplier, pierceCount));
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

    // fireDirection을 기준으로 360도를 count등분해 발사 (2 = 앞/뒤, 4 = 앞/우/뒤/좌)
    protected void FireRadial(int count, int pierceCount = 1)
    {
        if(count <= 1)
        {
            SpawnProjectile(fireDirection, pierceCount);
            return;
        }

        float step = 360.0f / count;

        for(int i = 0; i < count; i++)
        {
            SpawnProjectile(Rotate(fireDirection, step * i), pierceCount);
        }
    }

    private static Vector2 Rotate(Vector2 vector, float degrees)
    {
        return Quaternion.Euler(0.0f, 0.0f, degrees) * vector;
    }
}
