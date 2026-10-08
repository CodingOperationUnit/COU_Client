using System;

// 서버 API 요청/응답 JSON 형식 (필드 이름 = JSON 키)

[Serializable]
public class SignupRequest
{
    public string accountLoginId;
    public string password;
}

[Serializable]
public class LoginRequest
{
    public string accountLoginId;
    public string password;
}

[Serializable]
public class LoginResponse
{
    public string accessToken;
    public string tokenType;               // "Bearer"
    public ServerAccountData account;      // SaveData.cs의 기존 클래스 재사용
}

// 실패 응답 형식: { status, code, message }
[Serializable]
public class ErrorResponse
{
    public int status;
    public string code;
    public string message;
}