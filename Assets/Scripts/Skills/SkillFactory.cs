using System;
using System.Collections.Generic;
using UnityEngine;

public static class SkillFactory
{
    private static readonly Dictionary<int, Func<SkillBase>> _creators = new()
    {
        { 1, () => new Skill_Shuriken() },
        { 2, () => new Skill_Revolver() },
        { 3, () => new Skill_Katana() },
    };

    public static SkillBase Create(int skillId)
    {
        if(!_creators.TryGetValue(skillId, out Func<SkillBase> creator))
        {
            Debug.LogWarning($"[SkillFactory] 등록되지 않은 스킬 ID입니다: {skillId}");
            return null;
        }

        SkillData data = SkillDataBase.Get(skillId);

        if(data == null)
        {
            Debug.LogWarning($"[SkillFactory] 스킬 데이터가 없습니다: {skillId}");
            return null;
        }

        SkillBase skill = creator();
        skill.Initialize(data);

        return skill;
    }
}