using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDebug : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private int testDamage = 50;
    [SerializeField] private int testHeal = 20;
    [SerializeField] private float dropSpawnRange = 5f;

    private PlayerVisual playerVisual;
    private PlayerLootReceiver lootReceiver;

    private void Awake()
    {
        if (playerHealth == null)
        {
            Debug.LogError("[PlayerDebug] Player Health가 연결되지 않았습니다. Inspector에서 Player를 연결하세요.", this);
            enabled = false;
            return;
        }

        playerVisual = playerHealth.GetComponent<PlayerVisual>();
        lootReceiver = playerHealth.GetComponent<PlayerLootReceiver>();
    }

    private void OnEnable()
    {
        if (lootReceiver != null)
            lootReceiver.OnLooted += LogLooted;
    }

    private void OnDisable()
    {
        if (lootReceiver != null)
            lootReceiver.OnLooted -= LogLooted;
    }

    private void Update()
    {
        if (!Debug.isDebugBuild) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            playerHealth.GetDamage(testDamage);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            playerHealth.Heal(testHeal);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            playerHealth.IncreaseLife();

        if (Keyboard.current.digit4Key.wasPressedThisFrame && playerVisual != null)
            playerVisual.PlayAttackMotion();

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
            SpawnAllDropItems();

        if (Keyboard.current.digit6Key.wasPressedThisFrame)
            LogPlayerManager();
    }

    private void SpawnAllDropItems()
    {
        Vector2 center = playerHealth.transform.position;

        for (int i = 0; i <= (int)DropItemType.Bomb; i++)
            GameManager.DropItem.Spawn((DropItemType)i, center + Random.insideUnitCircle * dropSpawnRange);
    }

    private void LogLooted(DropItemType type)
    {
        Debug.Log("[PlayerDebug] OnLooted: " + type);
    }

    private void LogPlayerManager()
    {
        var player = PlayerManager.Instance;

        if (player == null)
        {
            Debug.LogWarning("[PlayerDebug] PlayerManager.Instance가 없습니다.");
            return;
        }

        Debug.Log("[PlayerDebug] PlayerManager 확인" +
                  " / 장비 출처: " + (player.Stats.UsesInventory ? "인벤토리" : "더미") +
                  " / 방향: " + player.Movement.FacingDirection +
                  " / 공격력: " + player.Stats.FinalAtk +
                  " / 치명타 확률: " + player.Stats.CriticalChance + "%" +
                  " / 체력: " + player.Health.CurrentHealth + "/" + player.Health.MaxHealth +
                  " / 사망: " + player.Health.IsDead +
                  " / 루팅 범위: " + player.Loot.LootRadius +
                  " / 무기: " + (player.Stats.HasWeapon ? player.Stats.EquippedWeaponName : "없음"));
    }

    private void OnGUI()
    {
        if (!Debug.isDebugBuild) return;

        GUI.skin.label.fontSize = 24;
        GUI.Label(new Rect(10, 10, 500, 360),
            "[1] Damage\n[2] Heal\n[3] Restore Life\n[4] Attack Motion\n[5] Spawn Drop Items\n[6] PlayerManager Log\n\n" +
            "HP : " + playerHealth.CurrentHealth + " / " + playerHealth.MaxHealth + "\n\n" +
            "Life : " + playerHealth.CurrentLives + " / " + playerHealth.MaxLives);
    }
}