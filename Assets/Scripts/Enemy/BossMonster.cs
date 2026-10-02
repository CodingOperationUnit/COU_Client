using System.Collections.Generic;
using UnityEngine;

public class BossMonster : Enemy
{
    // 보스의 현재 상태
    private enum BossState
    {
        Chase,      // 플레이어 추적
        Idle,       // 잠깐 멈춤
        WindUp,     // 패턴 예고
        Attacking,  // 패턴 발동 중
        Recovery    // 후딜레이
    }

    [Header("Movement Rhythm")]
    [SerializeField] private Vector2 chaseDurationRange = new(2f, 3.5f);   // 추적 지속 시간
    [SerializeField] private Vector2 idleDurationRange = new(0.6f, 1.2f);  // 멈춤 지속 시간

    [Header("Pattern Timing")]
    [SerializeField] private float windupTime = 0.8f;     // 패턴 예고 시간
    [SerializeField] private float recoveryTime = 0.6f;   // 후딜레이
    [SerializeField] private Color warningColor = new(1f, 0.3f, 0.3f);  // 예고 중 깜빡일 색
    [SerializeField] private float blinkSpeed = 10f;      // 깜빡이는 속도

    [Header("Melee : Dash")]
    [SerializeField] private float dashSpeed = 12f;       // 돌진 속도
    [SerializeField] private float dashDistance = 6f;     // 돌진 거리
    [SerializeField] private float dashHitRadius = 0.8f;  // 돌진 중 이 거리 안이면 맞은 것으로 봄

    [Header("Ranged : Spread")]
    [SerializeField] private GameObject projectilePrefab; // BossProjectile.prefab
    [SerializeField] private int projectileCount = 5;     // 발 수
    [SerializeField] private float spreadAngle = 60f;     // 퍼지는 전체 각도
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private float projectileLifeTime = 4f; // 이 시간이 지나면 사라짐

    [Header("Area : Warning Blast")]
    [SerializeField] private GameObject areaWarningPrefab; // BossAreaWarning.prefab
    [SerializeField] private int areaCount = 3;            // 장판 개수
    [SerializeField] private float areaRadius = 1.5f;      // 폭발 반지름
    [SerializeField] private float areaSpread = 3f;        // 플레이어 주변으로 흩어지는 범위

    // 보스 공격패턴 목록과, 패턴별 "다음 사용 가능 시각" (같은 인덱스끼리 짝)
    private readonly List<BossAttackData> bossAttacks = new();
    private readonly List<float> bossAttackReadyTimes = new();

    // 쓸 수 있는 패턴의 인덱스를 잠깐 담는 목록
    private readonly List<int> readyIndexBuffer = new();

    private BossState state;
    private float stateTimer;               // 현재 상태의 남은 시간
    private BossAttackData currentAttack;   // 지금 쓰고 있는 패턴

    private Vector3 dashDirection;          // 돌진 방향 (예고 시작 때 고정됨)
    private float dashRemaining;            // 남은 돌진 거리
    private bool dashHit;                   // 이번 돌진에서 이미 맞혔는지

    private Color originalColor;            // 깜빡임 후 되돌릴 원래 색

