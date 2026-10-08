using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class ServerLoadManager : MonoSingleton<ServerLoadManager>
{
    private bool isRequesting;

    // 로그인 직후 호출: 플레이어 데이터 + 장비 목록을 받아 PlayerDataManager에 넣는다
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

        try
        {
            string error = null;

            // 1) 플레이어 데이터 (프로필, 재화, 스테이지, 스탯)
            PlayerSaveData data = null;
            yield return RequestSave((loaded, message) =>
            {
                data = loaded;
                error = message;
            });

            if (data == null)
            {
                onComplete?.Invoke(false, error);
                yield break;
            }

            // 2) 장비 목록
            InventoryResponse inventory = null;
            yield return GetJson<InventoryResponse>(GameConstants.Server.INVENTORY_API,
                result => inventory = result, message => error = message);

            if (inventory == null)
            {
                onComplete?.Invoke(false, error);
                yield break;
            }

            ApplyInventory(data, inventory);

            // 3) PlayerDataManager에 보관
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
        }
        finally
        {
            // 성공/실패/중간 종료와 관계없이 다음 요청을 허용한다
            isRequesting = false;
        }

        GameManager.PlayerData.NotifyPlayerDataChanged();
        Debug.Log($"[ServerLoad] 불러오기 완료: playerId={GameManager.PlayerData.currentPlayerId}, " +
                  $"장비 {GameManager.PlayerData.currentData.inventoryList.Count}개");
        onComplete?.Invoke(true, "플레이어 데이터를 불러왔습니다.");
    }

    // GET /api/players/me/save 응답을 받아 검사만 한다. PlayerDataManager에는 넣지 않는다
    // (BattleManager.ReloadSave에서도 사용)
    // onComplete(받은 데이터, 실패 시 안내 메시지)
    public IEnumerator RequestSave(Action<PlayerSaveData, string> onComplete)
    {
        PlayerSaveData data = null;
        string error = null;

        yield return GetJson<PlayerSaveData>(GameConstants.Server.PLAYER_SAVE_API,
            result => data = result, message => error = message);

        if (data == null)
        {
            onComplete?.Invoke(null, error);
            yield break;
        }

        if (data.profile == null || data.currency == null
            || data.stageProgress == null || data.playerStat == null)
        {
            Debug.LogError("[ServerLoad] 응답에 필요한 데이터가 빠져 있습니다.");
            onComplete?.Invoke(null, "플레이어 데이터가 올바르지 않습니다.");
            yield break;
        }

        onComplete?.Invoke(data, null);
    }

    // 토큰을 붙여 GET 요청 → JSON을 T로 변환. 성공하면 onSuccess, 실패하면 onFail(화면용 문구)
    private IEnumerator GetJson<T>(string path, Action<T> onSuccess, Action<string> onFail) where T : class
    {
        using (var request = UnityWebRequest.Get(GameConstants.Server.BASE_URL + path))
        {
            request.SetRequestHeader("Authorization", GameManager.ServerLogin.AuthorizationHeader);
            request.timeout = GameConstants.Value.REQUEST_TIMEOUT_SECONDS;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning($"[ServerLoad] 서버 연결 실패 ({path}): {request.error}");
                onFail("서버에 연결할 수 없습니다. 잠시 후 다시 시도해 주세요.");
                yield break;
            }

            // 토큰이 없거나 만료/위조된 경우 (401은 본문이 비어 있음)
            if (request.responseCode == 401)
            {
                onFail("로그인 정보가 만료되었습니다. 다시 로그인해 주세요.");
                yield break;
            }

            if (request.responseCode != 200)
            {
                Debug.LogWarning($"[ServerLoad] 요청 실패 ({path}, {request.responseCode}): {request.downloadHandler.text}");
                onFail("플레이어 데이터를 불러오지 못했습니다.");
                yield break;
            }

            T body;
            try
            {
                body = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
            }
            catch (JsonException e)
            {
                Debug.LogError($"[ServerLoad] 응답 변환 실패 ({path}): {e.Message}");
                onFail("플레이어 데이터를 읽지 못했습니다.");
                yield break;
            }

            if (body == null)
            {
                onFail("플레이어 데이터가 비어 있습니다.");
                yield break;
            }

            onSuccess(body);
        }
    }

    // 서버 장비 목록 → 기존 클라 형식(inventoryList + profile 장착 칸)으로 옮겨 담는다
    // TODO(장비 long 전환): InventoryData가 long/inventoryEquipped로 바뀌면 형변환과 장착 칸 쓰기를 제거
    private static void ApplyInventory(PlayerSaveData data, InventoryResponse inventory)
    {
        ItemDatabase.Load();   // 로그인 씬에서는 아직 로드 전일 수 있다

        data.inventoryList = new List<InventoryData>();
        if (inventory.items == null) return;

        // 같은 슬롯에 장착 장비가 2개면 먼저 나온(작은 inventoryId) 장비만 사용 (서버 S1 규칙과 동일)
        foreach (var item in inventory.items.OrderBy(i => i.inventoryId))
        {
            data.inventoryList.Add(new InventoryData
            {
                inventoryId = (int)item.inventoryId,
                playerId = data.profile.playerId,
                itemId = (int)item.itemId,
                inventoryItemLevel = item.inventoryItemLevel,
                inventoryItemGrade = item.inventoryItemGrade,
                inventoryAcquiredAt = default   // 서버 응답에 아직 없음 (팀원에게 추가 요청)
            });

            if (!item.isEquipped || !TryGetSlot(item.itemId, out var slot))
                continue;

            if (PlayerInventory.GetEquippedInventoryId(data.profile, slot) == null)
                SetEquippedSlot(data.profile, slot, (int)item.inventoryId);
        }
    }

    private static bool TryGetSlot(long itemId, out EquipSlotType slot)
    {
        var itemData = ItemDatabase.GetAll().FirstOrDefault(d => d.itemId == itemId);
        slot = itemData?.SlotType ?? default;

        if (itemData == null)
            Debug.LogWarning($"[ServerLoad] 아이템 데이터에 없는 itemId {itemId}의 장착 정보는 건너뜁니다.");

        return itemData != null;
    }

    private static void SetEquippedSlot(PlayerProfileData profile, EquipSlotType slot, int inventoryId)
    {
        switch (slot)
        {
            case EquipSlotType.Weapon: profile.equippedWeaponInventoryId = inventoryId; break;
            case EquipSlotType.Armor: profile.equippedArmorInventoryId = inventoryId; break;
            case EquipSlotType.Belt: profile.equippedBeltInventoryId = inventoryId; break;
            case EquipSlotType.Gloves: profile.equippedGlovesInventoryId = inventoryId; break;
            case EquipSlotType.Necklace: profile.equippedNecklaceInventoryId = inventoryId; break;
            case EquipSlotType.Shoes: profile.equippedShoesInventoryId = inventoryId; break;
        }
    }
}