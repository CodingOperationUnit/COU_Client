using DG.Tweening;
using UnityEngine;

// 던진 순간의 타겟 위치로 포물선을 그리며 날아가고, 도착하면 폭발(_explosionPrefab)을 소환한 뒤 풀에 반환
// 날아가는 동안에는 적과 부딪히지 않음 (타겟을 따라가지 않고, 타겟이 죽어도 정한 위치에서 터짐)
public sealed class CpuProjectile : SkillProjectile
{
    // 도착하면 소환할 폭발. SkillObject<AoEData>가 붙은 프리팹이어야 함 (AoeHitObject 등)
    [SerializeField] private SkillObjectBase _explosionPrefab;

    // 포물선 높이 (월드 단위)
    [SerializeField] private float _jumpPower = 1.5f;

    // 시트 속도가 0일 때 쓰는 비행 시간
    [SerializeField] private float _defaultFlightTime = 0.6f;

    // 아주 가까운 적에게 던져도 포물선이 보이도록 하는 최소 비행 시간
    [SerializeField] private float _minFlightTime = 0.2f;

    private Tween _flightTween;

    protected override void OnLaunch()
    {
        // 타겟이 없으면(정면 조준 등) 발사 방향으로 1만큼 떨어진 곳에 던짐
        Vector3 destination = target != null ? target.position : transform.position + (Vector3)direction;
        destination.z = transform.position.z;

        float distance = Vector2.Distance(transform.position, destination);
        float duration = stats.Speed > 0.0f ? distance / stats.Speed : _defaultFlightTime;

        KillFlight();

        _flightTween = transform.DOJump(destination, _jumpPower, 1, Mathf.Max(duration, _minFlightTime))
            .SetEase(Ease.Linear)
            .OnComplete(Explode);
    }

    // 풀로 반환되거나 비활성화되면 남은 트윈을 정리 (다음에 꺼냈을 때 이전 트윈이 이어서 돌지 않도록)
    private void OnDisable()
    {
        KillFlight();
    }

    private void Explode()
    {
        // 완료 콜백 안에서 자신을 다시 Kill하지 않도록 먼저 비움
        _flightTween = null;

        SpawnExplosion();
        ReturnToPool();
    }

    private void SpawnExplosion()
    {
        if(_explosionPrefab == null)
        {
            Debug.LogWarning($"[CpuProjectile] 폭발 프리팹이 없습니다: {name}");
            return;
        }

        Vector3 position = transform.position;
        AoEData data = new AoEData(stats, position);

        SkillObjectPool.Instance.Get(_explosionPrefab.gameObject, position, Quaternion.identity, instance =>
        {
            if(instance.TryGetComponent(out SkillObject<AoEData> explosion))
            {
                explosion.Init(data);
            }
        });
    }

    private void KillFlight()
    {
        if(_flightTween != null)
        {
            _flightTween.Kill();
            _flightTween = null;
        }
    }
}
