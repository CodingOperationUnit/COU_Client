using System;
using System.Collections.Generic;
using UnityEngine;

public static class SkillFactory
{
    private static readonly Dictionary<int, Func<SkillBase>> _creators = new()
    {
        { 1, () => new Skill_Katana() },
        { 2, () => new Skill_Revolver() },
        { 3, () => new Skill_Shuriken() },
    };

    public static SkillBase Create(int skillId)
    {
        if(!_creators.TryGetValue(skillId, out Func<SkillBase> creator))
        {
            Debug.LogWarning($"[SkillFactory] 등록되지 않은 스킬 ID입니다: {skillId}");
            return null;
        }

        SkillBase skill = creator();

        SkillDataBase.Load();
        SkillData data = SkillDataBase.Get(skillId);
        skill.Initialize(data);

        return skill;
    }
}