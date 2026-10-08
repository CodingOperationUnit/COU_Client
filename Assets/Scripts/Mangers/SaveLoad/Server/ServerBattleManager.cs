using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

// 전투 입장과 결과를 서버로 처리한다. 요청 중 상태는 요청한 UI가 가진다
public class ServerBattleManager : MonoSingleton<ServerBattleManager>
{
    // 전투 입장: 성공하면 차감 후 재화를 반영한다. onComplete(battleId, 실패 시 에러)
    public IEnumerator EnterBattle(int stageId, Action<long, ErrorResponse> onComplete)
    {
        var body = new BattleEnterRequest { stageId = stageId };

        yield return PostJson<BattleEnterResponse>(GameConstants.Server.BATTLE_API, body, (response, error) =>
        {
            if (error != null)
            {
                onComplete?.Invoke(0, error);
                return;
            }

            GameManager.PlayerData.currentData.currency = response.currency;
            GameManager.PlayerData.NotifyPlayerDataChanged();
            onComplete?.Invoke(response.battleId, null);
        });
    }

    // 전투 결과: 성공하면 지급 결과를 PlayerSaveData에 덮어쓰고 보상 장비를 inventoryList에 넣는다
    // onComplete(성공 응답, 실패 시 에러)
    public IEnumerator SendResult(long battleId, BattleResultRequest body, Action<BattleResultResponse, ErrorResponse> onComplete)
    {
        string path = string.Format(GameConstants.Server.BATTLE_RESULT_API, battleId);

        yield return PostJson<BattleResultResponse>(path, body, (response, error) =>
        {
            if (error == null)
            {
                var data = GameManager.PlayerData.currentData;
                ApplyBattleChanges(data, response.profile, response.currency, response.stageProgress);
                SetStageRecord(data, response.stageRecord);
                data.inventoryList.AddRange(response.rewards);
                GameManager.PlayerData.NotifyPlayerDataChanged();
            }

            onComplete?.Invoke(response, error);
        });
    }

    // 결과가 반영됐는데 응답을 못 받은 경우(BATTLE_ALREADY_COMPLETED): 세이브를 다시 받아 전투가 바꾸는 값을 덮어쓴다
    // TODO(인벤토리 연동): 보상 장비까지 받으려면 GET /api/inventory도 다시 받아야 한다. 지금은 inventoryList를 유지한다
    public IEnumerator ReloadSave(Action<bool, string> onComplete)
    {
        yield return GameManager.ServerLoad.RequestSave((loaded, message) =>
        {
            if (loaded == null)
            {
                onComplete?.Invoke(false, message);
                return;
            }

            var data = GameManager.PlayerData.currentData;
            ApplyBattleChanges(data, loaded.profile, loaded.currency, loaded.stageProgress);
            data.stageRecords = loaded.stageRecords;
            GameManager.PlayerData.NotifyPlayerDataChanged();
            onComplete?.Invoke(true, null);
        });
    }

    // 전투가 바꾸는 값: 계정 레벨·경험치, 재화, 스테이지 진행
    // profile은 장착 칸을 유지하려고 레벨과 경험치만 옮긴다
    private static void ApplyBattleChanges(PlayerSaveData data, PlayerProfileData profile, CurrencyData currency, StageProgressData stageProgress)
    {
        data.profile.accountLevel = profile.accountLevel;
        data.profile.accountExp = profile.accountExp;
        data.currency = currency;
        data.stageProgress = stageProgress;
    }

    private static void SetStageRecord(PlayerSaveData data, StageRecordSaveData record)
    {
        int index = data.stageRecords.FindIndex(saved => saved.stageId == record.stageId);
        if (index >= 0)
            data.stageRecords[index] = record;
        else
            data.stageRecords.Add(record);
    }

    // 인증 헤더를 붙여 JSON 본문으로 POST 요청을 보낸다. onComplete(200 응답, 실패 시 에러)
    // 연결 실패는 status가 CONNECTION_FAILED인 에러로 넘긴다
    private static IEnumerator PostJson<T>(string path, object body, Action<T, ErrorResponse> onComplete) where T : class
    {
        string json = JsonConvert.SerializeObject(body);

        using (var request = new UnityWebRequest(GameConstants.Server.BASE_URL + path, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", GameManager.ServerLogin.AuthorizationHeader);
            request.timeout = GameConstants.Value.REQUEST_TIMEOUT_SECONDS;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning($"[ServerBattle] 서버 연결 실패: {request.error}");
                onComplete(null, new ErrorResponse { status = (int)GameConstants.Value.CONNECTION_FAILED });
                yield break;
            }

            int status = (int)request.responseCode;
            string text = request.downloadHandler.text;

            if (status == 200)
            {
                var response = Deserialize<T>(text);
                if (response != null)
                {
                    onComplete(response, null);
                    yield break;
                }
            }

            Debug.LogWarning($"[ServerBattle] 요청 실패 ({status}): {text}");
            var error = Deserialize<ErrorResponse>(text) ?? new ErrorResponse();
            error.status = status;
            onComplete(null, error);
        }
    }

    // 본문이 비었거나(401) 형식이 다르면 null
    private static T Deserialize<T>(string text) where T : class
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(text);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
