using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private HPBar hpBar;

    private PlayerStats playerStats;

    private int maxHealth;
    public int MaxHealth => maxHealth;

    [SerializeField][Min(1)] private int maxLives = 2;
    public int MaxLives => maxLives;

    [SerializeField][Min(0f)] private float invulnerableTime = 0.5f;
    private float invulnerableEndTime;
    public bool IsInvulnerable => Time.time < invulnerableEndTime;

    public int CurrentHealth { get; private set; }
    public int CurrentLives { get; private set; }

    public event Action OnDied;

    public bool IsDead => CurrentLives <= 0;

    public event Action OnDamaged;

    [Header("Test")]
    [SerializeField] private int testDamage = 50;
    [SerializeField] private int testHeal = 20;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        CurrentLives = maxLives;
    }

    private void Start()
    {
        maxHealth = playerStats.FinalHp;
        CurrentHealth = maxHealth;
        UpdateHpBar();
    }

    public void GetDamage(int damage)
    {
        if (damage <= 0 || IsDead) return;
        if (IsInvulnerable)
        {
            Debug.Log("무적 중 피해무시");
            return;
        }

        // 방어력 적용 (모든 받는 피해가 여기로 들어오므로 몬스터·보스 코드 수정 불필요)
        int finalDamage = PlayerDamageCalculator.ApplyDefense(damage, playerStats.FinalDefense);

        OnDamaged?.Invoke();

        if (CurrentHealth - finalDamage <= 0)
        {
            CurrentHealth = 0;
            DecreaseLife();
        }
        else
            CurrentHealth -= finalDamage;

        UpdateHpBar();
        Debug.Log("피해 " + damage + " → " + finalDamage + " (방어력 " + playerStats.FinalDefense + ")" +
                  ", 현재 체력 : " + CurrentHealth + ", 현재 목숨 : " + CurrentLives);
    }

    public void Heal(int heal)
    {
        if (heal <= 0 || IsDead) return;

        if (CurrentHealth + heal >= maxHealth)
            CurrentHealth = maxHealth;
        else
            CurrentHealth += heal;

        UpdateHpBar();
        Debug.Log("현재 체력 : " + CurrentHealth);
    }

    private void UpdateHpBar()
        => hpBar.SetRatio(maxHealth > 0 ? (float)CurrentHealth / maxHealth : 0f);

    private void DecreaseLife()
    {
        CurrentLives--;

        if (IsDead)
        {
            Debug.Log("플레이어 사망");
            OnDied?.Invoke();
        }
        else
        {
            CurrentHealth = maxHealth;
            invulnerableEndTime = Time.time + invulnerableTime;
        }
    }

    public void IncreaseLife()
    {
        if (CurrentLives >= maxLives || IsDead)
            return;

        CurrentLives++;
        Debug.Log("현재 목숨 : " + CurrentLives);
    }

    // For Debug (무적일 때 플레이어 주위에 원 표시)
    private void OnDrawGizmos()
    {
        if (IsInvulnerable)
            Gizmos.DrawWireSphere(transform.position, 1);
    }

    [ContextMenu("TestDamage")]
    private void TestGetDamage()
    {
        GetDamage(testDamage);
    }

    [ContextMenu("TestHeal")]
    private void TestHeal()
    {
        Heal(testHeal);
    }

    [ContextMenu("TestIncreaseLife")]
    private void TestIncreaseLife()
    {
        IncreaseLife();
    }
}