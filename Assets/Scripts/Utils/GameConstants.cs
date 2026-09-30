/// <summary>
/// 게임에서 사용되는 모든 상수값들 모아놓은 스크립트
/// </summary>

public static class GameConstants
{
    // 씬 이름 상수
    public static class SceneNames
    {
        public const string MANAGERS_TEST_SCENE = "ManagersTestScene";
        public const string MANAGERS_TEST_SCENE_2 = "ManagersTestScene_2";
        public const string LOGIN_SCENE = "LogInScene";
        public const string MAIN_SCENE = "MainScene";
    }
    
    // 파일 경로 관련 상수
    public static class Paths
    {
        public const string ACCOUNT_SAVE_PATH = "accounts.json";
        public const string PLAYER_DIRECTORY = "Players";
        public const string BossAttackData_Json_Path = "JsonFiles/BossAttack";
        public const string SpawnData_Json_Path = "JsonFiles/Spawn";
        public const string StageData_Json_Path = "JsonFiles/Stage";
        public const string MonsterData_Json_Path = "JsonFiles/Monster";
    }
}
