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

        if (Keyboard.current.digit7Key.wasPressedThisFrame)
            EquipSkill(1);

        if (Keyboard.current.digit8Key.wasPressedThisFrame)
            EquipSkill(2);

        if (Keyboard.current.digit9Key.wasPressedThisFrame)
            EquipSkill(3);
    }

    private void SpawnAllDropItems()
    {
        Vector2 center = playerHealth.transform.position;

        for (int i = 0; i <= (int)DropItemType.Bomb; i++)
            GameManager.DropItem.Spawn((DropItemType)i, center + Random.insideUnitCircle * dropSpawnRange);
    }

    private void EquipSkill(int skillId)
    {
        var player = PlayerManager.Instance;
        if (player == null || player.Skills == null)
        {
            Debug.LogWarning("[PlayerDebug] SkillController를 찾을 수 없습니다.");
            return;
        }

        bool owned = false;
        foreach (var skill in player.Skills.ActiveSkills)
        {
            if (skill.SkillId == skillId)
            {
                owned = true;
                break;
            }
        }

        // 보유 스킬은 레벨업, 미보유 스킬은 장착
        // (스킬 담당자 규칙: EquipSkill은 첫 장착 때만, 이후 레벨업은 LevelUpSkill)
        bool result = owned
            ? player.Skills.LevelUpSkill(skillId)
            : player.Skills.EquipSkill(skillId);

        Debug.Log("[PlayerDebug] 스킬 " + skillId + (owned ? " 레벨업: " : " 장착: ") + (result ? "성공" : "실패"));
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

        string skills = "";
        if (player.Skills != null)
        {
            foreach (var skill in player.Skills.ActiveSkills)
                skills += skill.SkillId + "(Lv" + skill.Level + ") ";
        }

        Debug.Log("[PlayerDebug] PlayerManager 확인" +
                  " / 장비 출처: " + player.Stats.EquipmentSource +
                  " / 방향: " + player.Movement.FacingDirection +
                  " / 공격력: " + player.Stats.FinalAtk +
                  " / 체력: " + player.Health.CurrentHealth + "/" + player.Health.MaxHealth +
                  " / 사망: " + player.Health.IsDead +
                  " / 무기: " + (player.Stats.HasWeapon ? player.Stats.EquippedWeaponName : "없음") +
                  " / 시작 스킬: " + player.Stats.StartingSkillId +
                  " / 스킬: " + (skills == "" ? "없음" : skills) +
                  " / 스킬 동작: " + (player.Skills != null && player.Skills.enabled)
                  );
    }

    private void OnGUI()
    {
        if (!Debug.isDebugBuild) return;

        GUI.skin.label.fontSize = 24;
        GUI.Label(new Rect(10, 10, 500, 420),
            "[1] Damage\n[2] Heal\n[3] Restore Life\n[4] Attack Motion\n[5] Spawn Drop Items\n[6] PlayerManager Log\n[7][8][9] Equip Skill 1/2/3\n\n" +
            "HP : " + playerHealth.CurrentHealth + " / " + playerHealth.MaxHealth + "\n\n" +
            "Life : " + playerHealth.CurrentLives + " / " + playerHealth.MaxLives);
    }
}