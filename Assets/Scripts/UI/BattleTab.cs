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

    public void SetStage(StageInfo stage)
    {
        stageNameText.text = stage.name;
        bestTimeText.text = stage.bestTime > 0
            ? $"최장 생존시간: {stage.bestTime / 60:00}:{stage.bestTime % 60:00}"
            : "최장 생존시간:";
        illustration.color = stage.illustrationColor;
    }

    public void SetStaminaCost(int cost)
        => staminaCostText.text = $"x {cost}";
}
