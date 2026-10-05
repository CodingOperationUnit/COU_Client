using System.Collections.Generic;
using UnityEngine;

// 스테이지의 웨이브(Wave.json)를 진행하고, 패턴(SpawnPattern.json)의 배치 형태로 위치를 정해
// MonsterSpawner에 스폰을 요청한다.
public class WaveManager : MonoBehaviour
{
    [SerializeField] private MonsterSpawner spawner;
    [SerializeField] private Transform player;

    [Header("Stage")]
    [SerializeField] private int stageId = 1;                // 메인 씬을 거치지 않을 때의 기본값

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 10f;        // player로부터의 거리
    [SerializeField] private float minRepeatInterval = 0.1f; // 매 프레임 스폰되지 않게
    [SerializeField] private float lineLength = 6f;          // Line 전체 길이
    [SerializeField] private float clusterRadius = 1.5f;     // Cluster 퍼짐 반경

    [Header("Test")]
    [SerializeField] private int testPatternId = 7;
    [SerializeField] private int testMonsterId = 11001;

    public StageData CurrentStage { get; private set; }
    public float Elapsed => elapsed;

    // 스폰 이벤트 진행 상태
    private class SpawnEventState
    {
        public WaveEntryData data;
        public SpawnPatternData pattern;
        public float nextSpawnTime;
        public bool finished;
    }
    private readonly List<SpawnEventState> eventStates = new();

    // 스테이지 경과 시간
    private float elapsed;

    private void Start()
    {
        int targetStageId = stageId;
        if (GameManager.Scene != null && GameManager.Scene.TryConsumePendingStageId(out int selectedStageId))
        {
            targetStageId = selectedStageId;
        }

        SetupStage(targetStageId);
    }

    // 스테이지 시작
    public void SetupStage(int newStageId)
    {
        stageId = newStageId;
        CurrentStage = GameManager.JsonData.GetStageDataFromJson(stageId);
        elapsed = 0f;

        eventStates.Clear();

        if (CurrentStage == null)   // 조회 실패 시 null (JsonDataManager가 경고 로그를 이미 찍음)
        {
            Debug.LogError($"[WaveManager] stageId {stageId}가 없어 웨이브를 시작할 수 없습니다.");
            return;
        }
        int waveId = CurrentStage.waveId;

        // 전체를 훑으면서 이 스테이지 웨이브의 항목만 골라 담는다
        var allWaveEntries = GameManager.JsonData.WaveEntryDataDic;
        if (allWaveEntries == null)   // 로드 실패 시 null (JsonDataManager가 에러 로그를 이미 찍음)
        {
            Debug.LogError("[WaveManager] Wave 데이터가 없어 스테이지를 시작할 수 없습니다.");
            return;
        }
        foreach (WaveEntryData data in allWaveEntries.Values)
        {
            // 다른 웨이브 항목은 건너뜀
            if (data.waveId != waveId) { continue; }

            SpawnPatternData pattern = GameManager.JsonData.GetSpawnPatternDataFromJson(data.patternId);
            if (pattern == null)
            {
                Debug.LogWarning($"[WaveManager] waveEntryId {data.waveEntryId}의 patternId {data.patternId}를 찾을 수 없어 건너뜁니다.");
                continue;
            }

            // 없는 드롭 테이블은 처치할 때마다 경고가 쌓이므로 시작할 때 한 번만 거른다
            if (pattern.dropTableId > 0 && GameManager.JsonData.GetDropTableFromJson(pattern.dropTableId) == null)
            {
                Debug.LogWarning($"[WaveManager] waveEntryId {data.waveEntryId}의 dropTableId {pattern.dropTableId}를 찾을 수 없어 건너뜁니다.");
                continue;
            }

            eventStates.Add(new SpawnEventState
            {
                data = data,
                pattern = pattern,
                nextSpawnTime = data.patternStartTime,
                finished = false
            });
        }

        if (eventStates.Count == 0)
            Debug.LogWarning($"[WaveManager] stageId {stageId}의 waveId {waveId}에 항목이 없습니다.");
    }

