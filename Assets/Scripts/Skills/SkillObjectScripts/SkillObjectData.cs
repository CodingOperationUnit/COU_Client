using UnityEngine;

// 스킬 오브젝트(SkillObject<TInitData>)가 Init으로 받는 초기화 데이터 모음
// 스킬 유형마다 필요한 값이 달라 구조체를 나눠 두며, SkillBase.Spawn<TInitData>가 이 타입으로 대상 오브젝트를 찾음

// 투사체 (SkillProjectile)
public readonly struct ProjectileData
{
    public readonly SkillData Data;
    public readonly Vector2 Direction;
    public readonly Transform Target; // 직선 탄환은 null
    public readonly float DamageMultiplier;
    public readonly int PierceCount;

    public ProjectileData(SkillData data, Vector2 direction, Transform target = null, float damageMultiplier = 1.0f, int pierceCount = 1)
    {
        Data = data;
        Direction = direction.normalized;
        Target = target;
        DamageMultiplier = damageMultiplier;
        PierceCount = pierceCount;
    }
}

// 범위 효과
public readonly struct AoEData
{
    public readonly SkillData Data;
    public readonly Vector2 Local;
    public readonly float DamageMultiplier;

    public AoEData(SkillData data, Vector2 local, float damageMultiplier = 1.0f)
    {
        Data = data;
        Local = local;
        DamageMultiplier = damageMultiplier;
    }
}

// 플레이어 주위를 도는 오브젝트 (PlanetObject)
public readonly struct OrbitData
{
    public readonly SkillData Data;
    public readonly Transform Owner;
    public readonly float StartAngle;
    public readonly float DamageMultiplier;

    public OrbitData(SkillData data, Transform owner, float startAngle, float damageMultiplier = 1.0f)
    {
        Data = data;
        Owner = owner;
        StartAngle = startAngle;
        DamageMultiplier = damageMultiplier;
    }
}
