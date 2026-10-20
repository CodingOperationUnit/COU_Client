using System;
using System.Collections.Generic;
using UnityEngine;

// 탐지 범위 안의 모든 적에게 번개(LightningObject)를 하나씩 소환. 번개가 생기는 순간 대미지가 들어감
// 연쇄 패시브가 있으면 맞은 적에게서 아직 안 맞은 가까운 적에게 번개가 이어짐
// 발동 조건(쿨타임, 범위 안에 적이 있는지)은 CooldownSkillBase를 따름
// 진화: 기본 번개는 그대로 두고, 쿨타임과 따로 StrikeInterval마다 탐지 범위 안에서 가장 가까운 적 StrikeCount명에게 낙뢰를 내리침 (케넨 궁극기처럼)
//       낙뢰는 SkillPrefabTable의 EvolvedPrefab으로 소환함 (AoeHitObject가 붙은 프리팹, 적 위치에 소환되며 판정 타이밍/범위는 프리팹이 정함)
//       기본 번개는 진화 후에도 기본 프리팹으로 소환함
public sealed class Skill_PowerStrip : CooldownSkillBase
{
    // 연쇄 번개가 다음 적을 찾는 반경 (직전에 맞은 적 기준). 임시 수치
    private const float ChainRadius = 3.0f;

    // 장착할 때 미리 만들어 둘 번개 수 (첫 발동에 Instantiate가 몰리지 않도록). 임시 수치
    private const int PrewarmCount = 64;

    // 진화 낙뢰 간격 (초). 임시 수치
    private const float StrikeInterval = 1.0f;

    // 낙뢰 한 번에 맞힐 적 수 (가까운 순). 임시 수치
    private const int StrikeCount = 3;

    // 낙뢰 대미지 비율 (1 = 기본 번개 한 번과 같은 대미지). 임시 수치
    private const float StrikeDamageRatio = 1.0f;

    // 진화할 때 미리 만들어 둘 낙뢰 수. 임시 수치
    private const int StrikePrewarmCount = 16;

    private struct StrikeTarget
    {
        public Enemy Enemy;
        public float SqrDistance;
    }

    // 가까운 순 정렬에 쓰는 비교 함수 (정렬할 때마다 델리게이트를 새로 만들지 않도록 캐시)
    private static readonly Comparison<StrikeTarget> CompareByDistance = (a, b) => a.SqrDistance.CompareTo(b.SqrDistance);

    private readonly List<StrikeTarget> _strikeTargets = new();
    private readonly HashSet<Enemy> _strikeRegistered = new();
    private float _strikeTimer;

    // 진화 프리팹이 비어 있다는 경고를 한 번만 띄우기 위함
    private bool _isStrikeUnavailable;

    // 이번 발동에서 번개를 맞을 수 있는 적. 연쇄 대상을 물리 탐색 없이 이 목록에서 거리 비교로 찾음
    private struct Candidate
    {
        public Enemy Enemy;
        public Vector2 Position;
        public bool IsStruck;
    }

    private readonly List<Candidate> _candidates = new();

    // _candidates 중 탐지 범위 안에 있어 처음 번개를 맞는 적의 인덱스
    private readonly List<int> _initialIndices = new();

    // 콜라이더가 여러 개인 적이 후보에 중복으로 들어가지 않도록 함
    private readonly HashSet<Enemy> _registeredEnemies = new();

    protected override void OnEquip()
    {
        base.OnEquip();

        SkillObjectPool.Instance.Prewarm(prefab, PrewarmCount);
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        if(IsEvolved)
        {
            TickStrike(deltaTime);
        }
    }

    // 진화하자마자 첫 낙뢰가 나가도록 함
    protected override void OnEvolved()
    {
        _strikeTimer = 0.0f;

        if(evolvedPrefab != null)
        {
            SkillObjectPool.Instance.Prewarm(evolvedPrefab, StrikePrewarmCount);
        }
    }

    protected override void Fire()
    {
        // 번개마다 같은 수치를 쓰므로 한 번만 계산
        SkillStats skillStats = BuildStats();
        int chainCount = GetFlatStat(SkillFlatStat.Chain, 0);

        CollectCandidates(chainCount);

        foreach(int index in _initialIndices)
        {
            Spawn(prefab, transform.position, new LinkData(skillStats, transform, _candidates[index].Enemy.transform));

            if(chainCount > 0)
            {
                Chain(index, chainCount, skillStats);
            }
        }
    }

    // 물리 탐색은 최대 두 번만 함: 탐지 범위(처음 대상), 연쇄가 닿을 수 있는 최대 범위(연쇄 후보)
    private void CollectCandidates(int chainCount)
    {
        _candidates.Clear();
        _initialIndices.Clear();
        _registeredEnemies.Clear();

        float range = Range;

        // 공유 버퍼를 순회하는 중에는 다시 탐색하지 않음 (순회가 끝난 뒤 다음 탐색)
        foreach(Collider2D other in SkillTargeting.OverlapEnemies(transform.position, range))
        {
            if(TryRegister(other, true))
            {
                _initialIndices.Add(_candidates.Count - 1);
            }
        }

        if(chainCount <= 0 || _initialIndices.Count == 0)
        {
            return;
        }

        // 연쇄는 탐지 범위 끝의 적에서 시작해 한 번에 ChainRadius씩 뻗어 나가므로 그 바깥은 닿을 수 없음
        float chainReach = range + ChainRadius * chainCount;

        foreach(Collider2D other in SkillTargeting.OverlapEnemies(transform.position, chainReach))
        {
            TryRegister(other, false);
        }
    }

