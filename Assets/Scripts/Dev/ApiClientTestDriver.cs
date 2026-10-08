using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

public class ApiClientTestDriver : MonoBehaviour
{
    [ContextMenu("1. 정적 데이터 (인증 없음")]
    private void TestStaticData()
    {
        StartCoroutine(ApiClient.Get<JToken>(GameConstants.Server.STATIC_DATA_API, result =>
        {
            Debug.Log("[ApiClientTest] 정적 데이터 : " + result.Describe());
        }, auth: false));
    }

    [ContextMenu("2. 최종 스탯 (인증 필요)")]
    private void TestFinalStats()
    {
        StartCoroutine(ApiClient.Get<JToken>("/api/players/me/stats", result =>
        {
            Debug.Log("[ApiClientTest] 최종 스탯 : " + result.Describe());
            if (result.IsSuccess)
                Debug.Log(result.Data.ToString());
        }));
    }

    [ContextMenu("3. 없는 주소 (404 형식 확인)")]
    private void TestNotFound()
    {
        StartCoroutine(ApiClient.Get<JToken>("/api/players/me/not-exist", result =>
        {
            Debug.Log("[ApiClientTest] 없는 주소 : " + result.Describe());
        }));
    }

    [ContextMenu("4. 토큰 비우고 최종 스탯 (401 확인)")]
    private void TestUnauthorized()
    {
        ApiClient.ClearAccessToken();
        ApiClient.OnUnauthorized += LogUnauthorized;
        TestFinalStats();
    }

    private void LogUnauthorized()
    {
        ApiClient.OnUnauthorized -= LogUnauthorized;
        Debug.Log("[ApiClientTest] OnUnauthorized 이벤트 수신");
    }
}
