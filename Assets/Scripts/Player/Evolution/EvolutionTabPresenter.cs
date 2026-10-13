using System.Collections.Generic;
using UnityEngine;

// 진화 탭에 서버 진화 규칙(4종 순환)을 표시하고, 해금을 서버 요청으로 처리한다
// EvolutionTab과 같은 오브젝트에 붙인다
[RequireComponent(typeof(EvolutionTab))]
public class EvolutionTabPresenter : MonoBehaviour
{
    [Header("Range")]
    [SerializeField] private bool showAllPastNodes = true;          // 처음부터 지나온 노드 전부 표시
    [SerializeField][Min(0)] private int pastNodeCount = 4;        // showAllPastNodes가 꺼져 있을 때만 사용
    [SerializeField][Min(1)] private int futureNodeCount = 12;     // 앞으로 갈 노드

    [Header("Icon Colors")]
    [SerializeField] private Color attackColor = new(0.90f, 0.30f, 0.30f);
    [SerializeField] private Color hpColor = new(0.30f, 0.80f, 0.40f);
    [SerializeField] private Color defenseColor = new(0.30f, 0.55f, 0.95f);
    [SerializeField] private Color potionRecoveryColor = new(0.70f, 0.40f, 0.90f);

    private EvolutionTab tab;
    private int unlockedInWindow;   // 표시 범위 안에서 해금된 노드 수 (= 다음 해금 노드 번호)
    private bool isRequesting;
    private bool isPlayerDataSubscribed;

    private void Awake()
    {
        tab = GetComponent<EvolutionTab>();
    }

    private void OnEnable()
    {
        tab.UnlockRequested += HandleUnlockRequested;

        if (GameManager.PlayerData != null)
        {
            GameManager.PlayerData.OnPlayerDataChanged += OnPlayerDataChanged;
            isPlayerDataSubscribed = true;
        }

        Rebuild();
        tab.SetUnlockInteractable(!isRequesting);
    }

    private void OnDisable()
    {
        tab.UnlockRequested -= HandleUnlockRequested;

        if (isPlayerDataSubscribed && GameManager.PlayerData != null)
            GameManager.PlayerData.OnPlayerDataChanged -= OnPlayerDataChanged;
        isPlayerDataSubscribed = false;
    }

    private void OnPlayerDataChanged(PlayerSaveData _) => Rebuild();

    // 현재 진화 단계 기준으로 지나온 노드 + 앞으로 갈 노드를 만든다
    private void Rebuild()
    {
        var dataManager = GameManager.PlayerData;
        var evolution = EvolutionRules.Data;

        if (dataManager == null || !dataManager.isPlayerDataLoaded || evolution == null)
        {
            Debug.LogWarning("[EvolutionTabPresenter] 계정 데이터 또는 진화 데이터가 없어 진화 탭을 비웁니다.", this);
            tab.SetLinear(System.Array.Empty<EvolutionNodeInfo>(), 0, 0);
            return;
        }

        var saveData = dataManager.currentData;
        int step = EvolutionRules.GetStep(saveData.playerStat);

        int windowStart = showAllPastNodes ? 0 : Mathf.Max(0, step - pastNodeCount);
        int windowEnd = step + futureNodeCount;
        if (evolution.evolutionMaxStep > 0)
            windowEnd = Mathf.Min(windowEnd, evolution.evolutionMaxStep);

        var nodes = new List<EvolutionNodeInfo>();
        for (int s = windowStart; s < windowEnd; s++)
            nodes.Add(BuildNode(evolution, s));

        unlockedInWindow = step - windowStart;
        tab.SetLinear(nodes.ToArray(), unlockedInWindow, saveData.currency.currencyGold);
    }

    // s번째 진화(0부터) 노드
    private EvolutionNodeInfo BuildNode(EvolutionData evolution, int s)
    {
        string statType = EvolutionRules.GetNextStatType(evolution, s);
        int increase = EvolutionRules.GetIncreasePerLevel(evolution, statType);
        string statName = GetStatName(statType);

        return new EvolutionNodeInfo
        {
            level = 0,
            name = statName,
            value = "+" + increase,
            description = statName + "이 " + increase + " 증가합니다.",
            iconColor = GetStatColor(statType),
            cost = EvolutionRules.GetNextCost(evolution, s)
        };
    }

    private void HandleUnlockRequested(int index)
    {
        if (isRequesting) return;
        if (index != unlockedInWindow) return;   // 다음 해금 노드만

        isRequesting = true;
        tab.SetUnlockInteractable(false);

        // 탭이 닫혀도 요청이 끝까지 처리되도록, 씬 전환에도 유지되는 PlayerDataManager에서 실행
        GameManager.PlayerData.StartCoroutine(PlayerEvolution.Upgrade(OnUpgraded));
    }

    private void OnUpgraded(ApiResult<EvolutionUpgradeResponse> result)
    {
        if (this == null) return;   // 씬이 바뀌어 진화 탭이 사라진 경우

        isRequesting = false;

        // 진화 레벨·골드 반영은 PlayerEvolution이 이미 처리함 (탭이 닫혀 있어도 반영됨)
        if (result.IsSuccess && result.Data.currency != null)
            UIManager.Instance.Get<TopBar>().SetGold(result.Data.currency.currencyGold);

        if (!result.IsSuccess)
            UIManager.Instance.Get<LogPopup>().Show("알림", GetFailMessage(result));

        if (isActiveAndEnabled)
            tab.SetUnlockInteractable(true);
    }

    private static string GetFailMessage(ApiResult<EvolutionUpgradeResponse> result)
    {
        if (result.FailType == ApiFailType.ServerError && result.Error != null)
        {
            if (result.Error.code == PlayerApi.ErrorNotEnoughGold) return "골드가 부족합니다.";
            if (result.Error.code == PlayerApi.ErrorEvolutionMaxStepReached) return "더 이상 진화할 수 없습니다.";
        }
        return result.ToUserMessage("진화에 실패했습니다.");
    }

    private static string GetStatName(string statType) => statType switch
    {
        PlayerStatType.Attack => "공격력",
        PlayerStatType.Hp => "체력",
        PlayerStatType.Defense => "방어력",
        PlayerStatType.PotionRecovery => "포션 회복량",
        _ => statType
    };

    private Color GetStatColor(string statType) => statType switch
    {
        PlayerStatType.Attack => attackColor,
        PlayerStatType.Hp => hpColor,
        PlayerStatType.Defense => defenseColor,
        PlayerStatType.PotionRecovery => potionRecoveryColor,
        _ => Color.white
    };
}