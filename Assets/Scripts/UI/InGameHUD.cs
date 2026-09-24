using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameHUD : UIView
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text killText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image expFill;

    public void SetTime(int seconds)
        => timerText.text = $"{seconds / 60:00}:{seconds % 60:00}";

    public void SetGold(int gold)
        => goldText.text = gold.ToString();

    public void SetKillCount(int kills)
        => killText.text = kills.ToString();

    public void SetLevel(int level)
        => levelText.text = $"LV.{level}";

    public void SetExp(float ratio)
        => expFill.fillAmount = ratio;
}
