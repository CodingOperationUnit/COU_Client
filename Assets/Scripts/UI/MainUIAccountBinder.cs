using System;
using System.Linq;
using UnityEngine;

public class MainUIAccountBinder : MonoBehaviour
{
    private void Start()
    {
        GameManager.PlayerData.OnPlayerDataChanged += RefreshAll;

        if (GameManager.PlayerData.isPlayerDataLoaded)
            RefreshAll(GameManager.PlayerData.currentData);
    }

    private void OnDestroy()
    {
        if (GameManager.PlayerData != null)
            GameManager.PlayerData.OnPlayerDataChanged -= RefreshAll;
    }

    private void RefreshAll(PlayerSaveData data)
    {
        RefreshTopBar(data);
        RefreshStageSelect(data);
    }

    private void RefreshTopBar(PlayerSaveData data)
    {
        var topBar = GameManager.UI.Get<TopBar>();
        var accountConst = GameManager.JsonData.AccountConstData;
        var profile = data.profile;

        topBar.SetNickname(profile.playerNickname);
        topBar.SetLevel(profile.accountLevel);
        topBar.SetExp(profile.accountLevel >= accountConst.maxAccountLevel
            ? 1f
            : profile.accountExp / (float)accountConst.GetRequiredExp(profile.accountLevel));
        // 최대 스태미나는 저장하지 않는 정적 값이라 AccountConst에서 읽는다
        topBar.SetStamina(data.currency.currencyEnergy, accountConst.maxStamina);
        topBar.SetGold(data.currency.currencyGold);
        topBar.SetGem(data.currency.currencyGem);
    }

    private void RefreshStageSelect(PlayerSaveData data)
    {
        var stageDataDic = GameManager.JsonData.StageDataDic; // 시트에서 온 공용 데이터
        if (stageDataDic == null || stageDataDic.Count == 0) return;

        var stages = stageDataDic.Values.OrderBy(s => s.stageId).ToArray();
        var progress = data.stageProgress;

        // StageSelectScreen은 기존 StageRecordSaveData 형식을 받으므로 StageProgress에서 변환
        // 최고 생존 기록은 데이터 정의서에 없어 0으로 둔다 (팀 결정 대기)
        var records = stages.ToDictionary(
            stage => stage.stageId,
            stage => new StageRecordSaveData
            {
                stageId = stage.stageId,
                isCleared = progress.IsCleared(stage.stageId),
                bestSurvivalSeconds = 0f
            });

        // 마지막으로 진행한 스테이지(currentStageId)를 기본 선택으로
        int selectedIndex = Math.Max(0, Array.FindIndex(stages, s => s.stageId == progress.currentStageId));

        GameManager.UI.Get<StageSelectScreen>().SetStages(stages, records, selectedIndex);

        var battleTab = GameManager.UI.Get<BattleTab>();
        battleTab.SetStage(stages[selectedIndex], 0);
        battleTab.SetStaminaCost(GameManager.JsonData.AccountConstData.battleStaminaCost);
    }
}