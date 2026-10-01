using System;
using UnityEngine;

public readonly struct ProjectileData
{
    public readonly SkillData Data;
    public readonly Vector2 Direction;
    public readonly Transform Target; // 직선 탄환은 null
    public readonly float DamageMultiplier;

    public ProjectileData(SkillData data, Vector2 direction, Transform target = null, float damageMultiplier = 1.0f)
    {
        Data = data;
        Direction = direction.normalized;
        Target = target;
        DamageMultiplier = damageMultiplier;
    }
}

public readonly struct AoEData
{
    public readonly SkillData Data;
    public readonly Vector2 Loacl;
    public readonly float DamageMultiplier;

    public AoEData(SkillData data, Vector2 loacl, float damageMultiplier = 1.0f)
    {
        Data = data;
        Loacl = loacl;
        DamageMultiplier = damageMultiplier;
    }
}

public readonly struct AutoData
{
    public readonly SkillData Data;

    public AutoData(SkillData data)
    {
        Data = data;
    }
}

public abstract class SkillObject<TInitData> : MonoBehaviour, ISkillPoolable
{
    public event Action<GameObject> OnBeforeReturn;
    protected SkillData skillData;

    public void Init(TInitData data)
    {
        OnInit(data);
    }

    protected abstract void OnInit(TInitData data);

    // 활성화되기 전
    protected abstract void OnLaunch();

    // 활성화 되고 나서
    public virtual void OnSpawn()
    {

    }

    public virtual void OnDespawn()
    {
        // SkillObjectPool.Instance.Return에 호출
    }

    protected virtual void ReturnToPool()
    {
        OnDespawn();// SkillObjectPool.Instance.Return에 호출
        OnBeforeReturn?.Invoke(gameObject);
        SkillObjectPool.Instance.Return(gameObject);
    }
}
