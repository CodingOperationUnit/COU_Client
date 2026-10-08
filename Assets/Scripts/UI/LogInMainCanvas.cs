using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class LogInMainCanvas : MonoBehaviour
{
    [FormerlySerializedAs("playerIDInputField")]
    [SerializeField] private TMP_InputField playerIdInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Popup_Alarm popUp_Alarm;

    private const string AppUpdateRequiredMessage = "앱 업데이트가 필요합니다.";

    // 서버 정적 데이터 확인이 끝나기 전에는 로그인/가입을 막는다
    private bool isStaticDataChecked;
    private bool isAppUpdateRequired;

    private void Awake()
    {
        popUp_Alarm.Close();
    }

    private void Start()
    {
        StartCoroutine(GameManager.JsonData.FetchStaticData(appUpdateRequired =>
        {
            isStaticDataChecked = true;
            isAppUpdateRequired = appUpdateRequired;

            if (appUpdateRequired)
                popUp_Alarm.Show(AppUpdateRequiredMessage);
        }));
    }

    private bool CanProceed()
    {
        if (!isStaticDataChecked)
        {
            popUp_Alarm.Show("게임 데이터를 확인하고 있습니다. 잠시 후 다시 시도해 주세요.");
            return false;
        }

        if (isAppUpdateRequired)
        {
            popUp_Alarm.Show(AppUpdateRequiredMessage);
            return false;
        }

        return true;
    }

    public void OnClickLogin()
    {
        if (!CanProceed()) return;

        TryLogin(playerIdInputField.text, passwordInputField.text);
    }

    public void OnClickSignUp()
    {
        if (!CanProceed()) return;

        bool success = GameManager.LocalLogin.SignUp(
            playerIdInputField.text, passwordInputField.text, out string message
        );

        if (success)
        {
            // 회원가입 완료 안내 -> 확인 버튼을 누르면 자동으로 로그인
            popUp_Alarm.Show(message, () => TryLogin(playerIdInputField.text, passwordInputField.text));
            Debug.Log("회원가입 성공");
        }
        else
        {
            popUp_Alarm.Show(message);
        }
    }

    private void TryLogin(string playerId, string password)
    {
        string normalizedPlayerId = (playerId ?? string.Empty).Trim().ToLowerInvariant();

        // 아이디/비밀번호 검증
        bool loginSuccess = GameManager.LocalLogin.Login(normalizedPlayerId, password, out string message);

        if (!loginSuccess)
        {
            popUp_Alarm.Show(message);
            return;
        }

        // 검증된 계정의 플레이어 JSON 로드 → DataManager에 보관
        bool loadSuccess = GameManager.LocalSaveLoad.LoadPlayerAfterLogin(normalizedPlayerId);

        if (!loadSuccess)
        {
            popUp_Alarm.Show("플레이어 데이터를 불러오지 못했습니다.");
            return;
        }

        // 인증과 데이터 로드가 모두 성공하면 입장
        Debug.Log("로그인 및 플레이어 데이터 로드 완료");
        GameManager.Scene.ChangeScene(GameConstants.SceneNames.MAIN_SCENE);
    }
}
