using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 로컬에 저장할 계정 목록 리스트
/// </summary>
[Serializable]
public class AccountSaveData
{
    public List<LocalAccountData> accounts = new List<LocalAccountData>();
}

/// <summary>
/// 계정 하나의 로그인 정보
/// </summary>
[Serializable]
public class LocalAccountData
{
    public string playerID;
    public string password;
}

/// <summary>
/// Players/{playerId}.json으로 저장할 게임 데이터
/// </summary>
[Serializable]
public class PlayerSaveData
{
    public string playerID;
    
    public int gold;
    public int gem;

    public int currentStamina;
    public int maxStamina;
    
    public int accountLevel = 1;
    public int accountExp;

    public List<EquipmentSaveData> equipmentList = new List<EquipmentSaveData>();
    public List<StageRecordSaveData> stageRecordList = new List<StageRecordSaveData>();

    public static PlayerSaveData CreateDefault(string playerID)
    {
        if (string.IsNullOrWhiteSpace(playerID))
            throw new ArgumentException("플레이어 ID가 필요합니다.");

        return new PlayerSaveData
        {
            playerID = playerID,
            gold = 0,
            gem = 0,
            maxStamina = 60,
            currentStamina = 60,
            accountLevel = 1,
            accountExp = 0,
            equipmentList = new List<EquipmentSaveData>(),
            stageRecordList = CreateDefaultStageRecords()
        };
    }

    private static List<StageRecordSaveData> CreateDefaultStageRecords()
    {
        var stageDataDic = GameManager.JsonData.StageDataDic;
        var records = new List<StageRecordSaveData>();

        if (stageDataDic == null)
        {
            Debug.LogWarning("[PlayerSaveData] 스테이지 데이터가 로드되지 않아 기록을 초기화하지 못했습니다.");
            return records;
        }

        foreach (var stageID in stageDataDic.Keys)
        {
            records.Add(new StageRecordSaveData
            {
                stageID = stageID,
                isCleared = false,
                bestSurvivalSeconds = 0f
            });
        }

        return records;
    }
}

[Serializable]
public class EquipmentSaveData
{
    public string instanceId;
    public long itemId;
    public int level = 1;
    public bool isEquipped;
}

[Serializable]
public class StageRecordSaveData
{
    public int stageID;
    public bool isCleared;
    public float bestSurvivalSeconds;
}