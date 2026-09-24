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
    [SerializeField] private GameObject expGroup;
    [SerializeField] private GameObject bossGroup;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private Image bossHpFill;

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

    public void ShowBoss(string bossName)
    {
        bossNameText.text = bossName;
        expGroup.SetActive(false);
        bossGroup.SetActive(true);
    }

    public void SetBossHp(float ratio)
        => bossHpFill.fillAmount = ratio;

    public void HideBoss()
    {
        bossGroup.SetActive(false);
        expGroup.SetActive(true);
    }
}
