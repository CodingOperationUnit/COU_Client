using System;
using System.Collections.Generic;
using UnityEngine;

public static class SkillFactory
{
    private static readonly Dictionary<int, Func<SkillBase>> _creators = new()
    {
        // 액티브 (20000대)
        //{ 20010, () => new Skill_Tumbler() },
        //{ 20020, () => new Skill_Glasses() },
        { 20030, () => new Skill_Monitor() },
        { 20040, () => new Skill_Speaker() },
        { 20050, () => new Skill_Mouse() },
        { 20060, () => new Skill_Keyboard() },
        { 20010, () => new Skill_CtrlZ() },
        { 20020, () => new Skill_PowerStrip() },

        // 패시브 (21000대)
        // 임시: 실제 효과가 확정되기 전에 동작 확인용으로 기존 테스트 패시브를 연결해 둠
        { 21010, () => new Skill_DamageUp() },
        { 21070, () => new Skill_ProjectileUp() },
    };

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // 테스트용: 시트 값을 무시하고 여기서 스킬 수치를 직접 덮어씀. 쓰지 않을 땐 항목을 지우거나 주석 처리할 것
    // 수정 후 재생을 다시 시작하거나, 이미 장착한 스킬은 해제 후 다시 장착해야 반영됨
    private static readonly Dictionary<int, Action<SkillData>> _debugOverrides = new()
    {
        { 20060, data => {
            data.skillType = "Nearest";
            data.skillCooldown = 0.75f; 
            data.skillSpeed = 0.0f;
            data.skillDamage = 1.25f;
            data.skillRange = 1.0f;
        } },
        { 20010, data => {
            data.skillType = "Nearest";
            data.skillCooldown = 1.0f;
            data.skillSpeed = 4.0f;
            data.skillDamage = 1.0f;
            data.skillRange = 5.0f;
        } },
        { 20020, data => {
            data.skillType = "None";
            data.skillCooldown = 1.0f;
            data.skillSpeed = 0.0f;
            data.skillDamage = 1.0f;
            data.skillRange = 3.0f;
        } },
    };
#endif

    public static bool IsRegistered(int skillId) => _creators.ContainsKey(skillId);

    public static SkillBase Create(int skillId)
    {
        if(!_creators.TryGetValue(skillId, out Func<SkillBase> creator))
        {
            Debug.LogWarning($"[SkillFactory] 등록되지 않은 스킬 ID입니다: {skillId}");
            return null;
        }

        SkillData data = GameManager.JsonData.GetSkillDataFromJson(skillId);

        if(data == null)
        {
            Debug.LogWarning($"[SkillFactory] 스킬 데이터가 없습니다: {skillId}");
            return null;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if(_debugOverrides.TryGetValue(skillId, out Action<SkillData> overrideAction))
        {
            // 딕셔너리의 원본이 바뀌지 않도록 복사본에만 적용
            data = data.Clone();
            overrideAction(data);
            Debug.Log($"[SkillFactory] 디버그 덮어쓰기 적용: {skillId} (쿨타임 {data.skillCooldown}, 속도 {data.skillSpeed}, 대미지 {data.skillDamage}, 사거리 {data.skillRange})");
        }
#endif

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