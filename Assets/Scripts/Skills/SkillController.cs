using System.Collections.Generic;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    private const int MaxSkillSlots = 3;

    private readonly List<SkillBase> _activeSkills = new();

    // 테스트
    public Vector2 _dir;

    public IReadOnlyList<SkillBase> ActiveSkills => _activeSkills;

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

        // 테스트
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            EquipSkill(11);

            Debug.Log($"스킬: Bullet");
        }

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            EquipSkill(21);
            Debug.Log("스킬: Shuriken");
        }

        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            EquipSkill(31);
            Debug.Log("스킬: Katana");
        }

        Vector2 input = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) input.y += 1;

        if (Input.GetKey(KeyCode.S)) input.y -= 1;

        if (Input.GetKey(KeyCode.D)) input.x += 1;

        if (Input.GetKey(KeyCode.A)) input.x -= 1;

        if (input != Vector2.zero) _dir = input.normalized;
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