    protected override void Awake()
    {
        base.Awake();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    // 보스 몬스터 초기화
    protected override void OnInit()
    {
        // 보스면 공격패턴 장착. 첫 사용은 쿨타임만큼 지난 뒤(등장하자마자 몰아치지 않게)
        bossAttacks.Clear();
        bossAttackReadyTimes.Clear();

        // 풀에서 재사용될 때 이전 전투의 상태가 남지 않게 초기화
        currentAttack = null;
        RestoreColor();
        EnterState(BossState.Chase);

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

    // 보스 몬스터 색상 초기화
    private void RestoreColor()
    {
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    // IPoolable: 풀로 돌아갈 때 정리
    protected override void OnDespawned()
    {
        bossAttacks.Clear();
        bossAttackReadyTimes.Clear();
        currentAttack = null;
        RestoreColor();
    }

    // 보스의 공격패턴
    protected override void Tick()
    {
        stateTimer -= Time.deltaTime;

        switch (state)
        {
            // 플레이어 추적
            case BossState.Chase:

            // 잠깐 멈춤
            case BossState.Idle:
                // 움직이든 멈춰 있든, 쓸 수 있는 패턴이 있으면 바로 예고 시작
                if (TryStartAttack()) { return; }

                // 시간이 다 되면 추적 <-> 멈춤 전환
                if (stateTimer <= 0f)
                    EnterState(state == BossState.Chase ? BossState.Idle : BossState.Chase);
                break;

            // 패턴 예고
            case BossState.WindUp:
                BlinkWarning();
                if (stateTimer <= 0f)
                {
                    RestoreColor();
                    ExecuteAttack();
                }
                break;

            // 패턴 발동 중
            case BossState.Attacking:
                // 순간형 패턴(산탄, 장판)은 바로 Recovery로 감 -> 돌진만 남음
                UpdateDash();
                break;

            // 후딜레이
            case BossState.Recovery:
                if (stateTimer <= 0f) EnterState(BossState.Chase);
                break;
        }
    }

    // 상태를 바꾸고, 그 상태가 얼마나 지속될지 결정
    private void EnterState(BossState next)
    {
        state = next;
        stateTimer = next switch
        {
            BossState.Chase => Random.Range(chaseDurationRange.x, chaseDurationRange.y),
            BossState.Idle => Random.Range(idleDurationRange.x, idleDurationRange.y),
            BossState.WindUp => windupTime,
            BossState.Recovery => recoveryTime,
            _ => 0f,   // Attacking은 시간 대신 "돌진 거리"로 끝남
        };
    }

    // 쿨타임이 끝났고 사거리 안인 패턴 중 하나를 랜덤으로 골라 예고 시작
    private bool TryStartAttack()
    {
        if (playerHealth == null || bossAttacks.Count == 0) { return false; }

        float distance = Vector2.Distance(transform.position, player.transform.position);

        readyIndexBuffer.Clear();
        for (int i = 0; i < bossAttacks.Count; i++)
        {
            if (Time.time < bossAttackReadyTimes[i]) { continue; }   // 쿨타임 중
            if (distance > bossAttacks[i].range) { continue; }       // 사거리 밖 (range = 사용 조건 거리)
            readyIndexBuffer.Add(i);
        }

        if (readyIndexBuffer.Count == 0) { return false; }

        int index = readyIndexBuffer[Random.Range(0, readyIndexBuffer.Count)];
        currentAttack = bossAttacks[index];
        bossAttackReadyTimes[index] = Time.time + currentAttack.cooldown;   // 쿨타임은 "예고 시작" 기준

        StartWindUp();
        return true;
    }

    // 패턴 예고 시작
    private void StartWindUp()
    {
        EnterState(BossState.WindUp);

        switch (currentAttack.AttackType)
        {
            case BossAttackType.Melee:
                // 돌진 방향을 "지금" 고정시킴
                dashDirection = (player.transform.position - transform.position).normalized;
                if (dashDirection.sqrMagnitude < 0.0001f) dashDirection = Vector3.right;   // 완전히 겹쳐 있을 때 대비
                break;

            case BossAttackType.Area:
                // 경고 원은 예고 시작과 동시에 깔고, windupTime 뒤에 스스로 터짐
                SpawnAreaWarnings();
                break;
        }
    }
    
    // 패턴 예고 연출
    private void BlinkWarning()
    {
        if (spriteRenderer == null) { return; }

        float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        spriteRenderer.color = Color.Lerp(originalColor, warningColor, t);
    }

    // 몬스터의 공격(패턴)
    private void ExecuteAttack()
    {
        Debug.Log($"[Boss] {Data.monsterName} - {currentAttack.AttackType} 발동");

        switch (currentAttack.AttackType)
        {
            // 돌진
            case BossAttackType.Melee:
                dashRemaining = dashDistance;
                dashHit = false;
                EnterState(BossState.Attacking);   // 돌진은 여러 프레임에 걸쳐 이동
                break;

            // 산탄
            case BossAttackType.Ranged:
                FireSpread();
                EnterState(BossState.Recovery);
                break;

            // 경고 장판 폭발
            case BossAttackType.Area:
                EnterState(BossState.Recovery);
                break;
        }
    }

    // [Melee] 돌진 : 고정된 방향으로 dashDistance만큼 빠르게 이동, 경로에서 한 번만 피해
    private void UpdateDash()
    {
        float step = dashSpeed * Time.deltaTime;
        if (step > dashRemaining) step = dashRemaining;   // 마지막 프레임에 거리를 넘지 않게

        transform.position += dashDirection * step;
        dashRemaining -= step;

        if (!dashHit &&
            Vector2.Distance(transform.position, player.transform.position) <= dashHitRadius)
        {
            dashHit = true;
            playerHealth?.GetDamage(currentAttack.damage);
        }

        if (dashRemaining <= 0f) EnterState(BossState.Recovery);
    }

    // [Ranged] 산탄 : 플레이어 방향을 가운데로 두고, spreadAngle 안에 projectileCount발을 고르게 뿌림
    private void FireSpread()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[Boss] projectilePrefab이 연결되지 않았습니다.");
            return;
        }

        Vector2 baseDir = ((Vector2)(player.transform.position - transform.position)).normalized;
        if (baseDir == Vector2.zero) baseDir = Vector2.right;

        // TODO(내일 시트 수정 후) : int count = currentAttack.count > 0 ? currentAttack.count : projectileCount;
        int count = Mathf.Max(1, projectileCount);
        // TODO(내일 시트 수정 후) : float angle = currentAttack.angle > 0f ? currentAttack.angle : spreadAngle;
        float angle = spreadAngle;

        // 360도(원형)일 때는 첫 발과 마지막 발이 겹치지 않도록 count로 나눔
        bool fullCircle = angle >= 360f;
        float stepAngle = count <= 1 ? 0f : (fullCircle ? 360f / count : angle / (count - 1));
        float startAngle = (count <= 1 || fullCircle) ? 0f : -angle / 2f;

        for (int i = 0; i < count; i++)
        {
            Vector2 dir = Quaternion.Euler(0f, 0f, startAngle + stepAngle * i) * baseDir;

            GameObject obj = GameManager.ObjectPool.GetObject(projectilePrefab, transform.position, Quaternion.identity);
            obj.GetComponent<BossProjectile>()?.Launch(
                dir, projectileSpeed, currentAttack.damage, projectileLifeTime,
                player.transform, playerHealth);
        }
    }

    // [Area] 경고 장판 폭발 : 첫 번째는 플레이어 발밑, 나머지는 주변에 흩뿌림
    private void SpawnAreaWarnings()
    {
        if (areaWarningPrefab == null)
        {
            Debug.LogWarning("[Boss] areaWarningPrefab이 연결되지 않았습니다.");
            return;
        }

        // TODO(내일 시트 수정 후) : int count = currentAttack.count > 0 ? currentAttack.count : areaCount;
        int count = Mathf.Max(1, areaCount);

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = i == 0 ? Vector2.zero : Random.insideUnitCircle * areaSpread;
            Vector3 position = player.transform.position + (Vector3)offset;

            GameObject obj = GameManager.ObjectPool.GetObject(areaWarningPrefab, position, Quaternion.identity);
            obj.GetComponent<BossAreaWarning>()?.Begin(
                areaRadius, windupTime, currentAttack.damage,
                player.transform, playerHealth);
        }
    }

    // 보스 몬스터의 움직임("추적 상태일 때만" 이동)
    protected override void Move()
    {
        // 추적 상태가 아니면 return
        if (state != BossState.Chase) { return; }

        base.Move();   
    }
}
