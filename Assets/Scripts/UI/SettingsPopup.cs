using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : UIPopup
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button soundButton;
    [SerializeField] private GameObject muteMark;
    [SerializeField] private Button darkThemeButton;
    [SerializeField] private GameObject darkThemeMark;
    [SerializeField] private Button lightThemeButton;
    [SerializeField] private GameObject lightThemeMark;
    [SerializeField] private Button logoutButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        soundButton.onClick.AddListener(ToggleMute);
        darkThemeButton.onClick.AddListener(() => SetTheme(UITheme.Dark));
        lightThemeButton.onClick.AddListener(() => SetTheme(UITheme.Light));
        logoutButton.onClick.AddListener(Logout);
    }

    public override void Open()
    {
        base.Open();
        muteMark.SetActive(AudioListener.volume == 0f);
        RefreshThemeMarks();
    }

    private void ToggleMute()
    {
        AudioListener.volume = AudioListener.volume > 0f ? 0f : 1f;
        muteMark.SetActive(AudioListener.volume == 0f);
    }

    private void SetTheme(UITheme theme)
    {
        UIManager.Instance.SetTheme(theme);
        RefreshThemeMarks();
    }

    private void RefreshThemeMarks()
    {
        var theme = UIManager.Instance.Theme;
        darkThemeMark.SetActive(theme == UITheme.Dark);
        lightThemeMark.SetActive(theme == UITheme.Light);
    }

    private void Logout()
    {
        if (!GameManager.LocalLogin.Logout(out string message))
        {
            UIManager.Instance.Get<MessagePopup>().ShowAlert(message);
            return;
        }

        GameManager.Scene.ChangeScene(GameConstants.SceneNames.LOGIN_SCENE);
    }
}
