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
        // 서버 데이터는 서버에 이미 저장되어 있으므로 로그아웃 때 따로 저장하지 않는다.
        GameManager.ServerLogin.Logout(); // 토큰, 계정 정보, PlayerDataManager 데이터 삭제
        
        // UIManager는 씬을 넘어 유지되므로, 열려 있는 팝업(설정, 프로필)을 닫고 이동한다.
        UIManager.Instance.CloseAll();

        GameManager.Scene.ChangeScene(GameConstants.SceneNames.LOGIN_SCENE);
    }
}
