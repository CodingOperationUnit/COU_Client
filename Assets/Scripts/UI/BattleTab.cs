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

    private int currentStageID;
    private int staminaCost;

    private void Awake()
    {
        illustrationButton.onClick.AddListener(() => UIManager.Instance.Open<StageSelectScreen>());
        gameStartButton.onClick.AddListener(StartBattle);
    }

    public void SetStage(StageData stage, int bestTime)
    {
        currentStageID = stage.stageID;
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

    public void StartBattle()
    {
        if (!PlayerInventory.Instance.TrySpendStamina(staminaCost))
        {
            UIManager.Instance.Get<LogPopup>().Show("알림", "스태미나가 부족합니다.");
            return;
        }

        GameManager.Scene.ChangeScene(GameConstants.SceneNames.BATTLE_SCENE, currentStageID);
    }
}
