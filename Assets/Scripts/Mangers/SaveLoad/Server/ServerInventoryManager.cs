using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

// 장비 장착/해제/레벨 업/합성을 서버 API로 요청하고, 응답을 PlayerInventory에 반영한다
public class ServerInventoryManager : MonoSingleton<ServerInventoryManager>
{
    private bool isRequesting;
    public bool IsRequesting => isRequesting;
    
    // ===== 장착 / 해제 =====
    public void Equip(OwnedItem item, Action<bool, string> onComplete)
        => Post<EquipResponse>(item, "equip",
            response => PlayerInventory.Instance.ApplyEquip(response), onComplete);
    
    public void Unequip(OwnedItem item, Action<bool, string> onComplete)
        => Post<EquipmentResponse>(item, "unequip",
            response => PlayerInventory.Instance.ApplyUnequip(response), onComplete);
    
    // ===== 레벨업 =====
    public void LevelUp(OwnedItem item, Action<bool, string> onComplete)
        => Post<LevelUpResponse>(item, "levelup",
            response => PlayerInventory.Instance.ApplyLevelUp(
                response.inventoryId, response.inventoryItemLevel, response.currencyGold),
            onComplete);

    public void BatchLevelUp(OwnedItem item, Action<bool, string> onComplete)
        => Post<LevelUpBatchResponse>(item, "levelup/batch",
            response => PlayerInventory.Instance.ApplyLevelUp(
                response.inventoryId, response.inventoryItemLevel, response.currencyGold),
            onComplete);

    // ===== 합성 =====
    public void Synthesize(OwnedItem item, Action<bool, string> onComplete)
        => Post<SynthesizeResponse>(item, "synthesize",
            response => PlayerInventory.Instance.ApplySynthesis(response), onComplete);

    public void BatchSynthesize(OwnedItem item, Action<bool, string> onComplete)
        => Post<SynthesizeResponse>(item, "synthesize/batch",
            response => PlayerInventory.Instance.ApplySynthesis(response), onComplete);
    
    // POST /api/inventory/{inventoryId}/{action} → 성공하면 apply(응답), 끝나면 onComplete(성공 여부, 실패 문구)
    private void Post<T>(OwnedItem item, string action, Action<T> apply, Action<bool, string> onComplete)
        where T : class
    {
        if (isRequesting)
        {
            onComplete?.Invoke(false, "요청을 처리하고 있습니다. 잠시만 기다려 주세요.");
            return;
        }

        long inventoryId = PlayerInventory.Instance.GetInventoryId(item);
        if (inventoryId <= 0)
        {
            onComplete?.Invoke(false, "서버에 없는 장비입니다.");
            return;
        }

        string path = $"{GameConstants.Server.INVENTORY_API}/{inventoryId}/{action}";
        StartCoroutine(PostRoutine(path, apply, onComplete));
    }

    private IEnumerator PostRoutine<T>(string path, Action<T> apply, Action<bool, string> onComplete)
        where T : class
    {
        isRequesting = true;

        // 본문 없는 POST (어떤 장비에 무엇을 할지는 URL로 전달)
        using (var request = new UnityWebRequest(GameConstants.Server.BASE_URL + path, UnityWebRequest.kHttpVerbPOST))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", GameManager.ServerLogin.AuthorizationHeader);
            request.timeout = GameConstants.Value.REQUEST_TIMEOUT_SECONDS;

            yield return request.SendWebRequest();

            isRequesting = false;

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning($"[ServerInventory] 서버 연결 실패 ({path}): {request.error}");
                onComplete?.Invoke(false, "서버에 연결할 수 없습니다. 잠시 후 다시 시도해 주세요.");
                yield break;
            }

            if (request.responseCode == 401)
            {
                onComplete?.Invoke(false, "로그인 정보가 만료되었습니다. 다시 로그인해 주세요.");
                yield break;
            }

            string text = request.downloadHandler.text;

            // 400/403/404: 서버가 보낸 message 사용 (예: "골드가 부족합니다.", "합성 재료가 부족합니다.")
            if (request.responseCode != 200)
            {
                Debug.LogWarning($"[ServerInventory] 요청 실패 ({path}, {request.responseCode}): {text}");
                onComplete?.Invoke(false, ReadErrorMessage(text));
                yield break;
            }

            T body;
            try
            {
                body = JsonConvert.DeserializeObject<T>(text);
            }
            catch (JsonException e)
            {
                Debug.LogError($"[ServerInventory] 응답 변환 실패 ({path}): {e.Message}");
                onComplete?.Invoke(false, "응답을 읽지 못했습니다.");
                yield break;
            }

            apply(body);
            onComplete?.Invoke(true, null);
        }
    }

    private static string ReadErrorMessage(string responseText)
    {
        try
        {
            var error = JsonConvert.DeserializeObject<ErrorResponse>(responseText);
            if (!string.IsNullOrEmpty(error?.message))
                return error.message;
        }
        catch (JsonException)
        {
            // 형식이 다르면 기본 문구
        }

        return "요청을 처리하지 못했습니다.";
    }
}
