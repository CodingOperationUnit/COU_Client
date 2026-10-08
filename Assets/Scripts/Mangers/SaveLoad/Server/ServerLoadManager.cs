using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class ServerLoadManager : MonoSingleton<ServerLoadManager>
{
    private bool isRequesting;
    
    // 로그인 직후 호출: 서버의 플레이어 데이터를 받아 PlayerDataManager에 넣는다
    // onComplete(성공 여부, 안내 메시지)
    public IEnumerator LoadPlayerData(Action<bool, string> onComplete)
    {
        var login = GameManager.ServerLogin;

        if (!login.IsLoggedIn)
        {
            onComplete?.Invoke(false, "로그인이 필요합니다.");
            yield break;
        }

        if (GameManager.PlayerData.isPlayerDataLoaded)
        {
            onComplete?.Invoke(false, "이미 플레이어 데이터가 로드되어 있습니다.");
            yield break;
        }

        if (isRequesting)
        {
            onComplete?.Invoke(false, "데이터를 불러오는 중입니다. 잠시만 기다려 주세요.");
            yield break;
        }

        isRequesting = true;

        string url = GameConstants.Server.BASE_URL + GameConstants.Server.PLAYER_SAVE_API;
        using (var request = UnityWebRequest.Get(url))
        {
            // 서버는 이 토큰으로 "누구의 데이터인지" (playerId)를 판단한다.
            request.SetRequestHeader("Authorization", login.AuthorizationHeader);
            request.timeout = GameConstants.Value.REQUEST_TIMEOUT_SECONDS;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning($"[ServerLoad] 서버 연결 실패: {request.error}");
                onComplete?.Invoke(false, "서버에 연결할 수 없습니다. 잠시 후 다시 시도해 주세요.");
                yield break;
            }
            
            // 토큰이 없거나 만료/위조된 경우 (401은 본문이 비어 있음)
            if (request.responseCode == 401)
            {
                onComplete?.Invoke(false, "로그인 정보가 만료되었습니다. 다시 로그인해 주세요.");
                yield break;
            }

            if (request.responseCode != 200)
            {
                Debug.LogWarning($"[ServerLoad] 불러오기 실패 ({request.responseCode}): {request.downloadHandler.text}");
                onComplete?.Invoke(false, "플레이어 데이터를 불러오지 못했습니다.");
                yield break;
            }

            PlayerSaveData data;
            try
            {
                data = JsonConvert.DeserializeObject<PlayerSaveData>(request.downloadHandler.text);
            }
            catch (JsonException e)
            {
                Debug.LogError($"[ServerLoad] 응답 변환 실패: {e.Message}");
                onComplete?.Invoke(false, "플레이어 데이터를 읽지 못했습니다.");
                yield break;
            }
            
            if (data?.profile == null || data.currency == null
                                      || data.stageProgress == null || data.playerStat == null)
            {
                Debug.LogError("[ServerLoad] 응답에 필요한 데이터가 빠져 있습니다.");
                onComplete?.Invoke(false, "플레이어 데이터가 올바르지 않습니다.");
                yield break;
            }

            // TODO(인벤토리 연동): GET /api/inventory 결과를 data.inventoryList에 채운다. 지금은 빈 목록
            data.inventoryList ??= new List<InventoryData>();

            try
            {
                GameManager.PlayerData.SetPlayerDataFromServer(login.CurrentAccount.accountLoginId, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ServerLoad] 데이터 적용 실패: {e.Message}");
                onComplete?.Invoke(false, "플레이어 데이터를 적용하지 못했습니다.");
                yield break;
            }
            
            GameManager.PlayerData.NotifyPlayerDataChanged();
            Debug.Log($"[ServerLoad] 불러오기 완료: playerId={GameManager.PlayerData.currentPlayerId}");
            onComplete?.Invoke(true, "플레이어 데이터를 불러왔습니다.");
        }
    }
}