    private void Update()
    {
        if (player == null) { return; }

        elapsed += Time.deltaTime;

        for (int i = 0; i < eventStates.Count; i++)
        {
            if (!eventStates[i].finished)
                ProcessEvent(eventStates[i]);
        }
    }

    // 이벤트 하나 처리(시간이 됐으면 스폰)
    private void ProcessEvent(SpawnEventState state)
    {
        SpawnPatternData pattern = state.pattern;
        bool repeat = pattern.patternDuration > 0f;
        float interval = Mathf.Max(pattern.spawnInterval, minRepeatInterval);

        // 보스전 중에는 보스 이벤트가 아닌 스폰을 멈춤
        // 보스를 잡은 뒤 한꺼번에 몰려나오지 않게 보스전 동안 지나간 스폰은 건너뜀
        if (spawner.IsBossBattle && pattern.EventType != SpawnEventType.Boss)
        {
            if (elapsed >= state.nextSpawnTime)
            {
                if (repeat) state.nextSpawnTime = elapsed + interval;
                else state.finished = true;
            }
            return;
        }

        if (elapsed < state.nextSpawnTime) { return; }

        // 한 번만 나오는 이벤트(엘리트/보스)
        if (!repeat)
        {
            SpawnPattern(state.data.monsterId, pattern);
            state.finished = true;
            return;
        }

        // 반복 이벤트 : [patternStartTime, patternStartTime + patternDuration) 구간 동안 interval마다
        if (elapsed >= state.data.patternStartTime + pattern.patternDuration)
        {
            state.finished = true;
            return;
        }

        SpawnPattern(state.data.monsterId, pattern);
        state.nextSpawnTime += interval;
    }

    // 배치 형태에 따라 위치를 정해 spawnCount마리 스폰
    private void SpawnPattern(int monsterId, SpawnPatternData pattern)
    {
        Vector3 center = player.position;
        int count = pattern.spawnCount;

        // Ring, Line, Cluster는 한 번 스폰할 때 방향을 하나만 뽑아 마리들이 나눠 쓴다
        Vector2 baseDir = RandomDirection();

        for (int i = 0; i < count; i++)
        {
            Vector3 position = pattern.Formation switch
            {
                SpawnFormation.Ring => center + Quaternion.Euler(0f, 0f, 360f * i / count) * baseDir * spawnRadius,
                SpawnFormation.Line => GetLinePosition(center, baseDir, i, count),
                SpawnFormation.Cluster => center + (Vector3)(baseDir * spawnRadius + Random.insideUnitCircle * clusterRadius),
                SpawnFormation.Screen => GetRandomScreenPosition(),
                _ => center + (Vector3)(RandomDirection() * spawnRadius)
            };

            spawner.SpawnMonster(monsterId, position, pattern.dropTableId);
        }
    }

    // 반지름 1인 원 위의 임의의 방향
    private static Vector2 RandomDirection()
    {
        return Random.insideUnitCircle.normalized;
    }

    // 중심 C = center + dir * spawnRadius에서 dir에 수직인 방향으로 양 끝을 포함해 같은 간격으로 배치
    private Vector3 GetLinePosition(Vector3 center, Vector2 dir, int index, int count)
    {
        Vector3 lineCenter = center + (Vector3)(dir * spawnRadius);
        if (count == 1) { return lineCenter; }

        Vector3 along = new Vector3(-dir.y, dir.x, 0f);
        float t = index / (float)(count - 1) - 0.5f;
        return lineCenter + along * (t * lineLength);
    }

    // 카메라 화면 안의 랜덤 위치
    private Vector3 GetRandomScreenPosition()
    {
        Camera cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        Vector3 center = cam.transform.position;

        return new Vector3(
            center.x + Random.Range(-halfWidth, halfWidth),
            center.y + Random.Range(-halfHeight, halfHeight),
            0f);
    }

    // 타임라인과 무관하게 testPatternId 패턴을 testMonsterId로 즉시 한 번 스폰
    [ContextMenu("TestSpawnPattern")]
    private void TestSpawnPattern()
    {
        SpawnPatternData pattern = GameManager.JsonData.GetSpawnPatternDataFromJson(testPatternId);
        if (pattern == null) { return; }

        SpawnPattern(testMonsterId, pattern);
    }
}
