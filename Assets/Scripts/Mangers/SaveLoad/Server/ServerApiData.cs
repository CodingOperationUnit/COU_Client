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

// POST /api/inventory/{id}/equip
[Serializable]
public class EquipResponse
{
    public EquipmentResponse equipped;      // 장착된 장비
    public EquipmentResponse unequipped;    // 같은 슬롯에서 자동 해제된 장비 (없으면 null)
}

// POST /api/inventory/{id}/levelup
[Serializable]
public class LevelUpResponse
{
    public long inventoryId;
    public int inventoryItemLevel;   // 레벨업 후 레벨
    public int spentGold;
    public int currencyGold;         // 차감 후 보유 골드
}

// POST /api/inventory/{id}/levelup/batch
[Serializable]
public class LevelUpBatchResponse
{
    public long inventoryId;
    public int inventoryItemLevel;   // 최종 레벨
    public int levelsGained;
    public int spentGold;
    public int currencyGold;
}

// POST /api/inventory/{id}/synthesize, /synthesize/batch (batch는 tiersGained가 추가됨)
[Serializable]
public class SynthesizeResponse
{
    public long inventoryId;
    public long itemId;
    public int inventoryItemLevel;

    [JsonConverter(typeof(StringEnumConverter))]
    public ItemGrade? inventoryItemGrade;   // 합성 후 등급

    public bool isEquipped;
    public int tiersGained;                 // 일괄 합성에서만 값이 옴
    public List<long> consumedInventoryIds; // 재료로 소모된 장비
}