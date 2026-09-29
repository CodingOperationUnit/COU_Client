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
    
    // 보유 장비, 골드, 스테이지 정보 등을 작성 예정
    public int gold;
    public int exp;
}