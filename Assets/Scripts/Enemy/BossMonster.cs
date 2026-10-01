using System.Collections.Generic;
using UnityEngine;

public class BossMonster : Enemy
{
    // 보스 공격패턴 목록과, 패턴별 "다음 사용 가능 시각" (같은 인덱스끼리 짝)
    private readonly List<BossAttackData> bossAttacks = new();
    private readonly List<float> bossAttackReadyTimes = new();

    // 보스 몬스터 초기화
    protected override void OnInit()
    {
        // 보스면 공격패턴 장착. 첫 사용은 쿨타임만큼 지난 뒤(등장하자마자 몰아치지 않게)
        bossAttacks.Clear();
        bossAttackReadyTimes.Clear();

        // 보스 공격패턴 전체를 훑어 해당 보스(monsterID)의 패턴만 추출
        var allBossAttacks = GameManager.JsonData.BossAttackDataDic;
        if (allBossAttacks == null) { return; }   // 로드 실패 → 접촉 공격만 함

        foreach (BossAttackData attack in allBossAttacks.Values)
        {
            // 다른 보스의 패턴은 건너뜀
            if (attack.monsterID != Data.monsterID) { continue; }

            bossAttacks.Add(attack);
            bossAttackReadyTimes.Add(Time.time + attack.cooldown);
        }
    }

    // IPoolable: 풀로 돌아갈 때 정리
    protected override void OnDespawned()
    {
        bossAttacks.Clear();
        bossAttackReadyTimes.Clear();
    }

    // 보스의 공격패턴
    protected override void Tick()
    {
        if (playerHealth == null) { return; }

        float distance = Vector2.Distance(transform.position, player.transform.position);

        for (int i = 0; i < bossAttacks.Count; i++)
        {
            // 쿨타임 중이면 무시
            if (Time.time < bossAttackReadyTimes[i]) { continue; }
            // 사거리 밖이면 무시
            if (distance > bossAttacks[i].range) { continue; }

            ExecuteBossAttack(bossAttacks[i]);
            bossAttackReadyTimes[i] = Time.time + bossAttacks[i].cooldown;
            break;
        }
    }

    // 패턴별 실행. 지금은 "사거리 안이면 즉시 피해"로 통일하고, 연출/투사체는 TODO로 남긴다.
    private void ExecuteBossAttack(BossAttackData attack)
    {
        switch (attack.AttackType)
        {
            case BossAttackType.Melee:
                // TODO : 휘두르기 애니메이션
                playerHealth.GetDamage(attack.damage);
                break;

            case BossAttackType.Ranged:
                // TODO : 투사체 프리팹을 GameManager.ObjectPool에서 꺼내 플레이어 방향으로 발사
                //        (지금은 투사체가 없으므로 임시로 즉시 피해)
                playerHealth.GetDamage(attack.damage);
                break;

            case BossAttackType.Area:
                // TODO : 바닥 경고 표시 → 일정 시간 뒤 range 안이면 피해
                playerHealth.GetDamage(attack.damage);
                break;
        }

        Debug.Log($"[Boss] {Data.monsterName} - {attack.AttackType} 공격 ({attack.damage})");
    }
}
