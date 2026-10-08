using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;


public class JsonDataManager : MonoSingleton<JsonDataManager>
{
    #region Fields

    private Dictionary<int, MonsterData> monsterDataDic;
    public Dictionary<int, MonsterData> MonsterDataDic => monsterDataDic;

    private Dictionary<int, MonsterAttackData> monsterAttackDataDic;
    public IReadOnlyDictionary<int, MonsterAttackData> MonsterAttackDataDic => monsterAttackDataDic;

    private Dictionary<int, List<MonsterAttackData>> monsterAttacksByMonsterId = new();

    private static readonly List<MonsterAttackData> EmptyAttacks = new();

    private Dictionary<int, WaveEntryData> waveEntryDataDic;
    public IReadOnlyDictionary<int, WaveEntryData> WaveEntryDataDic => waveEntryDataDic;

    private Dictionary<int, SpawnPatternData> spawnPatternDataDic;

    private Dictionary<int, List<DropTableEntryData>> dropTableDic;

    private Dictionary<int, StageData> stageDataDic;
    public IReadOnlyDictionary<int, StageData> StageDataDic => stageDataDic;

    private Dictionary<int, SkillData> skillDataDic;
    public IReadOnlyDictionary<int, SkillData> SkillDataDic => skillDataDic;

    private Dictionary<DropItemType, DropItemData> dropItemDataDic;
    public IReadOnlyDictionary<DropItemType, DropItemData> DropItemDataDic => dropItemDataDic;

    private AccountConstData accountConstData;
    public AccountConstData AccountConstData => accountConstData;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;

        monsterDataDic = LoadTable(GameConstants.Paths.MonsterData_Json_Path, ParseMonsterData);
        monsterAttackDataDic = LoadTable(GameConstants.Paths.MonsterAttackData_Json_Path, ParseMonsterAttackData);
        waveEntryDataDic = LoadTable(GameConstants.Paths.WaveData_Json_Path, ParseWaveData);
        spawnPatternDataDic = LoadTable(GameConstants.Paths.SpawnPatternData_Json_Path, ParseSpawnPatternData);
        stageDataDic = LoadTable(GameConstants.Paths.StageData_Json_Path, ParseStageData);
        skillDataDic = LoadTable(GameConstants.Paths.SkillData_Json_Path, ParseSkillData);
        dropItemDataDic = LoadTable(GameConstants.Paths.DropItemData_Json_Path, ParseDropItemData);
        dropTableDic = LoadTable(GameConstants.Paths.DropTableData_Json_Path, ParseDropTableData);
        accountConstData = LoadTable(GameConstants.Paths.AccountConstData_Json_Path, ParseAccountConstData);

