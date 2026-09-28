using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] [Min(1)] private int maxHealth = 100;
    public int MaxHealth => maxHealth;

    [SerializeField] [Min(1)] private int maxLives = 3;
    public int MaxLives => maxLives;

    public int CurrentHealth { get; private set; }
    public int CurrentLives { get; private set; }

    [Header("Test")]
    [SerializeField] private int testDamage = 50;
    [SerializeField] private int testHeal = 20;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        CurrentLives = maxLives;
    }

    public void GetDamage(int damage)
    {
        if (damage <= 0 || CurrentLives <= 0) return;
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
        if (heal <= 0 || CurrentLives <= 0) return;
        if (CurrentHealth + heal >= maxHealth)
            CurrentHealth = maxHealth;
        else
            CurrentHealth += heal;

        Debug.Log("현재 체력 : " + CurrentHealth);
    }

    private void DecreaseLife()
    {
        CurrentLives--;
        if (CurrentLives <= 0) Debug.Log("플레이어 사망 (추후 사망처리 로직 생성 후 수정)");
        else
            CurrentHealth = maxHealth;
    }

    public void IncreaseLife()
    {
        if (CurrentLives >= maxLives || CurrentLives <= 0)
            return;
        else
            CurrentLives++;

        Debug.Log("현재 목숨 : " + CurrentLives);
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
