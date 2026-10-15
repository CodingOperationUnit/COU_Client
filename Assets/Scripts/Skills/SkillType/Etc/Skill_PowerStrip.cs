using System.Collections.Generic;
using UnityEngine;

// 탐지 범위 안의 모든 적에게 번개(LightningObject)를 하나씩 소환. 번개가 생기는 순간 대미지가 들어감
// 연쇄 패시브가 있으면 맞은 적에게서 아직 안 맞은 가까운 적에게 번개가 이어짐
// 발동 조건(쿨타임, 범위 안에 적이 있는지)은 CooldownSkillBase를 따름
public sealed class Skill_PowerStrip : CooldownSkillBase
{
    // 연쇄 번개가 다음 적을 찾는 반경 (직전에 맞은 적 기준). 임시 수치
    private const float ChainRadius = 3.0f;

    private readonly List<Enemy> _initialTargets = new();

    // 이번 발동에서 이미 번개를 맞은 적 (콜라이더가 여러 개인 적에게 겹쳐 나가거나, 연쇄가 같은 적에게 돌아가는 것 방지)
    private readonly HashSet<Enemy> _struckEnemies = new();

    protected override void Fire()
    {
        // 번개마다 같은 수치를 쓰므로 한 번만 계산
        SkillStats skillStats = BuildStats();
        int chainCount = GetFlatStat(SkillFlatStat.Chain, 0);

        _initialTargets.Clear();
        _struckEnemies.Clear();

        // 탐색 결과 리스트는 공유 버퍼라 연쇄 탐색 중에 덮어써지므로, 처음 대상은 따로 모아 둠
        foreach(Collider2D other in SkillTargeting.OverlapEnemies(transform.position, Range))
        {
            if(other.TryGetComponent(out Enemy enemy) && !enemy.isDead && _struckEnemies.Add(enemy))
            {
                _initialTargets.Add(enemy);
            }
        }

        foreach(Enemy enemy in _initialTargets)
        {
            Spawn(transform.position, new LinkData(skillStats, transform, enemy.transform));

            if(chainCount > 0)
            {
                Chain(enemy, chainCount, skillStats);
            }
        }
    }

    // from에서 시작해 chainCount번 다음 적으로 번개를 이음. 이을 적이 없으면 거기서 멈춤
    private void Chain(Enemy from, int chainCount, SkillStats skillStats)
    {
        Enemy current = from;

        for(int i = 0; i < chainCount; i++)
        {
            Enemy next = SkillTargeting.FindNearestEnemy(current.transform.position, ChainRadius, _struckEnemies);

            if(next == null)
            {
                return;
            }

            _struckEnemies.Add(next);
            Spawn(current.transform.position, new LinkData(skillStats, current.transform, next.transform));
            current = next;
        }
    }
}
