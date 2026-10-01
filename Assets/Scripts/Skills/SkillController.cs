using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillController : MonoBehaviour
{
    public const int MaxSkillSlots = 3;

    private readonly List<SkillBase> _activeSkills = new();

    public PlayerMovement PlayerMovement { get; private set; }
    public PlayerStats PlayerStats { get; private set; }

    public IReadOnlyList<SkillBase> ActiveSkills => _activeSkills;

    private void Awake()
    {
        PlayerMovement = GetComponent<PlayerMovement>();
        PlayerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        foreach(SkillBase skill in _activeSkills)
        {
            skill.ReduceCooldown(Time.deltaTime);

            if(skill.CanActivate())
            {
                skill.Activate();
            }
        }

        Keyboard keyboard = Keyboard.current;

        if(keyboard == null)
        {
            return;
        }

        // 테스트
        if(keyboard.digit1Key.wasPressedThisFrame)
        {
            if(EquipSkill(1)) Debug.Log("스킬: Shuriken");
        }

        if(keyboard.digit2Key.wasPressedThisFrame)
        {
            if (EquipSkill(2)) Debug.Log("스킬: Revolver");
        }

        if(keyboard.digit3Key.wasPressedThisFrame)
        {
            if (EquipSkill(3)) Debug.Log("스킬: Katana");

        }
    }

    public bool EquipSkill(int skillId)
    {
        if (_activeSkills.Count >= MaxSkillSlots)
        {
            Debug.LogWarning($"[SkillController] 슬롯이 가득 차 스킬을 장착할 수 없습니다: {skillId}");
            return false;
        }

        SkillBase existing = _activeSkills.Find(s => s.SkillId == skillId);

        if (existing != null)
        {
            existing.Levelup();
            return true;
        }

        SkillBase skill = SkillFactory.Create(skillId);

        if(skill == null)
        {
            return false;
        }

        skill.SetSkillController(this);
        skill.SetPrefab(SkillManager.Instance.GetProjectilePrefab(skillId));

        skill.Levelup();
        _activeSkills.Add(skill);
        return true;
    }

    public bool LevelUpSkill(int skillId)
    {
        SkillBase skill = _activeSkills.Find(s => s.SkillId == skillId);

        if(skill == null || skill.Level >= SkillBase.MaxLevel)
        {
            return false;
        }

        skill.Levelup();
        return true;
    }

    // For Debug (장착된 스킬별 탐지 사거리 표시)
    private void OnDrawGizmos()
    {
        Color[] colors = { Color.cyan, Color.yellow, Color.magenta };

        for(int i = 0; i < _activeSkills.Count; i++)
        {
            Gizmos.color = colors[i % colors.Length];
            Gizmos.DrawWireSphere(transform.position, _activeSkills[i].Range);
        }
    }

    public bool UnequipSkill(int skillId)
    {
        SkillBase skill = _activeSkills.Find(s => s.SkillId == skillId);

        if(skill == null)
        {
            return false;
        }

        return _activeSkills.Remove(skill);
    }
}