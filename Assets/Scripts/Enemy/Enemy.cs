using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IPoolable
{
    [Header("Contact Attack")]
    [SerializeField] private float attackRange = 0.6f;    // 이 거리 안이면 닿은 것으로 봄
    [SerializeField] private float attackInterval = 1f;   // 접촉 피해 간격(초)

    public MonsterData Data { get; private set; }

    // 몬스터의 개별 상태
    public int currentHp;
    public bool isDead;
    private float nextAttackTime;
    // 몬스터 외형 교체(monsterAsset)
    private SpriteRenderer spriteRenderer;

    // 추적할 Player
    public GameObject player;
    private PlayerHealth playerHealth;

    // 보스 공격패턴 목록과, 패턴별 "다음 사용 가능 시각" (같은 인덱스끼리 짝)
    private readonly List<BossAttackData> bossAttacks = new();
    private readonly List<float> bossAttackReadyTimes = new();

    // IPoolable : 풀로 돌아가기 직전에 알림 (스포너가 구독해서 사용)
    public event Action<GameObject> OnBeforeReturn;
    // 몬스터 "처치됨" 알림(보스 등장시 몬스터들 사라질때를 위함)
    public event Action<Enemy> OnDied;
    // 보스 HP 바 갱신용: InGameHUD.SetBossHp(enemy.HpRatio)
    public event Action<Enemy> OnHpChanged;

    public MonsterType Type => Data != null ? Data.Type : MonsterType.Normal;
    public bool IsBoss => Type == MonsterType.Boss;
    public float HpRatio => Data == null ? 0f : (float)currentHp / Data.monsterMaxHealthPoint;

    private void Awake()
    {
        spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
    }

    // 몬스터 초기화
    public void Init(MonsterData data)
    {
        Data = data;
        currentHp = data.monsterMaxHealthPoint;
        isDead = false;
        nextAttackTime = 0f;
        Sprite sprite = Resources.Load<Sprite>(data.monsterAsset);
        if (sprite != null) spriteRenderer.sprite = sprite;

        // 보스면 공격패턴 장착. 첫 사용은 쿨타임만큼 지난 뒤(등장하자마자 몰아치지 않게)
        bossAttacks.Clear();
        bossAttackReadyTimes.Clear();

        // 보스 공격패턴 전체를 훑어 해당 보스(monsterID)의 패턴만 추출
        if (IsBoss)
        {
            var allBossAttacks = GameManager.JsonData.BossAttackDataDic;
            if (allBossAttacks != null)   // 로드 실패 시 null → 패턴 없이 접촉 공격만 함
            {
                foreach (BossAttackData attack in allBossAttacks.Values)
                {
                    // 다른 보스의 패턴은 건너뜀
                    if (attack.monsterID != Data.monsterID) { continue; }   

                    bossAttacks.Add(attack);
                    bossAttackReadyTimes.Add(Time.time + attack.cooldown);
                }
            }
        }
    }

    // player와 PlayerHealth 연결(Init 시점엔 player가 비어 있으므로 따로)
    public void SetTarget(GameObject target)
    {
        player = target;
        playerHealth = target != null ? target.GetComponent<PlayerHealth>() : null;
    }

    // IPoolable: 풀에서 꺼내질 때마다 호출됨
    public void OnSpawn()
    {

    }

    // IPoolable: 풀로 돌아갈 때 정리(참조를 끊음)
    public void OnDespawn()
    {
        player = null;
        playerHealth = null;

        bossAttacks.Clear();
        bossAttackReadyTimes.Clear();
    }

    void Update()
    {
        // 몬스터가 죽은 상태면 Data/player가 없으면 return
        if (isDead || Data == null || player == null) { return; }

        Move();

        Attack();
        if (IsBoss) { UpdateBossAttack(); } 
    }

    // 몬스터의 움직임
    public void Move()
    {
        // 몬스터가 죽은 상태면 return
        if (isDead) { return; }

        // player쪽 방향 찾기
        Vector3 direction = player.transform.position - this.transform.position;
        direction.Normalize();

        this.transform.position += direction * Data.monsterMoveSpeed * Time.deltaTime;
    }

    // 몬스터의 공격(닿아 있는 동안 attackInterval마다 피해)
    public void Attack()
    {
        // 몬스터가 죽은 상태면 return
        if (isDead || playerHealth == null) { return; }
        // 몬스터 다음 공격 시간보다 덜 지났으면 return
        if (Time.time < nextAttackTime) { return; }

        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance > attackRange) { return; }

        playerHealth.GetDamage(Data.monsterAttackPoint);
        nextAttackTime = Time.time + attackInterval;
    }

    // 보스의 공격패턴
    private void UpdateBossAttack()
    {
        if (playerHealth == null) { return; }

        float distance = Vector2.Distance(transform.position, player.transform.position);

        for (int i = 0; i < bossAttacks.Count; i++)
        {
            BossAttackData attack = bossAttacks[i];

            // 쿨타임 중이면 무시
            if (Time.time < bossAttackReadyTimes[i]) { continue; }
            // 사거리 밖이면 무시
            if (distance > attack.range) { continue; }              

            ExecuteBossAttack(attack);
            bossAttackReadyTimes[i] = Time.time + attack.cooldown;
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

    // 몬스터의 피격
    public void Damaged(int damage)
    {
        // 몬스터가 죽은 상태면 return
        if (isDead) { return; }

        currentHp -= damage;
        // 보스 HP 바 갱신
        OnHpChanged?.Invoke(this);   

        // 몬스터가 죽으면
        if (currentHp <= 0)
        {
            Die();
        }
    }

    // 몬스터가 죽었을 때
    public void Die(bool giveReward = true)
    {
        // 몬스터가 죽은 상태면 return
        if (isDead) { return; }

        isDead = true;

        // 보상 (giveReward)
        // - true  (기본) : 플레이어가 잡음 → 경험치 드롭 + OnDied(킬 수 집계)
        // - false        : 보스 등장 정리 등 "강제로 치움" → 보상/킬 없이 반납만
        if (giveReward)
        {
            // TODO : 우선은 ExpGem1 고정 -> monsterExp에 따른 잼 등급 나눠야 함
            GameManager.DropItem.Spawn(DropItemType.ExpGem1, transform.position);
            OnDied?.Invoke(this);
        }

        // 반납 알림
        OnBeforeReturn?.Invoke(gameObject);
        // 몬스터가 죽은 상태면 비활성화(Pool로 반납)
        GameManager.ObjectPool.ReturnObject(gameObject);
    }

    // 테스트용 피격, 죽음
    [ContextMenu("TestDamaged")]
    private void TestDamaged() => Damaged(5);

    [ContextMenu("TestKill")]
    private void TestKill() => Die();
}
