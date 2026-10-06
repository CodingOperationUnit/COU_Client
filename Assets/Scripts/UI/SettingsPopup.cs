using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : UIPopup
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button soundButton;
    [SerializeField] private GameObject muteMark;
    [SerializeField] private Button logoutButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        soundButton.onClick.AddListener(ToggleMute);
        logoutButton.onClick.AddListener(Logout);
    }

    public override void Open()
    {
        base.Open();
        muteMark.SetActive(AudioListener.volume == 0f);
    }

    private void ToggleMute()
    {
        AudioListener.volume = AudioListener.volume > 0f ? 0f : 1f;
        muteMark.SetActive(AudioListener.volume == 0f);
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
