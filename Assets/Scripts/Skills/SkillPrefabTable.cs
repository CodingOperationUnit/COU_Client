using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillPrefabEntry
{
    public int SkillId;
    public SkillProjectile Prefab;
}

[CreateAssetMenu(fileName = "SkillPrefabTable", menuName = "Skill/SkillPrefabTable")]
public class SkillPrefabTable : ScriptableObject
{
    [SerializeField] private List<SkillPrefabEntry> _entries = new();

    private Dictionary<int, SkillProjectile> _prefabById;

    public SkillProjectile Get(int skillId)
    {
        if(_prefabById == null)
        {
            Build();
        }

        if(!_prefabById.TryGetValue(skillId, out SkillProjectile prefab))
        {
            Debug.LogWarning($"[SkillPrefabTable] 등록되지 않은 스킬 프리팹입니다: {skillId}");
            return null;
        }

        return prefab;
    }

    private void Build()
    {
        _prefabById = new Dictionary<int, SkillProjectile>();

        foreach(SkillPrefabEntry entry in _entries)
        {
            if(!_prefabById.TryAdd(entry.SkillId, entry.Prefab))
            {
                Debug.LogWarning($"[SkillPrefabTable] 중복된 스킬 ID입니다: {entry.SkillId}");
            }
        }
    }

    private void OnValidate()
    {
        _prefabById = null;
    }
}
