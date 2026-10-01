using System;
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
        RefreshStageClearInfo(data);
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
    
    private void RefreshStageClearInfo(PlayerSaveData data)
    {
    }
}
