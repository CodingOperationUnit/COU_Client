using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

// GET /api/inventory 응답
[Serializable]
public class InventoryResponse
{
    public int currencyGold;                 // /me/save의 currency와 같은 값이라 사용하지 않음
    public int currencyGem;
    public List<EquipmentResponse> items;
}

[Serializable]
public class EquipmentResponse
{
    public long inventoryId;
    public long itemId;
    public int inventoryItemLevel;

    [JsonConverter(typeof(StringEnumConverter))]
    public ItemGrade? inventoryItemGrade;    // "General" → ItemGrade.General

    public bool isEquipped;
}