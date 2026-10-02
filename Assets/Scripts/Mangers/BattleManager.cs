using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private const int OptionCount = 3;

    public static BattleManager Instance { get; private set; }

    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerLootReceiver lootReceiver;
    [SerializeField] private SkillController skillController;
    [SerializeField] private MonsterSpawner spawner;
    [SerializeField] private int[] skillPoolIds = { 1, 2, 3 };
    [SerializeField] private int expGem1Value = 10;
    [SerializeField] private int[] goldValues = { 10, 30, 100, 300 }; // 임시 값: Gold1~4, 기획 확정 후 조정
    [SerializeField] private int accountExpPerKill = 1;       // 임시 값: 기획 확정 후 조정
    [SerializeField] private int accountExpPerSecond = 1;     // 임시 값: 기획 확정 후 조정
    [SerializeField] private int accountExpClearBonus = 500;  // 임시 값: 기획 확정 후 조정
    [SerializeField] private int baseRequiredExp = 20;
    [SerializeField] private int requiredExpIncrement = 6;
    [SerializeField] private float healRewardRatio = 0.3f;
    [SerializeField] private float bossVictoryDelay = 2f;

    [Header("Test")]
    [SerializeField] private int testExp;

    private readonly HashSet<object> pauseRequests = new();
    private readonly List<int> candidates = new();

    private InGameHUD hud;
    private BattleResultWindow resultWindow;
    private PauseWindow pauseWindow;
    private SkillSelectWindow selectWindow;

    private float elapsed;
    private int seconds;
    private int kills;
    private int gold;
    private int rewardBoxes;
    private int level = 1;
    private int exp;
    private int pendingLevelUps;
    private bool ended;
    private float victoryTimer;

    private int RequiredExp => baseRequiredExp + requiredExpIncrement * (level - 1);

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        playerHealth.OnDied += HandlePlayerDied;
        lootReceiver.OnLooted += HandleLooted;
        spawner.OnEnemyKilled += HandleEnemyKilled;
        spawner.OnBossKilled += HandleBossKilled;
    }

    private void OnDisable()
    {
        playerHealth.OnDied -= HandlePlayerDied;
        lootReceiver.OnLooted -= HandleLooted;
        spawner.OnEnemyKilled -= HandleEnemyKilled;
        spawner.OnBossKilled -= HandleBossKilled;
    }

    private void Start()
    {
        hud = UIManager.Instance.Get<InGameHUD>();
        resultWindow = UIManager.Instance.Get<BattleResultWindow>();
        pauseWindow = UIManager.Instance.Get<PauseWindow>();
        selectWindow = UIManager.Instance.Get<SkillSelectWindow>();

        selectWindow.OnSelected += HandleSkillSelected;

        hud.SetTime(0);
        hud.SetKillCount(0);
        hud.SetGold(0);
        hud.SetLevel(level);
        hud.SetExp(0f);
    }

    private void Update()
    {
        if (ended) return;

        if (victoryTimer > 0f)
        {
            victoryTimer -= Time.deltaTime;
            if (victoryTimer <= 0f)
            {
                EndBattle(true);
                return;
            }
        }

        elapsed += Time.deltaTime;
        var current = (int)elapsed;
        if (current == seconds) return;
        seconds = current;

        hud.SetTime(seconds);
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

    public void Surrender()
    {
        EndBattle(false);
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

        if (pendingLevelUps > 0 && !selectWindow.IsOpen)
            ShowSelection();
    }

    public void AddKill()
    {
        if (ended) return;

        kills++;
        hud.SetKillCount(kills);
        pauseWindow.SetKillCount(kills);
    }

    public void AddGold(int amount)
    {
        if (ended) return;

        gold += amount;
        hud.SetGold(gold);
        pauseWindow.SetGold(gold);
    }

    private void HandleEnemyKilled(Enemy enemy)
    {
        AddKill();
    }

    private void HandleBossKilled(Enemy boss)
    {
        victoryTimer = bossVictoryDelay;
    }

    private void HandleLooted(DropItemType type)
    {
        if (type == DropItemType.ExpGem1)
            AddExp(expGem1Value);
        else if (type >= DropItemType.Gold1 && type <= DropItemType.Gold4)
            AddGold(goldValues[type - DropItemType.Gold1]);
        else if (type == DropItemType.RewardBox && !ended)
            rewardBoxes++;
    }

    private void HandlePlayerDied()
    {
        EndBattle(false);
    }

    private void ShowSelection()
    {
        candidates.Clear();
        foreach (var skillId in skillPoolIds)
        {
            var skill = FindActiveSkill(skillId);
            var available = skill != null
                ? skill.Level < SkillBase.MaxLevel
                : skillController.ActiveSkills.Count < SkillController.MaxSkillSlots;
            if (available)
                candidates.Add(skillId);
        }

        // 부분 Fisher–Yates: 앞의 count개를 무작위 후보로 채운다
        var count = Mathf.Min(candidates.Count, OptionCount);
        for (var i = 0; i < count; i++)
        {
            var pick = Random.Range(i, candidates.Count);
            (candidates[i], candidates[pick]) = (candidates[pick], candidates[i]);

            var skill = FindActiveSkill(candidates[i]);
            var data = SkillDataBase.Get(candidates[i]);
            selectWindow.SetOption(i, data.Name, data.Description, skill == null ? 1 : skill.Level + 1, skill == null);
        }

        if (count == 0)
        {
            selectWindow.SetOption(0, "회복", $"HP {healRewardRatio:0%}", 0, false);
            count = 1;
        }

        selectWindow.SetWeaponSlots(skillController.ActiveSkills.Count);
        RequestPause(selectWindow);
        selectWindow.Show(count);
    }

    private void HandleSkillSelected(int index)
    {
        if (candidates.Count == 0)
            playerHealth.Heal(Mathf.RoundToInt(playerHealth.MaxHealth * healRewardRatio));
        else if (FindActiveSkill(candidates[index]) != null)
            skillController.LevelUpSkill(candidates[index]);
        else
            skillController.EquipSkill(candidates[index]);

        pendingLevelUps--;
        if (pendingLevelUps > 0)
        {
            ShowSelection();
            return;
        }

        selectWindow.Close();
        ReleasePause(selectWindow);
    }

    private SkillBase FindActiveSkill(int skillId)
    {
        foreach (var skill in skillController.ActiveSkills)
        {
            if (skill.SkillId == skillId) return skill;
        }
        return null;
    }

    private void EndBattle(bool victory)
    {
        if (ended) return;
        ended = true;

        if (selectWindow.IsOpen)
        {
            selectWindow.Close();
            ReleasePause(selectWindow);
        }
        pendingLevelUps = 0;

        var accountExp = kills * accountExpPerKill + seconds * accountExpPerSecond
                         + (victory ? accountExpClearBonus : 0);

        BattleResult.Last = new BattleResult
        {
            StageID = spawner.CurrentStage.stageID,
            Victory = victory,
            Seconds = seconds,
            Kills = kills,
            Gold = gold,
            RewardBoxes = rewardBoxes,
            AccountExp = accountExp
        };

        RequestPause(resultWindow);
        resultWindow.SetTime(seconds);
        resultWindow.SetKillCount(kills);
        resultWindow.SetGold(gold);
        resultWindow.SetBoxCount(rewardBoxes);
        resultWindow.SetExp(accountExp);
        resultWindow.Show(victory);
    }

    [ContextMenu("TestAddExp")]
    public void TestAddExp()
    {
        AddExp(testExp);
    }
}
