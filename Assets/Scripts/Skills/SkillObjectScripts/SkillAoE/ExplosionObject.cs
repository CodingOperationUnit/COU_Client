using System.Collections.Generic;
using UnityEngine;

// 소환된 자리에서 한 번만 대미지를 주는 범위 (폭발 등). 판정 시점에 콜라이더와 겹친 적 모두에게 한 번씩 대미지
// 범위의 모양/크기는 프리팹 콜라이더로 정하고 스프라이트와 맞춤. 애니메이션이 끝나는 시간(_lifeTime)에 풀에 반환
public sealed class ExplosionObject : SkillObject<AoEData>
{
    [SerializeField] private Collider2D _hitArea;

    // 생성 후 대미지 판정까지 걸리는 시간 (폭발 애니메이션 타이밍에 맞춤)
    [SerializeField] private float _hitDelay = 0.0f;

    // 풀에 반환되기까지의 시간 (애니메이션 길이). 판정 시간보다 짧아도 판정은 끝낸 뒤 반환
    [SerializeField] private float _lifeTime = 0.5f;

    private readonly List<Collider2D> _overlaps = new();
    private readonly HashSet<Enemy> _hitEnemies = new();

    private ContactFilter2D _filter;
    private Vector3 _baseScale;
    private bool _initialized;
    private float _elapsed;
    private bool _hasHit;

    protected override void OnInit(AoEData data)
    {
        EnsureInitialized();

        stats = data.Stats;

        // 범위 증가 보너스는 프리팹 크기에 곱해서 스프라이트와 판정이 함께 커지게 함
        transform.localScale = _baseScale * stats.Scale;

        OnLaunch();
    }

    protected override void OnLaunch()
    {
        _elapsed = 0.0f;
        _hasHit = false;
        _hitEnemies.Clear();
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;

        if(!_hasHit && _elapsed >= _hitDelay)
        {
            _hasHit = true;
            Hit();
        }

        if(_elapsed >= Mathf.Max(_lifeTime, _hitDelay))
        {
            ReturnToPool();
        }
    }

    // 풀에서 꺼낸 직후엔 Awake가 아직 불리지 않았을 수 있으므로 첫 Init에서 한 번만 준비
    private void EnsureInitialized()
    {
        if(_initialized)
        {
            return;
        }

        _initialized = true;
        _baseScale = transform.localScale;

        _filter = new ContactFilter2D();
        _filter.SetLayerMask(LayerMask.GetMask("Enemy"));
        _filter.useTriggers = true;

        if(_hitArea == null && !TryGetComponent(out _hitArea))
        {
            Debug.LogWarning($"[ExplosionObject] 판정용 콜라이더가 없습니다: {name}");
        }
    }

    private void Hit()
    {
        if(_hitArea == null)
        {
            return;
        }

        // 방금 배치/크기 변경한 상태가 물리에 반영된 상태에서 판정
        Physics2D.SyncTransforms();

        _overlaps.Clear();
        _hitArea.Overlap(_filter, _overlaps);

        foreach(Collider2D other in _overlaps)
        {
            // 콜라이더가 여러 개인 적에게 중복 대미지가 들어가지 않도록 함
            if(!other.TryGetComponent(out Enemy enemy) || !_hitEnemies.Add(enemy))
            {
                continue;
            }

            enemy.Damaged(stats.Damage);
        }
    }
}
