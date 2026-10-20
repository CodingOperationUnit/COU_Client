using UnityEngine;

// 적에게 피해를 주는 스킬 오브젝트(투사체, 범위, 번개, 스피커 등)의 공통 부모
// 이펙트만 있는 오브젝트에는 필요 없는 타격음, 넉백, 타격 시 소환을 여기서 가짐 (이펙트 전용은 SkillEffectObject)
public abstract class SkillAttackObject<TInitData> : SkillObject<TInitData>
{
    // 프리팹마다 인스펙터에서 지정하는 타격음. 이 오브젝트가 적에게 피해를 줄 때마다 재생됨. 비워 두면 소리가 나지 않음
    [SerializeField] private AudioClip _hitSfx;

    // 프리팹마다 인스펙터에서 정하는 넉백 여부. 체크하면 이 오브젝트가 준 피해로 적이 밀려남 (기본은 넉백 없음)
    // 보스와 상자는 체크해도 밀리지 않음 (Enemy의 넉백 저항)
    [Tooltip("체크하면 이 스킬 오브젝트에 맞은 적이 밀려납니다. 보스와 상자는 체크해도 밀리지 않습니다.")]
    [SerializeField] private bool _knockback = false;

    // 이 오브젝트가 적에게 피해를 줄 때마다 적 위치에 소환할 프리팹. 비워 두면 소환하지 않음
    // 공격 스킬이든 이펙트만 있는 프리팹이든 상관없고, 프리팹 최상위의 스크립트(SkillObject<AoEData>)를 지정함
    // 소환된 오브젝트는 맞힌 공격의 수치(SkillStats)와 적 위치를 AoEData로 받아 초기화됨
    [Tooltip("이 스킬 오브젝트가 적에게 피해를 줄 때마다 적 위치에 소환할 프리팹. 공격 스킬이나 이펙트(SkillEffectObject)의 최상위 스크립트를 지정합니다. 비워 두면 소환하지 않습니다.")]
    [SerializeField] private SkillObjectBase _hitSpawnPrefab;

    // 소환할 프리팹이 AoEData를 받을 수 있는지 확인한 결과 (0 = 아직 확인 안 함, 1 = 가능, 2 = 불가). 경고를 한 번만 띄우고 매 타격마다 컴포넌트를 찾지 않기 위함
    private int _hitSpawnState;

    // 서브클래스가 적에게 피해를 줄 때 Enemy.Damaged의 applyKnockback 인자로 넘김
    protected bool KnockbackEnabled => _knockback;

    // 적에게 피해를 준 직후 부르는 함수. 같은 클립이 짧은 시간에 겹치면 SoundManager가 한 번만 재생함
    protected void PlayHitSfx(float volumeScale = 1f, float pitch = 1f)
    {
        if(_hitSfx == null)
        {
            return;
        }

        GameManager.Sound.PlaySFX(_hitSfx, volumeScale, pitch);
    }

    // 적에게 피해를 준 직후 부르는 함수. 소환할 프리팹이 지정되어 있지 않으면 아무것도 하지 않음
    protected void SpawnOnHit(Enemy enemy, SkillStats hitStats)
    {
        if(_hitSpawnPrefab == null || enemy == null)
        {
            return;
        }

        if(_hitSpawnState == 0)
        {
            _hitSpawnState = _hitSpawnPrefab.TryGetComponent(out SkillObject<AoEData> _) ? 1 : 2;

            if(_hitSpawnState == 2)
            {
                Debug.LogWarning($"[SkillAttackObject] 타격 시 소환할 프리팹({_hitSpawnPrefab.name})이 SkillObject<AoEData>가 아니라서 소환하지 않습니다. 범위/이펙트 계열 스크립트(AoeHitObject, SkillEffectObject 등)만 지정할 수 있습니다: {name}");
            }
        }

        if(_hitSpawnState != 1)
        {
            return;
        }

        Vector3 position = enemy.transform.position;
        AoEData data = new AoEData(hitStats, position);

        SkillObjectPool.Instance.Get(_hitSpawnPrefab.gameObject, position, Quaternion.identity, instance =>
        {
            if(instance.TryGetComponent(out SkillObject<AoEData> spawned))
            {
                spawned.Init(data);
            }
        });
    }
}
