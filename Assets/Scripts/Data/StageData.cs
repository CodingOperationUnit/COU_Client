using System;
using System.Linq;
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
    public int clearAccountExp;
    public int[] rewardBoxGradeWeights;   // 보상상자 장비 등급 가중치. ItemGrade 순서(General, Super, Rare)

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

    // 보상상자 하나의 장비 등급을 가중치로 뽑는다
    public ItemGrade RollRewardBoxGrade()
    {
        int roll = UnityEngine.Random.Range(0, rewardBoxGradeWeights.Sum());
        for (int i = 0; i < rewardBoxGradeWeights.Length; i++)
        {
            roll -= rewardBoxGradeWeights[i];
            if (roll < 0) return (ItemGrade)i;
        }
        return ItemGrade.General;
    }
}
