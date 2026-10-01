using System;
using UnityEngine;

// 스테이지 관련 데이터
[Serializable]
public class StageData
{
    public int stageID;
    public string stageName;
    public float duration;        // 스테이지 길이(초). 보스 등장 시각과 맞춰 둔 값
}
