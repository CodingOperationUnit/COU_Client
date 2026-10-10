using System;
using UnityEngine;

public enum SkillTargetType
{
    None,
    Nearest,
    Forward
}

// 시트의 skillCategory 값과 같아야 함 (0: 액티브, 1: 패시브)
public enum SkillCategory
{
    Active = 0,
    Passive = 1
}

[Serializable]
public class SkillData : ISerializationCallbackReceiver
{
    public int skillId;
    public string skillName;
    public SkillCategory skillCategory;
    public string skillType;
    public float skillCooldown;
    public float skillSpeed;
    public float skillDamage;
    public float skillRange;
    public string skillDescription;
    public string level1SkillDescription;
    public string level2SkillDescription;
    public string level3SkillDescription;
    public string level4SkillDescription;
    public string level5SkillDescription;

    [NonSerialized] public SkillTargetType TargetType;

    public void OnAfterDeserialize()
    {
        if(string.IsNullOrEmpty(skillType))
        {
            return;
        }

        if(!Enum.TryParse(skillType, true, out TargetType))
        {
            Debug.LogWarning($"[SkillData] 알 수 없는 skillType입니다: {skillType} (ID: {skillId})");
        }
    }

    public void OnBeforeSerialize()
    {

    }
}