        RebuildMonsterAttackIndex();
    }

    // 로그인 전에 서버 정적 데이터를 확인한다. 요청이 실패하면 가진 데이터를 유지한다
    // onComplete: 앱 업데이트가 필요하면 true
    public IEnumerator FetchStaticData(Action<bool> onComplete)
    {
        string url = GameConstants.Server.BASE_URL + GameConstants.Server.STATIC_DATA_API;
        string savedVersion = LoadUsableSavedVersion();
        if (savedVersion != null)
            url += "?version=" + UnityWebRequest.EscapeURL(savedVersion);

        bool appUpdateRequired = false;

        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            // 204면 가진 데이터가 최신이다
            if (request.result != UnityWebRequest.Result.Success)
                Debug.LogWarning($"정적 데이터 요청 실패, 가진 데이터를 유지합니다: {request.error}");
            else if (request.responseCode == 200)
                appUpdateRequired = ApplyStaticDataResponse(request.downloadHandler.text);
        }

        onComplete?.Invoke(appUpdateRequired);
    }

    // 200 응답을 적용한다. 버전 첫째 자리가 빌드 값과 다르면 받은 데이터를 쓰지 않고 true(앱 업데이트 필요)를 반환한다
    private bool ApplyStaticDataResponse(string responseText)
    {
        try
        {
            JObject body = JObject.Parse(responseText);
            string version = body.Value<string>("version");

            if (!TryGetMajorVersion(version, out int major))
                throw new InvalidOperationException($"버전 형식이 올바르지 않습니다: {version}");

            if (major != GameConstants.Server.STATIC_DATA_MAJOR_VERSION)
                return true;

            JObject tables = body["tables"] as JObject
                ?? throw new InvalidOperationException("tables가 없습니다.");

            // 모두 파싱한 뒤 저장하고 한 번에 교체한다. 응답에 없는 테이블(null)은 가진 데이터를 유지한다
            var monsters = ParseResponseTable(tables, GameConstants.Paths.MonsterData_Json_Path, ParseMonsterData);
            var monsterAttacks = ParseResponseTable(tables, GameConstants.Paths.MonsterAttackData_Json_Path, ParseMonsterAttackData);
            var waveEntries = ParseResponseTable(tables, GameConstants.Paths.WaveData_Json_Path, ParseWaveData);
            var spawnPatterns = ParseResponseTable(tables, GameConstants.Paths.SpawnPatternData_Json_Path, ParseSpawnPatternData);
            var stages = ParseResponseTable(tables, GameConstants.Paths.StageData_Json_Path, ParseStageData);
            var skills = ParseResponseTable(tables, GameConstants.Paths.SkillData_Json_Path, ParseSkillData);
            var dropItems = ParseResponseTable(tables, GameConstants.Paths.DropItemData_Json_Path, ParseDropItemData);
            var dropTables = ParseResponseTable(tables, GameConstants.Paths.DropTableData_Json_Path, ParseDropTableData);
            var accountConst = ParseResponseTable(tables, GameConstants.Paths.AccountConstData_Json_Path, ParseAccountConstData);
            var items = ParseResponseTable(tables, GameConstants.Paths.ItemData_Json_Path, ItemDatabase.Parse);

            SaveLoadHelper.SaveStaticData(version, tables);

            monsterDataDic = monsters ?? monsterDataDic;
            monsterAttackDataDic = monsterAttacks ?? monsterAttackDataDic;
            waveEntryDataDic = waveEntries ?? waveEntryDataDic;
            spawnPatternDataDic = spawnPatterns ?? spawnPatternDataDic;
            stageDataDic = stages ?? stageDataDic;
            skillDataDic = skills ?? skillDataDic;
            dropItemDataDic = dropItems ?? dropItemDataDic;
            dropTableDic = dropTables ?? dropTableDic;
            accountConstData = accountConst ?? accountConstData;

            if (skills != null)
                SkillDataBase.Replace(skills);

            if (items != null)
                ItemDatabase.Replace(items);

            if (monsterAttacks != null)
                RebuildMonsterAttackIndex();

            Debug.Log($"정적 데이터를 {version}(으)로 갱신했습니다.");
        }
        catch (Exception exception)
        {
            Debug.LogError($"정적 데이터 갱신 실패, 가진 데이터를 유지합니다: {exception.Message}");
        }

        return false;
    }

    // 응답에 테이블이 없으면 null. 파싱에 실패하면 테이블 이름을 붙여 예외를 던진다
    private static T ParseResponseTable<T>(JObject tables, string resourcePath, Func<JObject, T> parse) where T : class
    {
        string tableName = Path.GetFileName(resourcePath);
        JToken table = tables[tableName];
        if (table == null)
            return null;

        try
        {
            return parse((JObject)table);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"{tableName}: {exception.Message}", exception);
        }
    }

    // 저장본 버전. 없거나 첫째 자리가 빌드 값과 다르면(앱 업데이트 직후) 저장본을 쓰지 않으므로 null
    private static string LoadUsableSavedVersion()
    {
        string version = SaveLoadHelper.LoadStaticDataVersion();
        return TryGetMajorVersion(version, out int major) && major == GameConstants.Server.STATIC_DATA_MAJOR_VERSION
            ? version
            : null;
    }

    // 버전 문자열의 첫째 자리. 자리는 점으로 나뉜다
    private static bool TryGetMajorVersion(string version, out int major)
    {
        major = 0;
        return !string.IsNullOrEmpty(version) && int.TryParse(version.Split('.')[0], out major);
    }

    #region GetMethod

    public MonsterData GetMonsterDataFromJson(int monsterId)
    {
        if (monsterDataDic == null)
        {
            Debug.LogError("몬스터 데이터가 초기화되지 않았습니다.");
            return null;
        }

        if (monsterDataDic.TryGetValue(monsterId, out MonsterData data))
            return data;

        Debug.LogWarning($"등록되지 않은 몬스터 ID: {monsterId}");
        return null;
    }

    // 공격이 없는 몬스터는 빈 목록(null 체크 불필요)
    public IReadOnlyList<MonsterAttackData> GetMonsterAttacks(int monsterId)
        => monsterAttacksByMonsterId.TryGetValue(monsterId, out var list) ? list : EmptyAttacks;

    // monsterAttackDataDic을 monsterId별로 묶는다. 원본이 바뀌는 모든 곳에서 호출해야 한다
    private void RebuildMonsterAttackIndex()
    {
        var index = new Dictionary<int, List<MonsterAttackData>>();
        if (monsterAttackDataDic != null)
        {
            foreach (MonsterAttackData data in monsterAttackDataDic.Values)
            {
                if (!index.TryGetValue(data.monsterId, out var list))
                {
                    list = new List<MonsterAttackData>();
                    index.Add(data.monsterId, list);
                }
                list.Add(data);
            }
        }
        monsterAttacksByMonsterId = index;
    }

    // 개별 행의 고유 ID로 조회합니다. 실패 시 null을 반환합니다.
    public MonsterAttackData GetMonsterAttackDataFromJson(int monsterAttackId)
    {
        if (monsterAttackDataDic == null)
        {
            Debug.LogError("MonsterAttack 데이터가 초기화되지 않았습니다.");
            return null;
        }

        if (monsterAttackDataDic.TryGetValue(monsterAttackId, out MonsterAttackData data))
            return data;

        Debug.LogWarning($"등록되지 않은 MonsterAttack ID: {monsterAttackId}");
        return null;
    }

    // 개별 행의 고유 ID로 조회합니다. 실패 시 null을 반환합니다.
    public WaveEntryData GetWaveEntryDataFromJson(int waveEntryId)
    {
        if (waveEntryDataDic == null)
        {
            Debug.LogError("Wave 데이터가 초기화되지 않았습니다.");
            return null;
        }

        if (waveEntryDataDic.TryGetValue(waveEntryId, out WaveEntryData data))
            return data;

        Debug.LogWarning($"등록되지 않은 Wave 항목 ID: {waveEntryId}");
        return null;
    }

    // 개별 행의 고유 ID로 조회합니다. 실패 시 null을 반환합니다.
    public SpawnPatternData GetSpawnPatternDataFromJson(int patternId)
    {
        if (spawnPatternDataDic == null)
        {
            Debug.LogError("SpawnPattern 데이터가 초기화되지 않았습니다.");
            return null;
        }

        if (spawnPatternDataDic.TryGetValue(patternId, out SpawnPatternData data))
            return data;

        Debug.LogWarning($"등록되지 않은 SpawnPattern ID: {patternId}");
        return null;
    }

    // 테이블 ID로 조회합니다. 행은 dropGroup 오름차순입니다. 실패 시 null을 반환합니다.
    public IReadOnlyList<DropTableEntryData> GetDropTableFromJson(int dropTableId)
    {
        if (dropTableDic == null)
        {
            Debug.LogError("DropTable 데이터가 초기화되지 않았습니다.");
            return null;
        }

        if (dropTableDic.TryGetValue(dropTableId, out List<DropTableEntryData> entries))
            return entries;

        Debug.LogWarning($"등록되지 않은 DropTable ID: {dropTableId}");
        return null;
    }

    // 개별 행의 고유 ID로 조회합니다. 실패 시 null을 반환합니다.
    public StageData GetStageDataFromJson(int stageId)
    {
        if (stageDataDic == null)
        {
            Debug.LogError("Stage 데이터가 초기화되지 않았습니다.");
            return null;
        }

        if (stageDataDic.TryGetValue(stageId, out StageData data))
            return data;

        Debug.LogWarning($"등록되지 않은 Stage ID: {stageId}");
        return null;
    }

    public SkillData GetSkillDataFromJson(int skillId)
    {
        if (skillDataDic == null)
        {
            Debug.LogError("Skill 데이터가 초기화되지 않았습니다.");
            return null;
        }

        if (skillDataDic.TryGetValue(skillId, out SkillData data))
            return data;

        Debug.LogWarning($"등록되지 않은 Skill ID: {skillId}");
        return null;
    }

    public DropItemData GetDropItemDataFromJson(DropItemType type)
    {
        if (dropItemDataDic == null)
        {
            Debug.LogError("DropItem 데이터가 초기화되지 않았습니다.");
            return null;
        }

        if (dropItemDataDic.TryGetValue(type, out DropItemData data))
            return data;

        Debug.LogWarning($"등록되지 않은 DropItem 종류: {type}");
        return null;
    }
    #endregion

    #region LoadData

    // 서버 테이블 원문. 쓸 수 있는 저장본이 있으면 저장본, 없으면 빌드 사본(Resources)이다. 둘 다 없으면 null
    public static string ReadTableText(string resourcePath)
    {
        if (LoadUsableSavedVersion() != null)
        {
            string saved = SaveLoadHelper.LoadStaticDataTable(Path.GetFileName(resourcePath));
            if (saved != null)
                return saved;
        }

        TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);
        return jsonFile != null ? jsonFile.text : null;
    }

    // 시작할 때 테이블 하나를 읽는다. 실패하면 로그만 남기고 null을 반환한다
    private static T LoadTable<T>(string resourcePath, Func<JObject, T> parse) where T : class
    {
        try
        {
            string text = ReadTableText(resourcePath)
                ?? throw new InvalidOperationException("JSON 파일이 없습니다: " + resourcePath);

            return parse(JObject.Parse(text));
        }
        catch (Exception exception)
        {
            Debug.LogError($"{Path.GetFileName(resourcePath)} 데이터 로드 실패: {exception.Message}");
            return null;
        }
    }

    private static Dictionary<int, MonsterData> ParseMonsterData(JObject root)
    {
        JArray rows = root["datas"] as JArray;

        if (rows == null || rows.Count == 0)
            throw new InvalidOperationException("몬스터 데이터 목록이 비어 있습니다.");

        var loadedDatas = new Dictionary<int, MonsterData>();
        foreach (JToken row in rows)
        {
            if (!(row is JObject))
            {
                throw new InvalidOperationException("몬스터 데이터 항목이 객체 형식이 아닙니다.");
            }

            MonsterData data = row.ToObject<MonsterData>();

            if (data == null || data.monsterId <= 0)
            {
                throw new InvalidOperationException("몬스터 데이터 또는 ID가 올바르지 않습니다.");
            }

            if (loadedDatas.ContainsKey(data.monsterId))
            {
                throw new InvalidOperationException($"중복된 몬스터 ID: {data.monsterId}");
            }

            data.OnLoaded();
            loadedDatas.Add(data.monsterId, data);
        }

        return loadedDatas;
    }

    private static Dictionary<int, MonsterAttackData> ParseMonsterAttackData(JObject root)
    {
        JArray rows = root["datas"] as JArray;
        if (rows == null || rows.Count == 0)
            throw new InvalidOperationException("MonsterAttack 데이터 목록이 비어 있습니다.");

        var loadedDatas = new Dictionary<int, MonsterAttackData>();
        foreach (JToken row in rows)
        {
            if (!(row is JObject))
                throw new InvalidOperationException("MonsterAttack 데이터 항목이 객체 형식이 아닙니다.");

            MonsterAttackData data = row.ToObject<MonsterAttackData>();
            if (data == null || data.monsterAttackId <= 0)
                throw new InvalidOperationException("MonsterAttack 데이터 또는 ID가 올바르지 않습니다.");

            if (loadedDatas.ContainsKey(data.monsterAttackId))
                throw new InvalidOperationException($"중복된 MonsterAttack ID: {data.monsterAttackId}");

            if (!data.OnLoaded())
                throw new InvalidOperationException($"MonsterAttack {data.monsterAttackId}의 monsterAttackType \"{data.monsterAttackType}\"을(를) 알 수 없습니다.");

            loadedDatas.Add(data.monsterAttackId, data);
        }

        return loadedDatas;
    }

    private static Dictionary<int, WaveEntryData> ParseWaveData(JObject root)
    {
        JArray rows = root["datas"] as JArray;
        if (rows == null || rows.Count == 0)
            throw new InvalidOperationException("Wave 데이터 목록이 비어 있습니다.");

        var loadedDatas = new Dictionary<int, WaveEntryData>();
        foreach (JToken row in rows)
        {
            if (!(row is JObject))
                throw new InvalidOperationException("Wave 데이터 항목이 객체 형식이 아닙니다.");

            WaveEntryData data = row.ToObject<WaveEntryData>();
            if (data == null || data.waveEntryId <= 0)
                throw new InvalidOperationException("Wave 데이터 또는 ID가 올바르지 않습니다.");

            if (loadedDatas.ContainsKey(data.waveEntryId))
                throw new InvalidOperationException($"중복된 Wave 항목 ID: {data.waveEntryId}");

            loadedDatas.Add(data.waveEntryId, data);
        }

        return loadedDatas;
    }

    private static Dictionary<int, SpawnPatternData> ParseSpawnPatternData(JObject root)
    {
        JArray rows = root["datas"] as JArray;
        if (rows == null || rows.Count == 0)
            throw new InvalidOperationException("SpawnPattern 데이터 목록이 비어 있습니다.");

        var loadedDatas = new Dictionary<int, SpawnPatternData>();
        foreach (JToken row in rows)
        {
            if (!(row is JObject))
                throw new InvalidOperationException("SpawnPattern 데이터 항목이 객체 형식이 아닙니다.");

            SpawnPatternData data = row.ToObject<SpawnPatternData>();
            if (data == null || data.patternId <= 0)
                throw new InvalidOperationException("SpawnPattern 데이터 또는 ID가 올바르지 않습니다.");

            if (loadedDatas.ContainsKey(data.patternId))
                throw new InvalidOperationException($"중복된 SpawnPattern ID: {data.patternId}");

            if (data.spawnCount < 1)
                throw new InvalidOperationException($"SpawnPattern {data.patternId}의 spawnCount는 1 이상이어야 합니다.");

            if (!data.OnLoaded())
                throw new InvalidOperationException($"SpawnPattern {data.patternId}의 eventType \"{data.eventType}\" 또는 formation \"{data.formation}\"을(를) 알 수 없습니다.");

            loadedDatas.Add(data.patternId, data);
        }

        return loadedDatas;
    }

    private static Dictionary<int, StageData> ParseStageData(JObject root)
    {
        JArray rows = root["datas"] as JArray;
        if (rows == null || rows.Count == 0)
            throw new InvalidOperationException("Stage 데이터 목록이 비어 있습니다.");

        var loadedDatas = new Dictionary<int, StageData>();
        foreach (JToken row in rows)
        {
            if (!(row is JObject))
                throw new InvalidOperationException("Stage 데이터 항목이 객체 형식이 아닙니다.");

            StageData data = row.ToObject<StageData>();
            if (data == null || data.stageId <= 0)
                throw new InvalidOperationException("Stage 데이터 또는 ID가 올바르지 않습니다.");

            if (loadedDatas.ContainsKey(data.stageId))
                throw new InvalidOperationException($"중복된 Stage ID: {data.stageId}");

            data.OnLoaded();
            loadedDatas.Add(data.stageId, data);
        }

        return loadedDatas;
    }

    private static Dictionary<int, SkillData> ParseSkillData(JObject root)
    {
        JArray rows = root["datas"] as JArray;

        if (rows == null || rows.Count == 0)
            throw new InvalidOperationException("Skill 데이터 목록이 비어 있습니다.");

        var loadedDatas = new Dictionary<int, SkillData>();

        foreach (JToken row in rows)
        {
            if (!(row is JObject))
                throw new InvalidOperationException("Skill 데이터 항목이 객체 형식이 아닙니다.");

            SkillData data = row.ToObject<SkillData>();

            if (data == null || data.skillId <= 0)
                throw new InvalidOperationException("Skill 데이터 또는 ID가 올바르지 않습니다.");

            if (loadedDatas.ContainsKey(data.skillId))
                throw new InvalidOperationException($"중복된 Skill ID: {data.skillId}");

            // Newtonsoft 역직렬화 후 Unity 콜백을 직접 호출하여
            // skillType 문자열을 TargetType 열거형으로 변환합니다.
            data.OnAfterDeserialize();

            loadedDatas.Add(data.skillId, data);
        }

        return loadedDatas;
    }

    private static Dictionary<DropItemType, DropItemData> ParseDropItemData(JObject root)
    {
        JArray rows = root["datas"] as JArray;
        if (rows == null || rows.Count == 0)
            throw new InvalidOperationException("DropItem 데이터 목록이 비어 있습니다.");

        var loadedDatas = new Dictionary<DropItemType, DropItemData>();
        var loadedIds = new HashSet<int>();
        foreach (JToken row in rows)
        {
            if (!(row is JObject))
                throw new InvalidOperationException("DropItem 데이터 항목이 객체 형식이 아닙니다.");

            DropItemData data = row.ToObject<DropItemData>();
            if (data == null || data.dropItemId <= 0)
                throw new InvalidOperationException("DropItem 데이터 또는 ID가 올바르지 않습니다.");

            if (!loadedIds.Add(data.dropItemId))
                throw new InvalidOperationException($"중복된 DropItem ID: {data.dropItemId}");

            if (!data.OnLoaded())
                throw new InvalidOperationException($"DropItem {data.dropItemId}의 dropItemType \"{data.dropItemType}\"을(를) 알 수 없습니다.");

            if (loadedDatas.ContainsKey(data.Type))
                throw new InvalidOperationException($"중복된 DropItem 종류: {data.Type}");

            loadedDatas.Add(data.Type, data);
        }

        return loadedDatas;
    }

    private static Dictionary<int, List<DropTableEntryData>> ParseDropTableData(JObject root)
    {
        JArray rows = root["datas"] as JArray;
        if (rows == null || rows.Count == 0)
            throw new InvalidOperationException("DropTable 데이터 목록이 비어 있습니다.");

        var loadedDatas = new Dictionary<int, List<DropTableEntryData>>();
        foreach (JToken row in rows)
        {
            if (!(row is JObject))
                throw new InvalidOperationException("DropTable 데이터 항목이 객체 형식이 아닙니다.");

            DropTableEntryData data = row.ToObject<DropTableEntryData>();
            if (data == null || data.dropTableId <= 0)
                throw new InvalidOperationException("DropTable 데이터 또는 ID가 올바르지 않습니다.");

            if (data.dropGroup < 0)
                throw new InvalidOperationException($"DropTable {data.dropTableId}의 dropGroup은 0 이상이어야 합니다.");

            if (data.weight < 1)
                throw new InvalidOperationException($"DropTable {data.dropTableId}의 weight는 1 이상이어야 합니다.");

            if (data.count < 1)
                throw new InvalidOperationException($"DropTable {data.dropTableId}의 count는 1 이상이어야 합니다.");

            if (!data.OnLoaded())
                throw new InvalidOperationException($"DropTable {data.dropTableId}의 dropItemType \"{data.dropItemType}\"을(를) 알 수 없습니다.");

            if (!loadedDatas.TryGetValue(data.dropTableId, out List<DropTableEntryData> entries))
            {
                entries = new List<DropTableEntryData>();
                loadedDatas.Add(data.dropTableId, entries);
            }
            entries.Add(data);
        }

        foreach (List<DropTableEntryData> entries in loadedDatas.Values)
            entries.Sort((a, b) => a.dropGroup.CompareTo(b.dropGroup));

        return loadedDatas;
    }

    private static AccountConstData ParseAccountConstData(JObject root)
    {
        JArray rows = root["datas"] as JArray;
        if (rows == null || rows.Count != 1)
            throw new InvalidOperationException("AccountConst 데이터는 행이 하나여야 합니다.");

        if (!(rows[0] is JObject))
            throw new InvalidOperationException("AccountConst 데이터 항목이 객체 형식이 아닙니다.");

        return rows[0].ToObject<AccountConstData>();
    }
    #endregion
}
