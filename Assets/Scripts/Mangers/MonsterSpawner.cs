using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterSpawner : MonoSingleton<MonsterSpawner>
{
    [Header("Prefab")]
    [SerializeField] private GameObject normalPrefab;      // 일반 몬스터용
    [SerializeField] private GameObject eliteBossPrefab;   // 엘리트·보스 공용
    [SerializeField] private GameObject boxPrefab;         // 상자용

    [Header("Player")]
    [SerializeField] private GameObject player;

    [Header("Stage")]
    [SerializeField] private int stageId = 1;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 10f;        // player로부터의 거리
    [SerializeField] private float minRepeatInterval = 0.1f; // 매 프레임 스폰되지 않게

    // 킬 수 집계용
    public event Action<Enemy> OnEnemyKilled;
    // 보스 연출/HUD용
    public event Action<Enemy> OnBossSpawned;
    public event Action<Enemy> OnBossKilled;

    // 스킬이 "가장 가까운 몬스터"를 찾을 때 사용
    public IReadOnlyList<Enemy> SpawnedEnemies => spawnedEnemies;
    public Enemy CurrentBoss { get; private set; }
    public bool IsBossBattle => CurrentBoss != null;
    public StageData CurrentStage { get; private set; }
    public float Elapsed => elapsed;

    // 현재 관리 중인 살아있는 몬스터
    private readonly List<Enemy> spawnedEnemies = new();

    // 스폰 이벤트 진행 상태
    private class SpawnEventState
    {
        public SpawnEventData data;
        public float nextSpawnTime;
        public bool finished;
    }
    private readonly List<SpawnEventState> eventStates = new();

    // 스테이지 경과 시간
    private float elapsed;

    private void Start()
    {
        int targetStageID = stageId;
        if (GameManager.Scene != null && GameManager.Scene.TryConsumePendingStageId(out int selectedStageID))
        {
            targetStageID = selectedStageID;
        }
        
        SetupStage(targetStageID);
    }

    // 스테이지 시작
    public void SetupStage(int newStageId)
    {
        stageId = newStageId;
        CurrentStage = GameManager.JsonData.GetStageDataFromJson(stageId);
        elapsed = 0f;

        eventStates.Clear();

        // 전체를 훑으면서 이 스테이지의 이벤트만 골라 담는다
        var allSpawnEvents = GameManager.JsonData.SpawnEventDataDic;
        if (allSpawnEvents == null)   // 로드 실패 시 null (JsonDataManager가 에러 로그를 이미 찍음)
        {
            Debug.LogError("[MonsterSpawner] Spawn 데이터가 없어 스테이지를 시작할 수 없습니다.");
            return;
        }
        foreach (SpawnEventData data in allSpawnEvents.Values)
        {
            // 다른 스테이지 이벤트는 건너뜀
            if (data.stageID != stageId) { continue; }   

            eventStates.Add(new SpawnEventState
            {
                data = data,
                nextSpawnTime = data.startTime,
                finished = false
            });
        }

        if (eventStates.Count == 0)
            Debug.LogWarning($"[MonsterSpawner] stageID {stageId}의 스폰 이벤트가 없습니다.");
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
        SpawnEventData data = state.data;

        // 보스전 중에는 보스 이벤트가 아닌 스폰을 멈춤
        // 보스를 잡은 뒤 한꺼번에 몰려나오지 않게 보스전 동안 지나간 스폰은 건너뜀
        if (IsBossBattle && data.EventType != SpawnEventType.Boss)
        {
            if (elapsed >= state.nextSpawnTime)
            {
                if (data.repeat) state.nextSpawnTime = elapsed + GetInterval(data);
                else state.finished = true;
            }
            return;
        }

        if (elapsed < state.nextSpawnTime) { return; }

        // 한 번만 나오는 이벤트(엘리트/보스)
        if (!data.repeat)
        {
            SpawnGroup(data);
            state.finished = true;
            return;
        }

        // 반복 이벤트 : [startTime, endTime) 구간 동안 interval마다
        if (elapsed >= data.endTime)
        {
            state.finished = true;
            return;
        }

        SpawnGroup(data);
        state.nextSpawnTime += GetInterval(data);
    }

    private float GetInterval(SpawnEventData data)
        => Mathf.Max(data.spawnInterval, minRepeatInterval);

    // spawnCount만큼 스폰
    private void SpawnGroup(SpawnEventData data)
    {
        for (int i = 0; i < data.spawnCount; i++)
            SpawnMonster(data.monsterID);
    }

    // monsterID로 데이터 조회 -> 타입에 맞는 풀에서 꺼내 Init/SetTarget
    public Enemy SpawnMonster(int monsterId)
    {
        MonsterData data = GameManager.JsonData.GetMonsterDataFromJson(monsterId);
        if (data == null) { return null; }

        // 보스 등장(남아있는 몬스터를 모두 사망 처리(보상 없음))
        if (data.Type == MonsterType.Boss)
            ClearAllEnemies(giveReward: false);

        // 풀(프리팹) 선택
        GameObject prefab = data.Type switch
        {
            MonsterType.Normal => normalPrefab,
            MonsterType.Box => boxPrefab,
            _ => eliteBossPrefab
        };

        // 상자는 움직이지 않으므로 화면 안에, 나머지는 플레이어 주변 원 위에 스폰
        Vector3 spawnPosition = data.Type == MonsterType.Box ? GetRandomScreenPosition() : GetRandomSpawnPosition();

        // 오브젝트 풀에서 몬스터 가져오기
        GameObject obj = GameManager.ObjectPool.GetObject(prefab, spawnPosition, Quaternion.identity);

        // 가져온 GameObject에서 Enemy 컴포넌트 가져오기
        Enemy enemy = obj.GetComponent<Enemy>();

        // 몬스터가 추적할 플레이어 연결
        enemy.Init(data);
        enemy.SetTarget(player);

        // 반납 알림 구독 → 죽으면 목록에서 제거
        // (재사용할 때 중복 구독되지 않도록 먼저 빼고 다시 등록)
        enemy.OnBeforeReturn -= OnEnemyReturned;
        enemy.OnBeforeReturn += OnEnemyReturned;

        enemy.OnDied -= OnEnemyDied;
        enemy.OnDied += OnEnemyDied;

        // 현재 spawner가 관리하는 몬스터 목록에 추가
        spawnedEnemies.Add(enemy);

        if (enemy.IsBoss)
        {
            CurrentBoss = enemy;
            OnBossSpawned?.Invoke(enemy);
        }

        return enemy;
    }

    // 살아 있는 몬스터 전부 사망 처리
    public void ClearAllEnemies(bool giveReward)
    {
        Enemy[] targets = spawnedEnemies.ToArray();
        foreach (Enemy enemy in targets)
        {
            // 상자는 보스 등장에도 지우지 않음
            if (enemy.Type == MonsterType.Box) { continue; }
            enemy.Die(giveReward);
        }
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

    // 플레이어 주변 원 위의 랜덤 위치
    private Vector3 GetRandomSpawnPosition()
    {
        // 반지름 1인 원 안에서 임의의 방향을 구함
        Vector2 dir = Random.insideUnitCircle.normalized;
        // 플레이어로부터 일정한 거리를 확보하여 최종 스폰 위치를 계산
        Vector3 spawnPosition = player.transform.position + (Vector3)(dir * spawnRadius);

        return spawnPosition;
    }

    // 처치 알림
    private void OnEnemyDied(Enemy enemy)
    {
        OnEnemyKilled?.Invoke(enemy);

        if (enemy == CurrentBoss)
        {
            CurrentBoss = null;          // 보스전 종료 → 일반 스폰 재개
            OnBossKilled?.Invoke(enemy);
        }
    }

    // 몬스터가 풀로 돌아가기 직전 목록에서 제거
    private void OnEnemyReturned(GameObject obj)
    {
        // 반납 알림을 보낸 GameObject에서 Enemy 컴포넌트 가져오기
        Enemy enemy = obj.GetComponent<Enemy>();

        // 스포너가 관리하는 목록에서 해당 몬스터 제거
        spawnedEnemies.Remove(enemy);

        // 보스가 처치가 아닌 이유로 반납된 경우도 보스전 상태를 풀어줌
        if (enemy == CurrentBoss) CurrentBoss = null;
    }
}

