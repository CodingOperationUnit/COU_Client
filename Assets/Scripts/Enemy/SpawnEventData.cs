using System;
using UnityEngine;

// 스폰 이벤트 종류. Spawn.json의 "eventType" 문자열을 enum으로 바꿔서 쓴다.
// - Normal : 일반 반복 스폰
// - Elite  : 엘리트 1회 등장
// - Horde  : 짧은 간격으로 몰려나오는 구간
// - Boss   : 보스 등장 (스포너가 남은 몬스터 정리 + 보스전 중 다른 스폰 정지)
public enum SpawnEventType { Normal, Elite, Horde, Boss }

// "언제, 무엇을, 얼마나 자주, 몇 마리" 뽑을지.
[Serializable]
public class SpawnEventData
{
    public int spawnEventID;
    public int stageID;
    public string eventType;      // "Normal" / "Elite" / "Horde" / "Boss"
    public float startTime;       // 시작 시각(초)
    public float endTime;         // 끝 시각(초). repeat=false면 startTime과 같음
    public int monsterID;         // // 뽑을 몬스터의 ID
    public float spawnInterval;   // 반복 간격(초)
    public int spawnCount;        // 한 번에 뽑는 수
    public bool repeat;           // true: start~end 동안 반복 / false: startTime에 한 번

    [NonSerialized] private SpawnEventType parsedEventType;
    public SpawnEventType EventType => parsedEventType;

    // MonsterDatabase.Load()에서 JSON을 읽은 직후 한 번 호출
    public void OnLoaded()
    {
        if (!Enum.TryParse(eventType, out parsedEventType))
            Debug.LogWarning($"[SpawnEventData] {spawnEventID}의 eventType \"{eventType}\"을(를) 알 수 없습니다.");
    }
}
