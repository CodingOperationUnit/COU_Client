using System;
using UnityEngine;

public enum SkillTargetType
{
    None,
    Nearest,
    Forward
}

[Serializable]
public class SkillData : ISerializationCallbackReceiver
{
    public int ID;
    public string Name;
    public string Type;
    public float Cooldown;
    public float Speed;
    public float Damage;
    public float Range;
    public string Description;

    [NonSerialized] public SkillTargetType TargetType;

    public void OnAfterDeserialize()
    {
        if(!Enum.TryParse(Type, true, out TargetType))
        {
            Debug.LogWarning($"[SkillData] 알 수 없는 Type입니다: {Type} (ID: {ID})");
        }
    }

    public void OnBeforeSerialize()
    {

    }
}

[Serializable]
public class SkillDataListWrapper
{
    public SkillData[] datas;
}
