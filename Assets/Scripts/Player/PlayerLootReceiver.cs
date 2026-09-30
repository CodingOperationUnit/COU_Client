using System;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerLootReceiver : MonoBehaviour, ILootReceiver
{
    [SerializeField][Min(0)] private int potionHealAmount = 300; // 임시 값: 기획 확정 후 조정

    private PlayerHealth playerHealth;
    private PlayerStats playerStats;

    // Potion, Magnet을 제외한 아이템 획득 알림 (경험치, 골드, 상자, 폭탄)
    public event Action<DropItemType> OnLooted;

    public Vector2 Position => transform.position;
    public float LootRadius => playerStats.LootRadius;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void OnEnable()
    {
        GameManager.DropItem.RegisterReceiver(this);
        playerHealth.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        // 가이드: OnDisable/OnDestroy에서는 GameManager.DropItem을 호출하지 않는다
        playerHealth.OnDied -= HandleDied;
    }

    public void OnLoot(DropItemType type)
    {
        switch (type)
        {
            case DropItemType.Potion:
                playerHealth.Heal(potionHealAmount);
                break;

            case DropItemType.Magnet:
                GameManager.DropItem.AttractAllExpGems();
                break;

            default:
                OnLooted?.Invoke(type);
                break;
        }
    }

    private void HandleDied()
    {
        GameManager.DropItem.UnregisterReceiver(this);
    }

    // For Debug (루팅 범위 표시)
    private void OnDrawGizmos()
    {
        if (playerStats == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, playerStats.LootRadius);
    }
}