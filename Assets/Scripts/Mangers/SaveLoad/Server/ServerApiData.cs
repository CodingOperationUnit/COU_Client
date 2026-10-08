using System;
using System.Collections.Generic;

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

[Serializable]
public class BattleEnterRequest
{
    public int stageId;
}

[Serializable]
public class BattleEnterResponse
{
    public long battleId;
    public CurrencyData currency;          // 차감 후 재화
}

// StageId는 서버가 battleId로 알고, 계정 경험치는 서버가 계산하므로 보내지 않는다
[Serializable]
public class BattleResultRequest
{
    public bool victory;
    public int seconds;
    public int kills;
    public int gold;
    public int rewardBoxes;
}

[Serializable]
public class BattleResultResponse
{
    public int grantedGold;                // 상한 적용 후 지급한 골드
    public int grantedExp;                 // 지급한 계정 경험치
    public PlayerProfileData profile;
    public CurrencyData currency;
    public StageProgressData stageProgress;
    public StageRecordSaveData stageRecord;
    public List<InventoryData> rewards;    // 보상상자로 지급한 장비
}