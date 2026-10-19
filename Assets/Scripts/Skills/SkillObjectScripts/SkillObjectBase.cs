using UnityEngine;

// 모든 스킬 오브젝트(투사체, 지속형 등)의 비제네릭 부모
// SkillPrefabTable처럼 종류와 상관없이 스킬 오브젝트를 하나의 타입으로 다뤄야 할 때 사용
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

    // 프리팹마다 인스펙터에서 지정하는 타격음. 이 오브젝트가 적에게 피해를 줄 때마다 재생됨. 비워 두면 소리가 나지 않음
    [SerializeField] private AudioClip _hitSfx;

    // 적에게 피해를 준 직후 부르는 함수. 같은 클립이 짧은 시간에 겹치면 SoundManager가 한 번만 재생함
    protected void PlayHitSfx(float volumeScale = 1f, float pitch = 1f)
    {
        if(_hitSfx == null)
        {
            return;
        }

        GameManager.Sound.PlaySFX(_hitSfx, volumeScale, pitch);
    }

    // 프리팹마다 인스펙터에서 정하는 넉백 여부. 체크하면 이 오브젝트가 준 피해로 적이 밀려남 (기본은 넉백 없음)
    // 보스와 상자는 체크해도 밀리지 않음 (Enemy의 넉백 저항)
    [Tooltip("체크하면 이 스킬 오브젝트에 맞은 적이 밀려납니다. 보스와 상자는 체크해도 밀리지 않습니다.")]
    [SerializeField] private bool _knockback = false;

    // 서브클래스가 적에게 피해를 줄 때 Enemy.Damaged의 applyKnockback 인자로 넘김
    protected bool KnockbackEnabled => _knockback;
}
