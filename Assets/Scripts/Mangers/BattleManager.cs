using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private int victorySeconds = 900;
    [SerializeField] private int baseRequiredExp = 20;
    [SerializeField] private int requiredExpIncrement = 6;

    [Header("Test")]
    [SerializeField] private int testExp;

    private readonly HashSet<object> pauseRequests = new();

    private InGameHUD hud;
    private BattleResultWindow resultWindow;

    private float elapsed;
    private int seconds;
    private int kills;
    private int level = 1;
    private int exp;
    private int pendingLevelUps;
    private bool ended;

    private int RequiredExp => baseRequiredExp + requiredExpIncrement * (level - 1);

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        playerHealth.OnDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        playerHealth.OnDied -= HandlePlayerDied;
    }

    private void Start()
    {
        hud = UIManager.Instance.Get<InGameHUD>();
        resultWindow = UIManager.Instance.Get<BattleResultWindow>();

        hud.SetTime(0);
        hud.SetKillCount(0);
        hud.SetLevel(level);
        hud.SetExp(0f);
    }

    private void Update()
    {
        if (ended) return;

        elapsed += Time.deltaTime;
        var current = (int)elapsed;
        if (current == seconds) return;
        seconds = current;

        hud.SetTime(seconds);

        if (seconds >= victorySeconds)
            EndBattle(true);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Time.timeScale = 1f;
    }

    public void RequestPause(object requester)
    {
        pauseRequests.Add(requester);
        Time.timeScale = 0f;
    }

    public void ReleasePause(object requester)
    {
        pauseRequests.Remove(requester);
        if (pauseRequests.Count == 0)
            Time.timeScale = 1f;
    }

    public void AddExp(int amount)
    {
        if (ended) return;

        exp += amount;
        while (exp >= RequiredExp)
        {
            exp -= RequiredExp;
            level++;
            pendingLevelUps++;
        }

        hud.SetLevel(level);
        hud.SetExp(exp / (float)RequiredExp);
    }

    public void AddKill()
    {
        if (ended) return;

        kills++;
        hud.SetKillCount(kills);
    }

    private void HandlePlayerDied()
    {
        EndBattle(false);
    }

    private void EndBattle(bool victory)
    {
        if (ended) return;
        ended = true;

        RequestPause(resultWindow);
        resultWindow.SetTime(seconds);
        resultWindow.SetKillCount(kills);
        resultWindow.Show(victory);
    }

    [ContextMenu("TestAddExp")]
    public void TestAddExp()
    {
        AddExp(testExp);
    }
}
