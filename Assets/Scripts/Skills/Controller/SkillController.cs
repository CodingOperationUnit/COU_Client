using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillController : MonoBehaviour
{
    // 액티브 슬롯 수 (외부에서 참조하므로 이름 유지)
    public const int MaxSkillSlots = 6;

    // 패시브 슬롯 수
    public const int MaxPassiveSlots = 6;

    private readonly List<SkillBase> _activeSkills = new();
    private readonly List<SkillBase> _passiveSkills = new();

    public PlayerMovement PlayerMovement { get; private set; }
    public PlayerStats PlayerStats { get; private set; }

    // 패시브가 등록한 보너스. 모든 장착 스킬이 같은 인스턴스를 공유함
    public SkillModifiers Modifiers { get; } = new();

    // 장착된 액티브 스킬만 (패시브는 PassiveSkills)
    public IReadOnlyList<SkillBase> ActiveSkills => _activeSkills;
    public IReadOnlyList<SkillBase> PassiveSkills => _passiveSkills;

    private void Awake()
    {
        PlayerMovement = GetComponent<PlayerMovement>();
        PlayerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        foreach(SkillBase skill in _activeSkills)
        {
            skill.Tick(Time.deltaTime);
        }

        foreach(SkillBase skill in _passiveSkills)
        {
            skill.Tick(Time.deltaTime);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Press 1: Shuriken, 2: Revolver, 3: Katana
        UpdateDebugInput();
#endif
    }

    public bool EquipSkill(int skillId)
    {
        // 이미 장착한 스킬은 슬롯이 가득 차 있어도 레벨업할 수 있어야 하므로 슬롯 검사보다 먼저 확인
        SkillBase existing = FindSkill(skillId);

        if(existing != null)
        {
            existing.Levelup();
            return true;
        }

        SkillBase skill = SkillFactory.Create(skillId);

        if(skill == null)
        {
            return false;
        }

        bool isPassive = skill is PassiveSkillBase;
        List<SkillBase> slots = isPassive ? _passiveSkills : _activeSkills;
        int maxSlots = isPassive ? MaxPassiveSlots : MaxSkillSlots;

        if(slots.Count >= maxSlots)
        {
            Debug.LogWarning($"[SkillController] {(isPassive ? "패시브" : "액티브")} 슬롯이 가득 차 스킬을 장착할 수 없습니다: {skillId} ({slots.Count}/{maxSlots})");
            return false;
        }

        skill.SetContext(new SkillContext(transform, PlayerMovement, PlayerStats, Modifiers));

        // 패시브는 소환할 오브젝트가 없으므로 프리팹을 찾지 않음 (찾으면 미등록 경고가 뜸)
        if(!isPassive)
        {
            skill.SetPrefab(SkillManager.Instance.GetPrefab(skillId));
        }

        // 최초 레벨 설정 후 장착해야 OnEquip에서 현재 레벨 기준으로 동작할 수 있음
        skill.Levelup();
        skill.Equip();
        slots.Add(skill);
        return true;
    }

    public bool LevelUpSkill(int skillId)
    {
        SkillBase skill = FindSkill(skillId);

        if(skill == null || skill.Level >= SkillBase.MaxLevel)
        {
            return false;
        }

        skill.Levelup();
        return true;
    }

    public bool UnequipSkill(int skillId)
    {
        SkillBase skill = FindSkill(skillId);

        if(skill == null)
        {
            return false;
        }

        skill.Unequip();

        return _activeSkills.Remove(skill) || _passiveSkills.Remove(skill);
    }

    // 액티브/패시브 구분 없이 장착된 스킬을 찾음
    private SkillBase FindSkill(int skillId)
    {
        SkillBase skill = _activeSkills.Find(s => s.SkillId == skillId);

        return skill ?? _passiveSkills.Find(s => s.SkillId == skillId);
    }

    // For Debug (장착된 스킬별 탐지 사거리 표시)
    private void OnDrawGizmos()
    {
        Color[] colors = { Color.cyan, Color.yellow, Color.magenta };

        for(int i = 0; i < _activeSkills.Count; i++)
        {
            // 패시브처럼 사거리가 없는 스킬은 표시하지 않음
            if(_activeSkills[i].Range <= 0.0f)
            {
                continue;
            }

            Gizmos.color = colors[i % colors.Length];
            Gizmos.DrawWireSphere(transform.position, _activeSkills[i].Range);
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // 테스트
    private void UpdateDebugInput()
    {
        Keyboard keyboard = Keyboard.current;

        if(keyboard == null)
        {
            return;
        }

        if(keyboard.digit1Key.wasPressedThisFrame)
        {
            if(EquipSkill(20010))
            {
                Debug.Log("스킬: Shuriken");
            }
        }

        if(keyboard.digit2Key.wasPressedThisFrame)
        {
            if(EquipSkill(20020))
            {
                Debug.Log("스킬: Revolver");
            }
        }

        if(keyboard.digit3Key.wasPressedThisFrame)
        {
            if(EquipSkill(20030))
            {
                Debug.Log("스킬: Katana");
            }
        }

        if (keyboard.digit4Key.wasPressedThisFrame)
        {
            if (EquipSkill(20040))
            {
                Debug.Log("스킬: Planet");
            }
        }

        // 패시브 (5: 투사체 수 증가, 6: 대미지 증가)
        if(keyboard.digit5Key.wasPressedThisFrame)
        {
            if(EquipSkill(21010))
            {
                Debug.Log("패시브: ProjectileUp");
            }
        }

        if(keyboard.digit6Key.wasPressedThisFrame)
        {
            if(EquipSkill(21070))
            {
                Debug.Log("패시브: DamageUp");
            }
        }
    }
#endif
}
