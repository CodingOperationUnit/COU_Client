using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] [Min(1)] private int maxHealth = 100;
    public int MaxHealth => maxHealth;

    public int CurrentHealth { get; private set; }

    [Header("Test")]
    [SerializeField] private int testDamage = 50;
    [SerializeField] private int testHeal = 20;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void GetDamage(int damage)
    {
        if (damage <= 0) return;
        // (임시) 추후 목숨 소진
        if (CurrentHealth - damage <= 0)
            CurrentHealth = 0;
        else
            CurrentHealth -= damage;

        Debug.Log("현재 체력 : " + CurrentHealth);
    }

    public void Heal(int heal)
    {
        if (heal <= 0) return;
        if (CurrentHealth + heal >= maxHealth)
            CurrentHealth = maxHealth;
        else
            CurrentHealth += heal;

        Debug.Log("현재 체력 : " + CurrentHealth);
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


}
