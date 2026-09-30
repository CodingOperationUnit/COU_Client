using System;
using UnityEngine;

public class DataManager : MonoSingleton<DataManager>
{
    public PlayerSaveData currentData { get; private set; }

    public bool isPlayerDataLoaded => currentData != null;
    
    // 로그인 시 확정된 계정 ID
    public string currentPlayerID { get; private set; }

    public event Action<PlayerSaveData> OnPlayerDataChanged;

    // 로컬 JSON에서 읽은 플레이어 데이터를 보관
    public void SetPlayerDataFromLocal(PlayerSaveData data)
    {
        if (data == null)
            throw new ArgumentException(nameof(data));

        if (string.IsNullOrWhiteSpace(data.playerID))
            throw new ArgumentException("플레이어 ID가 없습니다.");

        if (isPlayerDataLoaded)
            throw new InvalidOperationException("기존 계정 데이터를 해제한 뒤 적용하세요.");

        currentPlayerID = data.playerID;
        currentData = data;
    }
    
    // 데이터 로드 또는 변경 후 Action을 Invoke하는 메서드
    public void NotifyPlayerDataChanged()
    {
        if (isPlayerDataLoaded) OnPlayerDataChanged?.Invoke(currentData);
    }
    
    // 로그아웃 시 플레이어 데이터 해제
    public void ClearPlayerData()
    {
        currentData = null;
        currentPlayerID = null;
    }
}
