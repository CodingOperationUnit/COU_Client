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
        public const string BATTLE_SCENE = "BattleScene";
    }
    
    // 파일 경로 관련 상수
    public static class Paths
    {
        public const string ACCOUNT_SAVE_PATH = "accounts.json";
        public const string PLAYER_DIRECTORY = "Players";
        public const string BossAttackData_Json_Path = "JsonFiles/BossAttack";
        public const string WaveData_Json_Path = "JsonFiles/Wave";
        public const string SpawnPatternData_Json_Path = "JsonFiles/SpawnPattern";
        public const string DropTableData_Json_Path = "JsonFiles/DropTable";
        public const string StageData_Json_Path = "JsonFiles/Stage";
        public const string MonsterData_Json_Path = "JsonFiles/Monster";
        public const string ItemData_Json_Path = "JsonFiles/Item";
        public const string SkillData_Json_Path = "JsonFiles/Skill";
        public const string DropItemData_Json_Path = "JsonFiles/DropItem";
        public const string AccountConstData_Json_Path = "JsonFiles/AccountConst";
        public const string STATIC_DATA_DIRECTORY = "StaticData";
        public const string STATIC_DATA_VERSION_FILE = "version.txt";
    }

    // 서버 통신 상수
    public static class Server
    {
        public const string BASE_URL = "http://localhost:8080";
        public const string STATIC_DATA_API = "/api/static-data";
        // 이 빌드가 읽을 수 있는 정적 데이터 버전의 첫째 자리
        public const int STATIC_DATA_MAJOR_VERSION = 2;
        public const string SIGNUP_API = "/api/accounts/signup";
        public const string LOGIN_API = "/api/accounts/login";
        public const string PLAYER_SAVE_API = "/api/players/me/save"; 
    }

    public static class Value
    {
        public const int REQUEST_TIMEOUT_SECONDS = 10;
        public const long CONNECTION_FAILED = 0;
        public const long ALREADY_REQUESTING = -1; 
    }
}
