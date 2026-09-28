
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoSingleton<MonsterSpawner>
{
    [Header("Monster")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Player")]
    [SerializeField] private GameObject player;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 1f;  // 스폰 간격(초)
    [SerializeField] private float spawnRadius = 10f;   // 플레이어로부터의 거리

    // 현재 관리 중인 살아있는 몬스터
    private readonly List<Enemy> spawnedEnemies = new();

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < spawnInterval) { return; }

        timer = 0f;

        Spawn();
    }

    // 타이머 Coroutine 사용 시
    //private Coroutine spawnCoroutine;

    //private void Start()
    //{
    //    spawnCoroutine = StartCoroutine(SpawnRoutine());
    //}

    //private IEnumerator SpawnRoutine()
    //{
    //    while (true)
    //    {
    //        Spawn();

    //        yield return new WaitForSeconds(spawnInterval);
    //    }
    //}

    private void Spawn()
    {
        // 플레이어 주변 원 위의 랜덤 위치 계산
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // 오브젝트 풀에서 몬스터 가져오기(OnSpawn → Init 자동 호출)
        // 풀에 사용 가능한 몬스터가 있으면 재사용하고, 없으면 Prefab을 기반으로 새로 생성함
        GameObject obj = GameManager.ObjectPool.GetObject(enemyPrefab, spawnPosition, Quaternion.identity);

        // 가져온 GameObject에서 Enemy 컴포넌트 가져오기
        Enemy enemy = obj.GetComponent<Enemy>();
        // 몬스터가 추적할 플레이어 연결
        enemy.player = player;   

        // 반납 알림 구독 → 죽으면 목록에서 제거
        // (재사용할 때 중복 구독되지 않도록 먼저 빼고 다시 등록)
        enemy.OnBeforeReturn -= OnEnemyReturned;
        enemy.OnBeforeReturn += OnEnemyReturned;
        // 현재 spawner가 관리하는 몬스터 목록에 추가
        spawnedEnemies.Add(enemy);
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

    // 몬스터가 풀로 돌아가기 직전에 호출되는 이벤트 메서드
    private void OnEnemyReturned(GameObject obj)
    {
        // 반납 알림을 보낸 GameObject에서 Enemy 컴포넌트 가져오기
        Enemy enemy = obj.GetComponent<Enemy>();

        // 스포너가 관리하는 목록에서 해당 몬스터 제거
        spawnedEnemies.Remove(enemy);
    }
}

