using System;
using UnityEngine;

// 보스 공격 종류. BossAttack.json의 "attackType" 문자열을 enum으로 바꿔서 쓴다.
// - Melee  : 근접 공격(돌진)
// - Ranged : 원거리 공격(산탄)
// - Area   : 범위 공격(경고 장판 폭발)
public enum BossAttackType { Melee, Ranged, Area }

// 보스 공격 패턴(한 보스가 여러 패턴을 가짐)
[Serializable]
public class BossAttackData
{
    public int bossAttackId;
    public int monsterId;         // 이 패턴을 쓰는 보스
    public string attackType;     // "Melee" / "Ranged" / "Area"
    public float cooldown;        // 재사용 대기시간(초)
    public float range;           // 이 거리 안에 플레이어가 있어야 사용
    public int damage;

    [NonSerialized] private BossAttackType parsedAttackType;
    public BossAttackType AttackType => parsedAttackType;

    // JsonDataManager에서 JSON을 읽은 직후 한 번 호출
    public void OnLoaded()
    {
        if (!Enum.TryParse(attackType, out parsedAttackType))
            Debug.LogWarning($"[BossAttackData] {bossAttackId}의 attackType \"{attackType}\"을(를) 알 수 없습니다.");
    }
}