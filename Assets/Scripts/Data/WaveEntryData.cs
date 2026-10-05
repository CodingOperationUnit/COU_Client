using System;

// 웨이브의 한 줄: "이 웨이브의 이 시각에, 이 패턴으로, 이 몬스터를" 뽑는다.
// 웨이브는 스테이지와 1:1이며 Stage.json의 waveId가 참조한다.
[Serializable]
public class WaveEntryData
{
    public int waveEntryId;
    public int waveId;
    public float patternStartTime;  // 시작 시각(초)
    public int patternId;         // SpawnPattern.json 참조
    public int monsterId;         // 뽑을 몬스터의 ID
}
