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
        if (BattleManager.Instance != null)
            BattleManager.Instance.RequestPause(this);
    }

    public override void Close()
    {
        base.Close();
        if (BattleManager.Instance != null)
            BattleManager.Instance.ReleasePause(this);
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
