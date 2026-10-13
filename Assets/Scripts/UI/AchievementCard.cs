using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementCard : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text progressText;
    
    public AchievementData Data { get; private set; }
    
    // tier: 보여줄 단계(0단계 부터), progressValue: 현재 누적값
    public void set(AchievementData data, int tier = 0, long progressValue = 0)
    {
        Data = data;
        tier = Mathf.Clamp(tier, 0, data.TierCount - 1);

        long target = data.GetTargetValue(tier);
        long current = Math.Min(progressValue, target);

        nameText.text = data.achievementName;
        descriptionText.text = data.GetDescription(tier);
        progressFill.fillAmount = (float)current / target;
        progressText.text = $"{current} / {target}";
    }
}
