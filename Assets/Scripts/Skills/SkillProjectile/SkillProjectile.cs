using System;
using UnityEngine;

public abstract class SkillProjectile : MonoBehaviour, ISkillPoolable
{
    public event Action<GameObject> OnBeforeReturn;

    protected SkillData skillData;
    protected ProjectileLaunchInfo launchInfo;

    public void Init(SkillData data, in ProjectileLaunchInfo info)
    {
        skillData = data;
        launchInfo = info;
        OnLaunch();
    }

    public void OnSpawn()
    {
        // 데이터가 필요한 초기화는 Init에서 처리한다
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
