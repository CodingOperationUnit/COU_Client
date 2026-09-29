using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogInMainCanvas : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerIDInputField;
    [SerializeField] private TMP_InputField passwordInputField;

    public void OnClickLogin()
    {
        bool success = GameManager.LocalLogin.Login(
            playerIDInputField.text, passwordInputField.text, out string message
        );
        
        Debug.Log(message);

        if (success)
        {
            // 플레이어 데이터 로드 후 게임 씬으로 이동
            Debug.Log("로그인 성공");
        }
    }

    public void OnClickSignUp()
    {
        bool success = GameManager.LocalLogin.SignUp(
            playerIDInputField.text, passwordInputField.text, out string message
        );
        
        Debug.Log(message);

        if (success)
        {
            // 회원가입 완료 안내
            // 확인 버튼을 누르면 자동으로 로그인
            Debug.Log("회원가입 성공");
        }
    }
}
