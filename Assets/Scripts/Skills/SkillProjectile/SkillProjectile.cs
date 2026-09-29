using System;
using UnityEngine;

public abstract class SkillProjectile : MonoBehaviour, ISkillPoolable
{
    public event Action<GameObject> OnBeforeReturn;

    protected float damage;

    public void Init(float damageValue)
    {
        damage = damageValue;
    }

    public void OnSpawn()
    {
        OnLaunch();
    }

    public void OnDespawn()
    {
    }

    protected abstract void OnLaunch();

    protected virtual void ApplyDamage()
    {
        // TODO: 타겟 판정 및 데미지 적용 (Monster 구현 후 작성 예정)
    }

    protected void ReturnToPool()
    {
        OnDespawn();
        OnBeforeReturn?.Invoke(gameObject);
        SkillObjectPool.Instance.Return(gameObject);
    }
}
