using System.Collections.Generic;
using UnityEngine;

public static class SkillDataBase
{
    private static Dictionary<int, SkillData> _dataById;

    public static void Load()
    {
        if(_dataById != null)
        {
            return;
        }

        _dataById = new Dictionary<int, SkillData>();

        TextAsset json = Resources.Load<TextAsset>("JsonFiles/Skill");

        if(json == null)
        {
            Debug.LogError("[SkillDataBase] Resources/JsonFiles/Skill.json을 찾을 수 없습니다.");
            return;
        }

        SkillDataListWrapper wrapper = JsonUtility.FromJson<SkillDataListWrapper>(json.text);

        foreach(SkillData data in wrapper.datas)
        {
            _dataById[data.ID] = data;
        }
    }

    public static SkillData Get(int skillId)
    {
        Load();

        return _dataById.GetValueOrDefault(skillId);
    }
}
