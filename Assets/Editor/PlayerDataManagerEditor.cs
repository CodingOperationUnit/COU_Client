using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerDataManager))]
public class PlayerDataManagerEditor : Editor
{
    private bool showProfile = true;
    private bool showCurrency = true;
    private bool showEquipped = true;
    private bool showInventory = true;
    private bool showStageProgress = true;
    private bool showPlayerStat;

    private static readonly EquipSlotType[] Slots =
        (EquipSlotType[])Enum.GetValues(typeof(EquipSlotType));

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("현재 플레이어 데이터", EditorStyles.boldLabel);

        var manager = (PlayerDataManager)target;
        var data = manager.currentData;

        if (data == null)
        {
            EditorGUILayout.HelpBox("로드된 플레이어 데이터가 없습니다.", MessageType.Info);
            return;
        }

        // 조회만 수행합니다. 원본 데이터와 리스트를 생성하거나 수정하지 않습니다.
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.TextField("로그인 아이디 (로컬)", manager.currentAccountLoginId ?? string.Empty);
        }

        DrawProfile(data.profile);
        DrawCurrency(data.currency);
        DrawEquipped(data);
        DrawInventory(data);
        DrawStageProgress(data.stageProgress);
        DrawPlayerStat(data.playerStat);
    }

    // ===== PlayerProfile =====
    private void DrawProfile(PlayerProfileData profile)
    {
        if (!BeginSection(ref showProfile, "프로필 (PlayerProfile)", profile))
            return;

        using (new EditorGUI.IndentLevelScope())
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.IntField("플레이어 ID", profile.playerId);
            EditorGUILayout.IntField("계정 ID", profile.accountId);
            EditorGUILayout.TextField("닉네임", profile.playerNickname ?? string.Empty);
            EditorGUILayout.IntField("계정 레벨", profile.accountLevel);
            EditorGUILayout.IntField("계정 경험치 (현재 레벨 기준)", profile.accountExp);
        }
    }

    // ===== Currency =====
    private void DrawCurrency(CurrencyData currency)
    {
        if (!BeginSection(ref showCurrency, "재화 (Currency)", currency))
            return;

        var accountConst = Application.isPlaying ? GameManager.JsonData?.AccountConstData : null;
        string maxStamina = accountConst != null ? accountConst.maxStamina.ToString() : "-";

        using (new EditorGUI.IndentLevelScope())
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.IntField("골드", currency.currencyGold);
            EditorGUILayout.IntField("보석", currency.currencyGem);
            EditorGUILayout.TextField("스태미나 (Energy)", $"{currency.currencyEnergy} / {maxStamina}");
            EditorGUILayout.TextField("스태미나 갱신 시각", FormatDate(currency.currencyEnergyUpdatedAt));
        }
    }

    // ===== PlayerProfile.equipped{Slot}InventoryId =====
    private void DrawEquipped(PlayerSaveData data)
    {
        if (!BeginSection(ref showEquipped, "장착 장비 (equipped{Slot}InventoryId)", data.profile))
            return;

        using (new EditorGUI.IndentLevelScope())
        using (new EditorGUI.DisabledScope(true))
        {
            foreach (var slot in Slots)
            {
                int? inventoryId = PlayerInventory.GetEquippedInventoryId(data.profile, slot);
                string label = inventoryId.HasValue
                    ? $"inventoryId {inventoryId.Value} (itemId {FindItemId(data, inventoryId.Value)})"
                    : "(비어 있음)";

                EditorGUILayout.TextField(slot.ToString(), label);
            }
        }
    }

    // ===== Inventory =====
    private void DrawInventory(PlayerSaveData data)
    {
        EditorGUILayout.Space();
        showInventory = EditorGUILayout.Foldout(showInventory,
            $"보유 장비 (Inventory) ({data.inventoryList?.Count ?? 0})", true);

        if (!showInventory)
            return;

        if (data.inventoryList == null)
        {
            EditorGUILayout.HelpBox("장비 목록이 null입니다.", MessageType.Info);
            return;
        }

        if (data.inventoryList.Count == 0)
        {
            EditorGUILayout.HelpBox("보유 장비가 없습니다.", MessageType.Info);
            return;
        }

        using (new EditorGUI.IndentLevelScope())
        {
            for (int i = 0; i < data.inventoryList.Count; i++)
            {
                var inventory = data.inventoryList[i];
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField($"장비 {i + 1}", EditorStyles.boldLabel);
                    if (inventory == null)
                    {
                        EditorGUILayout.HelpBox("장비 데이터가 null입니다.", MessageType.Warning);
                        continue;
                    }

                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.IntField("보유 장비 ID", inventory.inventoryId);
                        EditorGUILayout.IntField("아이템 ID", inventory.itemId);
                        EditorGUILayout.IntField("장비 레벨", inventory.inventoryItemLevel);
                        EditorGUILayout.TextField("장비 등급",
                            inventory.inventoryItemGrade.HasValue ? inventory.inventoryItemGrade.Value.ToString() : "(기본 등급)");
                        EditorGUILayout.TextField("획득 시각", FormatDate(inventory.inventoryAcquiredAt));
                        EditorGUILayout.Toggle("장착 여부", IsEquipped(data.profile, inventory.inventoryId));
                    }
                }
            }
        }
    }

    // ===== StageProgress =====
    private void DrawStageProgress(StageProgressData progress)
    {
        if (!BeginSection(ref showStageProgress, "스테이지 진행 (StageProgress)", progress))
            return;

        using (new EditorGUI.IndentLevelScope())
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.IntField("현재 스테이지 ID", progress.currentStageId);
            EditorGUILayout.TextField("최고 클리어 스테이지 ID",
                progress.maxClearedStageId.HasValue ? progress.maxClearedStageId.Value.ToString() : "(클리어 이력 없음)");
        }
    }

    // ===== PlayerStat (필드만) =====
    private void DrawPlayerStat(PlayerStatData stat)
    {
        if (!BeginSection(ref showPlayerStat, "스탯 강화 (PlayerStat, 필드만)", stat))
            return;

        using (new EditorGUI.IndentLevelScope())
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.IntField("공격력 강화 레벨", stat.playerStatAttackLevel);
            EditorGUILayout.IntField("체력 강화 레벨", stat.playerStatHpLevel);
            EditorGUILayout.IntField("방어력 강화 레벨", stat.playerStatDefenseLevel);
        }
    }

    // ===== 공통 =====

    // Foldout을 그리고, 펼쳐져 있으며 데이터가 null이 아닐 때만 true
    private static bool BeginSection(ref bool foldout, string title, object section)
    {
        EditorGUILayout.Space();
        foldout = EditorGUILayout.Foldout(foldout, title, true);

        if (!foldout)
            return false;

        if (section == null)
        {
            EditorGUILayout.HelpBox($"{title} 데이터가 null입니다.", MessageType.Warning);
            return false;
        }

        return true;
    }

    private static bool IsEquipped(PlayerProfileData profile, int inventoryId)
    {
        if (profile == null)
            return false;

        foreach (var slot in Slots)
        {
            if (PlayerInventory.GetEquippedInventoryId(profile, slot) == inventoryId)
                return true;
        }

        return false;
    }

    private static string FindItemId(PlayerSaveData data, int inventoryId)
    {
        var found = data.inventoryList?.Find(i => i != null && i.inventoryId == inventoryId);
        return found != null ? found.itemId.ToString() : "목록에 없음";
    }

    private static string FormatDate(DateTime? dateTime)
    {
        return dateTime.HasValue ? dateTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "(없음)";
    }

    public override bool RequiresConstantRepaint()
    {
        return EditorApplication.isPlaying;
    }
}