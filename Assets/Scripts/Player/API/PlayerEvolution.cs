using System;
using System.Collections;
using UnityEngine;

public static class PlayerEvolution
{
    public static IEnumerator Upgrade(Action<ApiResult<EvolutionUpgradeResponse>> onComplete)
    {
        yield return PlayerApi.UpgradeEvolution(result =>
        {
            if (result.IsSuccess && result.Data != null)
                Apply(result.Data);

            onComplete?.Invoke(result);
        });
    }

    public static void Apply (EvolutionUpgradeResponse response)
    {
        var dataManager = GameManager.PlayerData;
        if (dataManager == null || !dataManager.isPlayerDataLoaded) return;

        var data = dataManager.currentData;
        if (response.playerStat != null)
            data.playerStat = response.playerStat;

        if (response.currency != null)
            data.currency = response.currency;

        dataManager.NotifyPlayerDataChanged();
    }
}
