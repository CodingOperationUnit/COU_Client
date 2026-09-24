using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleResultWindow : UIView
{
    [SerializeField] private Image banner;
    [SerializeField] private Image ribbon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Color victoryColor;
    [SerializeField] private Color defeatColor;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text chapterText;
    [SerializeField] private TMP_Text bestTimeText;
    [SerializeField] private TMP_Text killText;
    [SerializeField] private GameObject box;
    [SerializeField] private TMP_Text boxCountText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text expText;

    public void Show(bool victory)
    {
        titleText.text = victory ? "승리" : "실패";
        banner.color = ribbon.color = victory ? victoryColor : defeatColor;
        Open();
    }

    public void SetTime(int seconds)
        => timeText.text = FormatTime(seconds);

    public void SetChapter(int chapter)
        => chapterText.text = $"{chapter} 챕터";

    public void SetBestTime(int seconds)
        => bestTimeText.text = $"최고:<color=#A8F000>{FormatTime(seconds)}</color>";

    public void SetKillCount(int kills)
        => killText.text = kills.ToString();

    public void SetBoxCount(int count)
    {
        box.SetActive(count > 0);
        boxCountText.text = $"x{count}";
    }

    public void SetGold(int gold)
        => goldText.text = $"x{FormatAmount(gold)}";

    public void SetExp(int exp)
        => expText.text = $"x{FormatAmount(exp)}";

    private static string FormatTime(int seconds)
        => $"{seconds / 60:00}:{seconds % 60:00}";

    private static string FormatAmount(int amount)
        => amount >= 10000 ? $"{amount / 1000f:0.#}K" : amount.ToString();
}
