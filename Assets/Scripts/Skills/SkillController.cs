using System.Collections.Generic;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    private const int MaxSkillSlots = 3;

    private readonly List<SkillBase> _activeSkills = new();

    public IReadOnlyList<SkillBase> ActiveSkills => _activeSkills;

    private void Update()
    {
        foreach( SkillBase skill in _activeSkills )
        {
            skill.ReduceCooldown( Time.deltaTime );

            if( skill.CanActivate() )
            {
                skill.Activate();
            }
        }
    }

    public bool EquipSkill( int skillId )
    {
        if( _activeSkills.Count >= MaxSkillSlots )
        {
            // TODO: 슬롯 초과 시 교체/선택 정책은 추후 UI 연동 후 확정 (현재는 실패 처리)
            Debug.LogWarning( $"[SkillController] 슬롯이 가득 차 스킬을 장착할 수 없습니다: {skillId}" );
            return false;
        }

        SkillBase skill = SkillFactory.Create( skillId );

        if( skill == null )
        {
            return false;
        }

        _activeSkills.Add( skill );
        return true;
    }

    public bool UnequipSkill( int skillId )
    {
        SkillBase skill = _activeSkills.Find( s => s.SkillId == skillId );

        if( skill == null )
        {
            return false;
        }

        return _activeSkills.Remove( skill );
    }
}
