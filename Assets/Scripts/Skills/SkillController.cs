using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillController : MonoBehaviour
{
    private const int MaxSkillSlots = 3;

    private readonly List<SkillBase> _activeSkills = new();
    private PlayerMovement _playerMovement;

    public IReadOnlyList<SkillBase> ActiveSkills => _activeSkills;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
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
            EquipSkill(1);

            Debug.Log("스킬: Shuriken");
        }

        if(keyboard.digit2Key.wasPressedThisFrame)
        {
            EquipSkill(2);
            Debug.Log("스킬: Revolver");
        }

        if(keyboard.digit3Key.wasPressedThisFrame)
        {
            EquipSkill(3);
            Debug.Log("스킬: Katana");
        }
    }

    public bool EquipSkill(int skillId)
    {
        if(_activeSkills.Count >= MaxSkillSlots)
        {
            // TODO: 슬롯 초과 시 교체/선택 정책은 추후 UI 연동 후 확정 (현재는 실패 처리)
            Debug.LogWarning($"[SkillController] 슬롯이 가득 차 스킬을 장착할 수 없습니다: {skillId}");
            return false;
        }

        SkillBase skill = SkillFactory.Create(skillId);

        if(skill == null)
        {
            return false;
        }

        // 테스트 ( 플레이어의 트랜스폼을 넣을 예정 )
        skill.SetOwner(transform);
        skill.BindPlayerMovement(_playerMovement);
        skill.SetProjectilePrefab(SkillManager.Instance.GetProjectilePrefab(skillId));

        skill.Levelup();
        _activeSkills.Add(skill);
        return true;
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