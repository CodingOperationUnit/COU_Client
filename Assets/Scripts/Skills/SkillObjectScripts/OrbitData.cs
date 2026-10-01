using UnityEngine;

// 플레이어 주위를 도는 스킬 오브젝트의 초기화 데이터
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
