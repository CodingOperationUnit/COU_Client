using System.Collections.Generic;
using UnityEngine;

// 장착하는 동안 오브젝트가 유지되는 스킬(회전 칼날 등)의 베이스
// 장착/레벨 변경/해제 시점에 오브젝트를 생성하고 정리함
public abstract class PersistentSkillBase : SkillBase
{
    private readonly List<GameObject> _spawnedObjects = new();

    // 쿨타임 발동이 없으므로 기본은 비어 있음. 매 프레임 갱신이 필요하면 오버라이드
    public override void Tick(float deltaTime)
    {
    }

    protected override void OnEquip()
    {
        SpawnObjects();
    }

    protected override void OnUnequip()
    {
        DespawnAll();
    }

    // 레벨이 바뀌면 개수/크기 등이 달라질 수 있으므로 전부 정리하고 다시 생성
    protected override void OnLevelChanged()
    {
        DespawnAll();
        SpawnObjects();
    }

    // SpawnPersistent로 현재 레벨에 맞는 오브젝트를 생성
    protected abstract void SpawnObjects();

    protected void SpawnPersistent<TInitData>(Vector3 position, TInitData initData)
    {
        GameObject instance = Spawn(position, initData);

        if(instance != null)
        {
            _spawnedObjects.Add(instance);
        }
    }

    private void DespawnAll()
    {
        foreach(GameObject instance in _spawnedObjects)
        {
            // 스스로 풀에 반환된 오브젝트를 다시 반환하면 풀에 중복 등록되므로 활성 상태만 반환
            if(instance != null && instance.activeInHierarchy)
            {
                SkillObjectPool.Instance.Return(instance);
            }
        }

        _spawnedObjects.Clear();
    }
}
