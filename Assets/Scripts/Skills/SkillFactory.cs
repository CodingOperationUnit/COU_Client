using System;
using System.Collections.Generic;
using UnityEngine;

public static class SkillFactory
{
    private static readonly Dictionary<int, Func<SkillBase>> _creators = new()
    {
        // 액티브 (20000대)
        { 20010, () => new Skill_Shuriken() },
        { 20020, () => new Skill_Revolver() },
        { 20030, () => new Skill_Katana() },
        { 20040, () => new Skill_Planet() },

        // 패시브 (21000대)
        // 임시: 실제 효과가 확정되기 전에 동작 확인용으로 기존 테스트 패시브를 연결해 둠
        { 21010, () => new Skill_DamageUp() },
        { 21070, () => new Skill_ProjectileUp() },
    };

    public static bool IsRegistered(int skillId) => _creators.ContainsKey(skillId);

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

        // 시트의 분류와 클래스가 어긋나면 슬롯/UI 분류가 틀어지므로 바로 알 수 있게 경고
        bool isPassiveClass = skill is PassiveSkillBase;

        if(isPassiveClass != (data.skillCategory == SkillCategory.Passive))
        {
            Debug.LogWarning($"[SkillFactory] 시트의 skillCategory와 스킬 클래스가 맞지 않습니다: {skillId} ({data.skillCategory})");
        }

        skill.Initialize(data);

        return skill;
    }
}