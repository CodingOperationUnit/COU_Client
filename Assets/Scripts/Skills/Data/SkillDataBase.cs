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

        string json = JsonDataManager.ReadTableText(GameConstants.Paths.SkillData_Json_Path);

        if(json == null)
        {
            Debug.LogError("[SkillDataBase] Resources/Data/Skill.json을 찾을 수 없습니다.");
            return;
        }

        SkillDataListWrapper wrapper = JsonUtility.FromJson<SkillDataListWrapper>(json);

        foreach(SkillData data in wrapper.datas)
        {
            _dataById[data.skillId] = data;
        }
    }

    // 서버 정적 데이터 갱신 때 JsonDataManager가 교체한다
    public static void Replace(Dictionary<int, SkillData> dataById)
    {
        _dataById = dataById;
    }

    public static SkillData Get(int skillId)
    {
        Load();

        return _dataById.GetValueOrDefault(skillId);
    }
}
