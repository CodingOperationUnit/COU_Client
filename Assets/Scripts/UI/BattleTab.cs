using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleTab : UIView
{
    [SerializeField] private TMP_Text stageNameText;
    [SerializeField] private TMP_Text bestTimeText;
    [SerializeField] private Image illustration;
    [SerializeField] private Button illustrationButton;
    [SerializeField] private TMP_Text staminaCostText;
    [SerializeField] private Button gameStartButton;

    private int currentStageId;
    private int staminaCost;
    private bool isRequesting;

    private void Awake()
    {
        illustrationButton.onClick.AddListener(() => UIManager.Instance.Open<StageSelectScreen>());
        gameStartButton.onClick.AddListener(StartBattle);
    }

    public void SetStage(StageData stage, int bestTime)
    {
        currentStageId = stage.stageId;
        stageNameText.text = stage.stageName;
        bestTimeText.text = bestTime > 0
            ? $"최장 생존시간: {bestTime / 60:00}:{bestTime % 60:00}"
            : "최장 생존시간:";
        illustration.color = stage.IllustrationColor;
    }

    public void SetStaminaCost(int cost)
    {
        staminaCost = cost;
        staminaCostText.text = $"x {cost}";
    }

    // 서버가 해금과 스태미나를 검증해 차감하고 battleId를 발급한다
    public void StartBattle()
    {
        if (isRequesting) return;
        isRequesting = true;

        // 응답의 재화 반영(NotifyPlayerDataChanged)이 선택 스테이지를 다시 설정하므로 요청한 스테이지를 잡아 둔다
        int stageId = currentStageId;

        StartCoroutine(BattleApi.Enter(stageId, result =>
        {
            isRequesting = false;

            if (!result.IsSuccess)
            {
                UIManager.Instance.Get<LogPopup>().Show("알림", GetEnterErrorMessage(result));
                return;
            }

            // 차감 후 재화로 덮어쓴다
            GameManager.PlayerData.currentData.currency = result.Data.currency;
            GameManager.PlayerData.NotifyPlayerDataChanged();

            GameManager.Scene.ChangeScene(GameConstants.SceneNames.BATTLE_SCENE, stageId, result.Data.battleId);
        }));
    }

    private static string GetEnterErrorMessage(ApiResult<BattleEnterResponse> result)
    {
        if (result.FailType == ApiFailType.NetworkError)
            return "서버에 연결할 수 없습니다. 잠시 후 다시 시도해 주세요.";

        return result.Error?.code switch
        {
            BattleApi.ErrorStageLocked => "아직 열리지 않은 스테이지입니다.",
            BattleApi.ErrorNotEnoughStamina => "스태미나가 부족합니다.",
            _ => string.IsNullOrEmpty(result.Error?.message) ? "전투에 입장하지 못했습니다." : result.Error.message
        };
    }
}
