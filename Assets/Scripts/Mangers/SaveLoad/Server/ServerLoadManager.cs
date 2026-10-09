using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            // 1) 플레이어 데이터 (프로필, 재화, 스테이지, 스탯)
            PlayerSaveData data = null;
            string error = null;
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
            ApiResult<InventoryResponse> inventoryResult = null;
            yield return InventoryApi.GetInventory(result => inventoryResult = result);

            if (!inventoryResult.IsSuccess)
            {
                onComplete?.Invoke(false, inventoryResult.ToUserMessage("장비 목록을 불러오지 못했습니다."));
                yield break;
            }

            ApplyInventory(data, inventoryResult.Data);

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
        ApiResult<PlayerSaveData> result = null;
        yield return AccountApi.GetSave(r => result = r);

        if (!result.IsSuccess)
        {
            onComplete?.Invoke(null, result.ToUserMessage("플레이어 데이터를 불러오지 못했습니다."));
            yield break;
        }

        var data = result.Data;
        if (data?.profile == null || data.currency == null
            || data.stageProgress == null || data.playerStat == null)
        {
            Debug.LogError("[ServerLoad] 응답에 필요한 데이터가 빠져 있습니다.");
            onComplete?.Invoke(null, "플레이어 데이터가 올바르지 않습니다.");
            yield break;
        }

        onComplete?.Invoke(data, null);
    }

    // 서버 장비 목록 → 기존 클라 형식(inventoryList + profile 장착 칸)으로 옮겨 담는다
    // TODO(장비 long 전환): InventoryData가 long/inventoryEquipped로 바뀌면 형변환과 장착 칸 쓰기를 제거
    private static void ApplyInventory(PlayerSaveData data, InventoryResponse inventory)
    {
        data.inventoryList = new List<InventoryData>();
        if (inventory?.items == null) return;

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
        GameManager.JsonData.ItemDataDic.TryGetValue(itemId, out var itemData);
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