    // 살아 있고 아직 등록되지 않은 적이면 후보에 추가. isStruck은 처음 번개를 맞는 대상인지
    private bool TryRegister(Collider2D other, bool isStruck)
    {
        if(!other.TryGetComponent(out Enemy enemy) || enemy.isDead || !_registeredEnemies.Add(enemy))
        {
            return false;
        }

        _candidates.Add(new Candidate
        {
            Enemy = enemy,
            Position = enemy.transform.position,
            IsStruck = isStruck
        });

        return true;
    }

    // fromIndex의 적에서 시작해 chainCount번 다음 적으로 번개를 이음. 이을 적이 없으면 거기서 멈춤
    private void Chain(int fromIndex, int chainCount, SkillStats skillStats)
    {
        int currentIndex = fromIndex;

        for(int i = 0; i < chainCount; i++)
        {
            int nextIndex = FindNearestCandidate(_candidates[currentIndex].Position);

            if(nextIndex < 0)
            {
                return;
            }

            Candidate next = _candidates[nextIndex];
            next.IsStruck = true;
            _candidates[nextIndex] = next;

            Transform from = _candidates[currentIndex].Enemy.transform;
            Spawn(prefab, _candidates[currentIndex].Position, new LinkData(skillStats, from, next.Enemy.transform));

            currentIndex = nextIndex;
        }
    }

    // position에서 ChainRadius 안에 있고 아직 번개를 맞지 않은 가장 가까운 후보의 인덱스. 없으면 -1
    private int FindNearestCandidate(Vector2 position)
    {
        int nearestIndex = -1;
        float minSqrDistance = ChainRadius * ChainRadius;

        for(int i = 0; i < _candidates.Count; i++)
        {
            Candidate candidate = _candidates[i];

            // 앞선 번개에 죽은 적은 건너뜀
            if(candidate.IsStruck || candidate.Enemy.isDead)
            {
                continue;
            }

            float sqrDistance = (candidate.Position - position).sqrMagnitude;

            if(sqrDistance <= minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    // 진화 낙뢰 타이머. 범위 안에 적이 없으면 타이머를 0에 두고 기다렸다가 적이 들어오는 즉시 내리침
    private void TickStrike(float deltaTime)
    {
        if(_isStrikeUnavailable)
        {
            return;
        }

        if(_strikeTimer > 0.0f)
        {
            _strikeTimer -= deltaTime;
            return;
        }

        if(Strike())
        {
            _strikeTimer = StrikeInterval;
        }
    }

    // 탐지 범위 안에서 가장 가까운 적부터 StrikeCount명에게 낙뢰를 소환. 맞힐 적이 없으면 false
    private bool Strike()
    {
        if(evolvedPrefab == null)
        {
            Debug.LogWarning($"[Skill_PowerStrip] 진화 프리팹(SkillPrefabTable의 EvolvedPrefab)이 비어 있어 낙뢰가 나가지 않습니다: {SkillId}");
            _isStrikeUnavailable = true;
            return false;
        }

        CollectStrikeTargets();

        if(_strikeTargets.Count == 0)
        {
            return false;
        }

        SkillStats strikeStats = BuildStrikeStats();
        int count = Mathf.Min(StrikeCount, _strikeTargets.Count);

        for(int i = 0; i < count; i++)
        {
            Vector2 position = _strikeTargets[i].Enemy.transform.position;

            Spawn(evolvedPrefab, position, new AoEData(strikeStats, position));
        }

        return true;
    }

    // 탐지 범위 안의 살아 있는 적을 가까운 순으로 _strikeTargets에 담음
    private void CollectStrikeTargets()
    {
        _strikeTargets.Clear();
        _strikeRegistered.Clear();

        Vector2 origin = transform.position;

        // 공유 버퍼를 순회하는 중에는 다시 탐색하지 않음
        foreach(Collider2D other in SkillTargeting.OverlapEnemies(origin, Range))
        {
            // 콜라이더가 여러 개인 적이 중복으로 들어가지 않도록 함
            if(!other.TryGetComponent(out Enemy enemy) || enemy.isDead || !_strikeRegistered.Add(enemy))
            {
                continue;
            }

            _strikeTargets.Add(new StrikeTarget
            {
                Enemy = enemy,
                SqrDistance = ((Vector2)enemy.transform.position - origin).sqrMagnitude
            });
        }

        _strikeTargets.Sort(CompareByDistance);
    }

    // 현재 수치에 낙뢰 대미지 비율만 곱한 수치
    private SkillStats BuildStrikeStats()
    {
        SkillStats skillStats = BuildStats();

        return new SkillStats(skillStats.SkillId, skillStats.SkillMultiplier * StrikeDamageRatio, skillStats.Speed, skillStats.Scale, skillStats.PlayerStats, skillStats.IsEvolved);
    }
}
