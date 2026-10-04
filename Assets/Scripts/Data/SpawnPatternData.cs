using System;

// 스폰 이벤트 종류. SpawnPattern.json의 "eventType" 문자열을 enum으로 바꿔서 쓴다.
// - Normal : 일반 반복 스폰
// - Elite  : 엘리트 1회 등장
// - Horde  : 짧은 간격으로 몰려나오는 구간
// - Boss   : 보스 등장 (스포너가 남은 몬스터 정리 + 보스전 중 다른 스폰 정지)
public enum SpawnEventType { Normal, Elite, Horde, Boss }

// 스폰 배치 형태. SpawnPattern.json의 "formation" 문자열을 enum으로 바꿔서 쓴다.
// - Random  : 마리마다 플레이어 주변 원 위의 무작위 위치
// - Ring    : 플레이어 주변 원 위에 같은 간격
// - Line    : 한쪽에 플레이어 방향과 수직인 직선
// - Cluster : 원 위의 한 지점 주변에 뭉침
// - Screen  : 카메라 화면 안의 무작위 위치
public enum SpawnFormation { Random, Ring, Line, Cluster, Screen }

// "어떤 모양으로, 몇 마리를, 얼마나 자주" 뽑을지. Wave.json의 patternID가 참조한다.
[Serializable]
public class SpawnPatternData
{
    public int patternID;
    public string eventType;      // "Normal" / "Elite" / "Horde" / "Boss"
    public string formation;      // "Random" / "Ring" / "Line" / "Cluster" / "Screen"
    public int spawnCount;        // 한 번 스폰할 때의 마릿수
    public float spawnInterval;   // 반복 간격(초). duration이 0이면 쓰지 않음
    public float duration;        // 반복 지속 시간(초). 0이면 startTime에 한 번
    public int dropTableID;       // DropTable.json의 테이블. 0이면 드롭 없음

    [NonSerialized] private SpawnEventType parsedEventType;
    public SpawnEventType EventType => parsedEventType;

    [NonSerialized] private SpawnFormation parsedFormation;
    public SpawnFormation Formation => parsedFormation;

    // JsonDataManager에서 JSON을 읽은 직후 한 번 호출. 둘 중 하나라도 알 수 없으면 false
    public bool OnLoaded()
        => Enum.TryParse(eventType, out parsedEventType) && Enum.TryParse(formation, out parsedFormation);
}
