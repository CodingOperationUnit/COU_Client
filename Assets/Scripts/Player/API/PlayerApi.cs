using System;
using System.Collections;
using UnityEngine;


// 플레이어 시스템 서버 API 호출부
// 사용 : StartCorutine(PlayerAPi.GetFinalStats(result => { .... }));
public static class PlayerApi
{
    public const string EvolutionUpgradePath = "/api/players/me/evolution/upgrade";
    public const string FinalStatsPath = "/api/players/me/stats";

    // 서버 에러 code
    public const string ErrorPlayerStatNotFound = "PLAYER_STAT_NOT_FOUND";
    public const string ErrorInternal = "INTERNAL_ERROR";

    // 진화 1단계 업그레이드
    public static IEnumerator UpgradeEvolution(Action<ApiResult<EvolutionUpgradeResponse>> onComplete)
        => ApiClient.Post(EvolutionUpgradePath, null, onComplete);

    // 최종 스탯 조회
    public static IEnumerator GetFinalStats(Action<ApiResult<FinalStatsResponse>> onComplete)
        => ApiClient.Get(FinalStatsPath, onComplete);
}
