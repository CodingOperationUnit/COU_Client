using UnityEngine;

public readonly struct ProjectileLaunchInfo
{
    public readonly Vector2 Direction;
    public readonly Transform Target; // 직선 탄환은 null

    public ProjectileLaunchInfo(Vector2 direction, Transform target = null)
    {
        Direction = direction.normalized;
        Target = target;
    }
}
