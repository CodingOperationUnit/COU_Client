using TMPro;
using UnityEngine;

public class LogInMainCanvas : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerIDInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Popup_Alarm popUp_Alarm;

    private void Awake()
    {
        popUp_Alarm.Close();
    }

    public void OnClickLogin()
    {
        TryLogin(playerIDInputField.text, passwordInputField.text);
    }

    public void OnClickSignUp()
    {
        bool success = GameManager.LocalLogin.SignUp(
            playerIDInputField.text, passwordInputField.text, out string message
        );

        if (success)
        {
            // 회원가입 완료 안내 -> 확인 버튼을 누르면 자동으로 로그인
            popUp_Alarm.Show(message, () => TryLogin(playerIDInputField.text, passwordInputField.text));
            Debug.Log("회원가입 성공");
        }
        else
        {
            popUp_Alarm.Show(message);
        }
    }

    private void TryLogin(string playerID, string password)
    {
        bool success = GameManager.LocalLogin.Login(playerID, password, out string message);

        if (!success)
        {
            popUp_Alarm.Show(message);
            return;
        }
        
        Debug.Log("로그인 성공");
        // 플레이어 데이터 로드 완료
        // 여기에 게임 씬으로 이동하는 코드 연결
    }
}
