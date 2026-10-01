using UnityEngine;

public class SkillManager : MonoSingleton<SkillManager>
{
    [SerializeField] private SkillPrefabTable _prefabTable;

    public GameObject GetPrefab(int skillId)
    {
        if(_prefabTable == null)
        {
            Debug.LogWarning("[SkillManager] SkillPrefabTable이 연결되지 않았습니다. 씬의 SkillManager 오브젝트를 확인하세요.");
            return null;
        }

        SkillProjectile prefab = _prefabTable.Get(skillId);

        return prefab != null ? prefab.gameObject : null;
    }
}
