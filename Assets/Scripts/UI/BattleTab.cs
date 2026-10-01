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

    private void Awake()
    {
        illustrationButton.onClick.AddListener(() => UIManager.Instance.Open<StageSelectScreen>());
    }

    public void SetStage(StageData stage, int bestTime)
    {
        stageNameText.text = stage.stageName;
        bestTimeText.text = bestTime > 0
            ? $"최장 생존시간: {bestTime / 60:00}:{bestTime % 60:00}"
            : "최장 생존시간:";
        illustration.color = stage.IllustrationColor;
    }

    public void SetStaminaCost(int cost)
        => staminaCostText.text = $"x {cost}";
}
