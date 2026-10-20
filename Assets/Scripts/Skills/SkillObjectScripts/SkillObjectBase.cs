using UnityEngine;

// 모든 스킬 오브젝트의 비제네릭 부모. 종류와 상관없이 하나의 타입으로 다뤄야 할 때 사용 (SkillPrefabTable, 타격 시 소환 프리팹 등)
// 공격 오브젝트(SkillAttackObject)와 이펙트 전용 오브젝트(SkillEffectObject)가 공통으로 가진 것만 둠: 소환될 때 재생되는 효과음
public abstract class SkillObjectBase : MonoBehaviour
{
    // 프리팹마다 인스펙터에서 지정하는 효과음. 비워 두면 PlaySfx를 불러도 소리가 나지 않음
    [SerializeField] private AudioClip _sfx;

    // 지정된 효과음을 SoundManager로 재생. 소리를 낼 순간에 서브클래스가 부름
    protected void PlaySfx(float volumeScale = 1f, float pitch = 1f)
    {
        if(_sfx == null)
        {
            return;
        }

        GameManager.Sound.PlaySFX(_sfx, volumeScale, pitch);
    }
}
