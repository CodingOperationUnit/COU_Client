using System.Collections.Generic;
using UnityEngine;

// 사용자를 따라다니며 바라보는 방향(PlayerMovement.FacingDirection)에 범위를 계속 유지하는 오브젝트 (럼블 Q처럼 앞쪽 지속 범위)
// 판정 시점은 스킬이 정함: 스킬이 틱마다 Hit을 부르면 그 순간 콜라이더와 겹친 적 모두에게 한 번씩 피해를 줌 (넉백 없음)
// 풀에 스스로 반환되지 않으며, 스킬이 해제될 때 스킬이 반환함
// 범위의 위치/크기는 프리팹 콜라이더로 정함 (오브젝트 pivot = 사용자 위치, 로컬 right(+X) = 바라보는 방향)
public class FacingFieldObject : SkillObject<FacingFieldData>
{
    [SerializeField] private Collider2D _hitArea;

    private readonly List<Collider2D> _overlaps = new();
    private readonly HashSet<Enemy> _hitEnemies = new();

    private ContactFilter2D _filter;
    private Vector3 _baseScale;
    private bool _initialized;

    private Transform _owner;
    private PlayerMovement _movement;

    protected override void OnInit(FacingFieldData data)
    {
        EnsureInitialized();

        stats = data.Stats;
        _owner = data.Owner;
        _movement = data.Movement;

        OnLaunch();
    }

    protected override void OnLaunch()
    {
        UpdateTransform();
    }

    // 스킬이 틱마다 부름. 그 순간의 수치(대미지, 범위 증가)로 겹친 적 모두에게 한 번씩 피해를 줌
    public void Hit(SkillStats tickStats)
    {
        stats = tickStats;
        UpdateTransform();

        if(_hitArea == null)
        {
            return;
        }

        // 방금 옮기고 회전한 상태가 물리에 반영된 상태에서 판정
        Physics2D.SyncTransforms();

        _overlaps.Clear();
        _hitArea.Overlap(_filter, _overlaps);
        _hitEnemies.Clear();

        foreach(Collider2D other in _overlaps)
        {
            // 콜라이더가 여러 개인 적에게 한 틱에 중복 피해가 들어가지 않도록 함
            if(!other.TryGetComponent(out Enemy enemy) || !_hitEnemies.Add(enemy))
            {
                continue;
            }

            OnHitEnemy(enemy);
        }
    }

    // 판정에 걸린 적 하나에 주는 효과. 지속 범위는 틱마다 넉백하면 적이 범위 밖으로 밀려나므로, 인스펙터의 넉백은 기본(꺼짐)으로 두는 것을 권장
    protected virtual void OnHitEnemy(Enemy enemy)
    {
        PlayerDamageCalculator.HitResult hit = stats.CalculateHit();

        enemy.Damaged(hit.damage, stats.SkillId, KnockbackEnabled);
    }

    // 사용자의 이동이 끝난 뒤의 위치와 방향에 맞추기 위해 LateUpdate에서 갱신
    private void LateUpdate()
    {
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        if(_owner == null)
        {
            return;
        }

        transform.position = _owner.position;

        // 범위 증가 보너스는 프리팹 크기에 곱해서 스프라이트와 판정이 함께 커지게 함
        Vector3 scale = _baseScale * stats.Scale;
        Vector2 facing = _movement != null ? _movement.FacingDirection : Vector2.right;

        if(facing != Vector2.zero)
        {
            transform.right = facing;

            // 왼쪽을 향하면 180도 회전해서 스프라이트가 뒤집혀 보이므로 Y 스케일을 반전해 바로 세움
            if(facing.x < 0.0f)
            {
                scale.y = -scale.y;
            }
        }

        transform.localScale = scale;
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
            Debug.LogWarning($"[FacingFieldObject] 판정용 콜라이더가 없습니다: {name}");
        }
    }
}
