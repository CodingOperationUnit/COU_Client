using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerVisual))]
[RequireComponent(typeof(PlayerLootReceiver))]
[RequireComponent(typeof(SkillController))]
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public PlayerMovement Movement { get; private set; }
    public PlayerHealth Health { get; private set; }
    public PlayerStats Stats { get; private set; }
    public PlayerVisual Visual { get; private set; }
    public PlayerLootReceiver Loot { get; private set; }
    public SkillController Skills { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[PlayerManager] 씬에 PlayerManager가 두 개 이상 있습니다.", this);
            return;
        }

        Instance = this;

        Movement = GetComponent<PlayerMovement>();
        Health = GetComponent<PlayerHealth>();
        Stats = GetComponent<PlayerStats>();
        Visual = GetComponent<PlayerVisual>();
        Loot = GetComponent<PlayerLootReceiver>();
        Skills = GetComponent<SkillController>();
    }

    private void OnEnable()
    {
        if (Health != null)
            Health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (Health != null)
            Health.OnDied -= HandleDied;
    }

    private void HandleDied()
    {
        if (Skills != null)
            Skills.enabled = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}