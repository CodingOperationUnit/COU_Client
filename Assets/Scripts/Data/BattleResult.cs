public class BattleResult
{
    // 씬 전환 후 메인씬에서 읽는 가장 최근 전투 결과
    public static BattleResult Last { get; set; }

    public int StageID;
    public bool Victory;
    public int Seconds;
    public int Kills;
    public int Gold;
    public int RewardBoxes;
    public int AccountExp;
}
