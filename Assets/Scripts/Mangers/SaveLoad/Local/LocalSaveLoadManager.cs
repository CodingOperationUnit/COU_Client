using System;
using UnityEngine;

/// <summary>
/// 로그인한 계정의 플레이어 데이터를 로드하고,
/// 게임 진행 중 변경된 데이터를 로컬에 저장합니다.
/// 이후 서버와 연동한다면 대체될 기능입니다.
/// </summary>
public class LocalSaveLoadManager : MonoSingleton<LocalSaveLoadManager>
{
    public PlayerSaveData currentData { get; private set; }
    
    public bool isPlayerDataLoaded => currentData != null;
    private string loadedPlayerID;
    
    public event Action<PlayerSaveData> OnPlayerDataChanged;

    /// <summary>
    /// 회원가입 과정에서 초기 게임 데이터를 생성합니다.
    /// 계정 목록 등록과 비밀번호 검증은 로그인 담당 코드에서 처리합니다.
    /// </summary>
    public bool CreateInitialPlayerData(string playerID)
    {
        try
        {
            // 기존 데이터가 있으면 덮어쓰지 않습니다.
            if (SaveLoadHelper.LoadPlayer(playerID) != null)
            {
                Debug.LogWarning("이미 플레이어 저장 데이터가 있습니다.");
                return false;
            }

            var data = new PlayerSaveData
            {
                playerID = playerID,
                gold = 0,
                exp = 0
            };

            SaveLoadHelper.SavePlayer(data);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"초기 데이터 생성 실패: {exception.Message}");
            return false;
        }
    }

    /// <summary>
    /// 로그인 검증 성공 후 호출합니다.
    /// true를 반환했을 때 게임 씬으로 이동합니다.
    /// </summary>
    public bool LoadPlayerAfterLogin(string playerID)
    {
        if (isPlayerDataLoaded)
        {
            Debug.LogWarning("현재 계정에서 로그아웃한 뒤 로드하세요.");
            return false;
        }

        PlayerSaveData data;

        try
        {
            data = SaveLoadHelper.LoadPlayer(playerID);

            if (data == null)
            {
                Debug.LogError("해당 계정의 플레이어 저장 데이터가 없습니다.");
                return false;
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"플레이어 데이터 로드 실패: {exception.Message}");
            return false;
        }

        loadedPlayerID = playerID;
        currentData = data;

        NotifyPlayerDataChanged();
        return true;
    }

    /// <summary>
    /// 현재 플레이어 데이터를 로컬 JSON 파일에 저장합니다.
    /// 전투 보상, 장착, 강화 등의 데이터 변경 후 호출합니다.
    /// </summary>
    public bool SaveCurrentPlayerData()
    {
        if (!isPlayerDataLoaded)
        {
            Debug.LogWarning("저장할 플레이어 데이터가 없습니다.");
            return false;
        }

        if (currentData.playerID != loadedPlayerID)
        {
            Debug.LogError("현재 계정과 저장 대상 계정이 다릅니다.");
            return false;
        }

        try
        {
            SaveLoadHelper.SavePlayer(currentData);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"플레이어 데이터 저장 실패: {exception.Message}");
            return false;
        }
    }

    /// <summary>
    /// 데이터 변경 사실을 UI 등에 알립니다.
    /// 파일 저장은 별도로 호출해야 합니다.
    /// </summary>
    public void NotifyPlayerDataChanged()
    {
        if (isPlayerDataLoaded)
        {
            OnPlayerDataChanged?.Invoke(currentData);
        }
    }

    /// <summary>
    /// 저장에 성공한 경우에만 현재 계정 데이터를 해제합니다.
    /// true를 반환하면 로그인 화면으로 이동합니다.
    /// </summary>
    public bool Logout()
    {
        if (!isPlayerDataLoaded)
            return true;

        if (!SaveCurrentPlayerData())
            return false;

        currentData = null;
        loadedPlayerID = null;

        return true;
    }

    // 앱이 백그라운드로 이동할 때 보조 저장
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && isPlayerDataLoaded)
        {
            SaveCurrentPlayerData();
        }
    }

    // 종료 시 보조 저장
    private void OnApplicationQuit()
    {
        if (isPlayerDataLoaded)
        {
            SaveCurrentPlayerData();
        }
    }
}
