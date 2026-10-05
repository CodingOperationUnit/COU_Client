using UnityEngine;

// 레벨 배율과 패시브 보너스가 모두 반영된, 스킬 오브젝트가 그대로 쓰는 최종 수치
// SkillData(기획 원본 값)와 달리 스킬 클래스가 계산을 끝낸 값만 담음
public readonly struct SkillStats
{
    public readonly int Damage;
    public readonly float Speed;
    public readonly float Scale; // 1.0 = 프리팹 원래 크기

    public SkillStats(int damage, float speed, float scale)
    {
        Damage = damage;
        Speed = speed;
        Scale = scale;
    }
}

// 스킬 오브젝트(SkillObject<TInitData>)가 Init으로 받는 초기화 데이터 모음
// 스킬 유형마다 필요한 값이 달라 구조체를 나눠 두며, SkillBase.Spawn<TInitData>가 이 타입으로 대상 오브젝트를 찾음

// 투사체 (SkillProjectile)
public readonly struct ProjectileData
{
    public readonly SkillStats Stats;
    public readonly Vector2 Direction;
    public readonly Transform Target; // 직선 탄환은 null
    public readonly int PierceCount;

    public ProjectileData(SkillStats stats, Vector2 direction, Transform target = null, int pierceCount = 1)
    {
        Stats = stats;
        Direction = direction.normalized;
        Target = target;
        PierceCount = pierceCount;
    }
}

// 범위 효과
public readonly struct AoEData
{
    public readonly SkillStats Stats;
    public readonly Vector2 Local;

    public AoEData(SkillStats stats, Vector2 local)
    {
        Stats = stats;
        Local = local;
    }
}

// 플레이어 주위를 도는 오브젝트 (PlanetObject)
public readonly struct OrbitData
{
    public readonly SkillStats Stats;
    public readonly Transform Owner;
    public readonly float StartAngle;
    public readonly float Radius;

    public OrbitData(SkillStats stats, Transform owner, float startAngle, float radius)
    {
        Stats = stats;
        Owner = owner;
        StartAngle = startAngle;
        Radius = radius;
    }
}
