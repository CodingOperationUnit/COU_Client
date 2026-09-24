using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseWindow : UIView
{
    [SerializeField] private TMP_Text killText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button soundButton;
    [SerializeField] private GameObject muteMark;

    private bool muted;

    private void Awake()
    {
        homeButton.onClick.AddListener(() => UIManager.Instance.Open<HomePopup>());
        continueButton.onClick.AddListener(Close);
        soundButton.onClick.AddListener(ToggleMute);
    }

    public override void Open()
    {
        base.Open();
        Time.timeScale = 0f;
    }

    public override void Close()
    {
        base.Close();
        Time.timeScale = 1f;
    }

    public void SetKillCount(int kills)
        => killText.text = kills.ToString();

    public void SetGold(int gold)
        => goldText.text = gold.ToString();

    private void ToggleMute()
    {
        muted = !muted;
        AudioListener.volume = muted ? 0f : 1f;
        muteMark.SetActive(muted);
    }
}
