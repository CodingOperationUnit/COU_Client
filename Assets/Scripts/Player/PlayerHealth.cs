using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerHealth : MonoBehaviour
{
    private PlayerStats playerStats;

    private int maxHealth;
    public int MaxHealth => maxHealth;

    [SerializeField] [Min(1)] private int maxLives = 2;
    public int MaxLives => maxLives;

    [SerializeField][Min(0f)] private float invulnerableTime = 0.5f;
    private float invulnerableEndTime;
    public bool IsInvulnerable => Time.time < invulnerableEndTime;

    public int CurrentHealth { get; private set; }
    public int CurrentLives { get; private set; }

    public event Action OnDied;

    public bool IsDead => CurrentLives <= 0;

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
        maxHealth = playerStats.FinalHp; // 뭐로 불러와야하지?

        CurrentHealth = maxHealth;
    }

    public void GetDamage(int damage)
    {
        if (damage <= 0 || IsDead) return;
        if (IsInvulnerable)
        {
            Debug.Log("무적 중 피해무시");
            return;
        }
        if (CurrentHealth - damage <= 0)
        {
            CurrentHealth = 0;
            DecreaseLife();
        }
        else
            CurrentHealth -= damage;

        Debug.Log("현재 체력 : " + CurrentHealth + ", 현재 목숨 : " + CurrentLives);
    }

    public void Heal(int heal)
    {
        if (heal <= 0 || IsDead) return;
        if (CurrentHealth + heal >= maxHealth)
            CurrentHealth = maxHealth;
        else
            CurrentHealth += heal;

        Debug.Log("현재 체력 : " + CurrentHealth);
    }

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
        else
            CurrentLives++;

        Debug.Log("현재 목숨 : " + CurrentLives);
    }

    // For Debug (무적일때 플레이어 주위에 원 (플레이어 무적 시각적 확인))
    private void OnDrawGizmos()
    {
        if (IsInvulnerable)
            Gizmos.DrawWireSphere(transform.position, 1);
    }

    [ContextMenu("TestDamage")]
    public void TestGetDamage()
    {
        GetDamage(testDamage);
    }

    [ContextMenu("TestHeal")]
    public void TestHeal()
    {
        Heal(testHeal);
    }

    [ContextMenu("TestIncreaseLife")]
    public void TestIncreaseLife()
    {
        IncreaseLife();
    }
}
