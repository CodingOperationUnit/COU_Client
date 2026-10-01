using System;
using System.Collections.Generic;
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
        
        topBar.SetNickname(data.playerID);
        topBar.SetLevel(data.accountLevel);
        topBar.SetExp(data.accountExp);
        topBar.SetStamina(data.currentStamina, data.maxStamina);
        topBar.SetGold(data.gold);
        topBar.SetGem(data.gem);
    }
    
    private void RefreshStageSelect(PlayerSaveData data)
    {
        var stageDataDic = GameManager.JsonData.StageDataDic; // 시트에서 온 공용 데이터
        if (stageDataDic == null || stageDataDic.Count == 0) return;

        var records = data.stageRecordList?.ToDictionary(r => r.stageID)  // 계정별 기록
                      ?? new Dictionary<int, StageRecordSaveData>();

        var stages = stageDataDic.Values.OrderBy(s => s.stageID).ToArray();

        GameManager.UI.Get<StageSelectScreen>().SetStages(stages, records, 0);

        records.TryGetValue(stages[0].stageID, out var firstRecord);
        var firstBestTime = firstRecord != null ? Mathf.RoundToInt(firstRecord.bestSurvivalSeconds) : 0;
        GameManager.UI.Get<BattleTab>().SetStage(stages[0], firstBestTime);
    }
}
