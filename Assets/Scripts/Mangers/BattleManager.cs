using System.Collections;
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
    [SerializeField] private int baseRequiredExp = 6;
    [SerializeField] private int requiredExpIncrement = 6;
    [SerializeField] private float requiredExpAcceleration = 0.3f;
    [SerializeField] private float healRewardRatio = 0.3f;
    [SerializeField] private float bossVictoryDelay = 2f;

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

    private long battleId;
    private bool hasBattleId;   // 메인 씬을 거치지 않고 실행하면 battleId가 없어 결과를 보내지 않는다
    private BattleResultRequest resultRequest;

    private int RequiredExp => baseRequiredExp + requiredExpIncrement * (level - 1)
                               + Mathf.RoundToInt(requiredExpAcceleration * (level - 1) * (level - 1));

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

        hasBattleId = GameManager.Scene.TryConsumePendingBattleId(out battleId);

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
        if (type >= DropItemType.ExpGem1 && type <= DropItemType.ExpGem4)
            AddExp(GameManager.JsonData.GetDropItemDataFromJson(type).value);
        else if (type >= DropItemType.Gold1 && type <= DropItemType.Gold4)
            AddGold(GameManager.JsonData.GetDropItemDataFromJson(type).value);
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
        foreach (var skillId in GameManager.JsonData.SkillDataDic.Keys)
        {
            // 테이블에는 있지만 아직 구현되지 않은 스킬은 후보에서 뺀다
            if (!SkillFactory.IsRegistered(skillId)) continue;

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
            selectWindow.SetOption(i, data.skillName, data.skillDescription,skill == null ? 1 : skill.Level + 1, skill == null);
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

        // 킬 수와 생존 시간은 바로 표시하고, 지급값은 서버 응답을 받은 뒤 표시한다
        RequestPause(resultWindow);
        resultWindow.SetTime(seconds);
        resultWindow.SetKillCount(kills);
        resultWindow.Show(victory);

        if (!hasBattleId)
        {
            Debug.LogWarning("[BattleManager] battleId가 없어 결과를 서버로 보내지 않습니다. (메인 씬을 거치지 않은 경우 정상)");
            resultWindow.SetGold(gold);
            resultWindow.SetBoxCount(rewardBoxes);
            resultWindow.SetExp(0);
            return;
        }

        resultRequest = new BattleResultRequest
        {
            victory = victory,
            seconds = seconds,
            kills = kills,
            gold = gold,
            rewardBoxes = rewardBoxes
        };

        resultWindow.SetWaiting(true);
        StartCoroutine(SendResult());
    }

    private IEnumerator SendResult()
    {
        yield return BattleApi.SendResult(battleId, resultRequest, result =>
        {
            if (!result.IsSuccess)
            {
                HandleResultError(result);
                return;
            }

            // 지급 결과를 덮어쓰고 보상 장비를 inventoryList에 넣는다
            var response = result.Data;
            var data = GameManager.PlayerData.currentData;
            ApplyBattleChanges(data, response.profile, response.currency, response.stageProgress);
            SetStageRecord(data, response.stageRecord);
            data.inventoryList.AddRange(response.rewards);
            GameManager.PlayerData.NotifyPlayerDataChanged();

            resultWindow.SetGold(response.grantedGold);
            resultWindow.SetExp(response.grantedExp);
            resultWindow.SetBoxCount(response.rewards.Count);
            resultWindow.SetWaiting(false);

            // 보상상자 팝업은 메인 씬으로 돌아가 PlayerInventory가 띄운다
            BattleResult.Rewards = response.rewards;
        });
    }

    private void HandleResultError(ApiResult<BattleResultResponse> result)
    {
        var popup = UIManager.Instance.Get<MessagePopup>();

        // 통신 실패와 5xx는 같은 battleId로 다시 보낸다
        if (result.FailType == ApiFailType.NetworkError || result.StatusCode >= 500)
        {
            popup.ShowAlert("결과를 보내지 못했습니다. 확인을 누르면 다시 보냅니다.", () => StartCoroutine(SendResult()));
            return;
        }

        switch (result.Error?.code)
        {
            // 첫 요청이 반영됐는데 응답을 받지 못한 경우: 지급값은 표시할 수 없고 세이브를 다시 받는다
            case BattleApi.ErrorBattleAlreadyCompleted:
                StartCoroutine(ReloadSave());
                break;

            // 결과를 보내기 전에 새 전투에 입장한 경우: 보상 없이 닫는다
            case BattleApi.ErrorBattleExpired:
                resultWindow.SetGold(0);
                resultWindow.SetExp(0);
                resultWindow.SetBoxCount(0);
                resultWindow.SetWaiting(false);
                popup.ShowAlert("만료된 전투라 보상을 받을 수 없습니다.");
                break;

            default:
                resultWindow.SetWaiting(false);
                popup.ShowAlert(string.IsNullOrEmpty(result.Error?.message) ? "결과를 반영하지 못했습니다." : result.Error.message);
                break;
        }
    }

    // 세이브를 다시 받아 전투가 바꾸는 값을 덮어쓴다
    // TODO(인벤토리 연동): 보상 장비까지 받으려면 GET /api/inventory도 다시 받아야 한다. 지금은 inventoryList를 유지한다
    private IEnumerator ReloadSave()
    {
        yield return GameManager.ServerLoad.RequestSave((loaded, message) =>
        {
            resultWindow.SetWaiting(false);

            if (loaded == null)
            {
                UIManager.Instance.Get<MessagePopup>().ShowAlert(message);
                return;
            }

            var data = GameManager.PlayerData.currentData;
            ApplyBattleChanges(data, loaded.profile, loaded.currency, loaded.stageProgress);
            data.stageRecords = loaded.stageRecords;
            GameManager.PlayerData.NotifyPlayerDataChanged();
        });
    }

    // 전투가 바꾸는 값: 계정 레벨·경험치, 재화, 스테이지 진행
    // profile은 장착 칸을 유지하려고 레벨과 경험치만 옮긴다
    private static void ApplyBattleChanges(PlayerSaveData data, PlayerProfileData profile, CurrencyData currency, StageProgressData stageProgress)
    {
        data.profile.accountLevel = profile.accountLevel;
        data.profile.accountExp = profile.accountExp;
        data.currency = currency;
        data.stageProgress = stageProgress;
    }

    private static void SetStageRecord(PlayerSaveData data, StageRecordSaveData record)
    {
        int index = data.stageRecords.FindIndex(saved => saved.stageId == record.stageId);
        if (index >= 0)
            data.stageRecords[index] = record;
        else
            data.stageRecords.Add(record);
    }
}
