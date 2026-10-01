using UnityEngine;

public readonly struct ProjectileLaunchInfo
{
    public readonly Vector2 Direction;
    public readonly Transform Target; // 직선 탄환은 null

    public readonly float DamageMultiplier;

    public ProjectileLaunchInfo(Vector2 direction, Transform target = null, float damageMultiplier = 1.0f)
    {
        Direction = direction.normalized;
        Target = target;
        DamageMultiplier = damageMultiplier;
    }
}
