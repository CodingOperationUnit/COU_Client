using System;
using UnityEngine;

// 스테이지 관련 데이터
[Serializable]
public class StageData
{
    public int stageID;
    public string stageName;
    public string illustrationColor;
    public float duration;        // 스테이지 길이(초). 보스 등장 시각과 맞춰 둔 값
    public string stageDescription;

    [NonSerialized] private Color parsedColor;
    public Color IllustrationColor => parsedColor;

    public void OnLoaded()
    {
        if (!ColorUtility.TryParseHtmlString(illustrationColor, out parsedColor))
        {
            Debug.LogWarning($"[StageData] {stageID}의 illustrationColor \"{illustrationColor}\"을(를) 파싱할 수 없어 회색으로 처리합니다.");
            parsedColor = Color.gray;
        }
    }
}
