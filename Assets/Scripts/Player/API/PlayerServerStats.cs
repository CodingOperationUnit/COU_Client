using System;
using System.Collections;
using UnityEngine;

public static class PlayerServerStats
{
    public static FinalStatsResponse Stats { get; private set; }
    public static bool HasStats => Stats != null;

    public static IEnumerator Fetch(Action<ApiResult<FinalStatsResponse>> onComplete)
    {
        Clear();

        yield return PlayerApi.GetFinalStats(result =>
        {
            if (result.IsSuccess && result.Data != null)
                Stats = result.Data;
            else
                Debug.LogError("[PlayerServerStats] 최종 스탯을 불러오지 못했습니다: " + result.Describe());

            onComplete?.Invoke(result);
        });
    }

    public static void Clear() => Stats = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Stats = null;
}
