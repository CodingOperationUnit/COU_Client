using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;


public class JsonDataManager : MonoSingleton<JsonDataManager>
{
    #region Fields

    private Dictionary<int, MonsterData> monsterDataDic;
    public Dictionary<int, MonsterData> MonsterDataDic => monsterDataDic;

    private Dictionary<int, BossAttackData> bossAttackDataDic;
    public IReadOnlyDictionary<int, BossAttackData> BossAttackDataDic => bossAttackDataDic;

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

        LoadMonsterData();
        LoadBossAttackData();
        LoadWaveData();
        LoadSpawnPatternData();
        LoadStageData();
        LoadSkillData();
        LoadDropItemData();
        LoadDropTableData();
        LoadAccountConstData();
    }

    #region GetMethod

    public MonsterData GetMonsterDataFromJson(int monsterID)
    {
        if (monsterDataDic == null)
        {
            Debug.LogError("몬스터 데이터가 초기화되지 않았습니다.");
            return null;
        }

        if (monsterDataDic.TryGetValue(monsterID, out MonsterData data))
            return data;
        
        Debug.LogWarning($"등록되지 않은 몬스터 ID: {monsterID}");
        return null;
    }

    // 개별 행의 고유 ID로 조회합니다. 실패 시 null을 반환합니다.
    public BossAttackData GetBossAttackDataFromJson(int bossAttackID)
    {
        if (bossAttackDataDic == null)
        {
            Debug.LogError("BossAttack 데이터가 초기화되지 않았습니다.");
            return null;
        }

        if (bossAttackDataDic.TryGetValue(bossAttackID, out BossAttackData data))
            return data;

        Debug.LogWarning($"등록되지 않은 BossAttack ID: {bossAttackID}");
        return null;
    }

    // 개별 행의 고유 ID로 조회합니다. 실패 시 null을 반환합니다.
    public WaveEntryData GetWaveEntryDataFromJson(int waveEntryID)
    {
        if (waveEntryDataDic == null)
        {
            Debug.LogError("Wave 데이터가 초기화되지 않았습니다.");
            return null;
        }

        if (waveEntryDataDic.TryGetValue(waveEntryID, out WaveEntryData data))
            return data;

        Debug.LogWarning($"등록되지 않은 Wave 항목 ID: {waveEntryID}");
        return null;
    }

    // 개별 행의 고유 ID로 조회합니다. 실패 시 null을 반환합니다.
    public SpawnPatternData GetSpawnPatternDataFromJson(int patternID)
    {
        if (spawnPatternDataDic == null)
        {
            Debug.LogError("SpawnPattern 데이터가 초기화되지 않았습니다.");
            return null;
        }

        if (spawnPatternDataDic.TryGetValue(patternID, out SpawnPatternData data))
            return data;

        Debug.LogWarning($"등록되지 않은 SpawnPattern ID: {patternID}");
        return null;
    }

    // 테이블 ID로 조회합니다. 행은 group 오름차순입니다. 실패 시 null을 반환합니다.
    public IReadOnlyList<DropTableEntryData> GetDropTableFromJson(int dropTableID)
    {
        if (dropTableDic == null)
        {
            Debug.LogError("DropTable 데이터가 초기화되지 않았습니다.");
            return null;
        }

        if (dropTableDic.TryGetValue(dropTableID, out List<DropTableEntryData> entries))
            return entries;

        Debug.LogWarning($"등록되지 않은 DropTable ID: {dropTableID}");
        return null;
    }

    // 개별 행의 고유 ID로 조회합니다. 실패 시 null을 반환합니다.
    public StageData GetStageDataFromJson(int stageID)
    {
        if (stageDataDic == null)
        {
            Debug.LogError("Stage 데이터가 초기화되지 않았습니다.");
            return null;
        }

        if (stageDataDic.TryGetValue(stageID, out StageData data))
            return data;

        Debug.LogWarning($"등록되지 않은 Stage ID: {stageID}");
        return null;
    }
    
    public SkillData GetSkillDataFromJson(int skillID)
    {
        if (skillDataDic == null)
        {
            Debug.LogError("Skill 데이터가 초기화되지 않았습니다.");
            return null;
        }

        if (skillDataDic.TryGetValue(skillID, out SkillData data))
            return data;

        Debug.LogWarning($"등록되지 않은 Skill ID: {skillID}");
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
    
    private void LoadMonsterData()
    {
        if (monsterDataDic != null) 
            return;

        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(GameConstants.Paths.MonsterData_Json_Path);

            if (jsonFile == null)
            {
                Debug.LogError($"몬스터 JSON 파일이 없습니다: {GameConstants.Paths.MonsterData_Json_Path}");
                return;
            }

            JObject root = JObject.Parse(jsonFile.text);
            JArray rows = root["datas"] as JArray;

            if (rows == null || rows.Count == 0)
            {
                Debug.LogError("몬스터 데이터 목록이 비어 있습니다.");
                return;
            }

            var loadedDatas = new Dictionary<int, MonsterData>();
            foreach (JToken row in rows)
            {
                if (!(row is JObject))
                {
                    throw new InvalidOperationException("몬스터 데이터 항목이 객체 형식이 아닙니다.");
                }

                MonsterData data = row.ToObject<MonsterData>();

                if (data == null || data.monsterID <= 0)
                {
                    throw new InvalidOperationException("몬스터 데이터 또는 ID가 올바르지 않습니다.");
                }

                if (loadedDatas.ContainsKey(data.monsterID))
                {
                    throw new InvalidOperationException($"중복된 몬스터 ID: {data.monsterID}");
                }

                data.OnLoaded();
                loadedDatas.Add(data.monsterID, data);
            }

            // 전체 로드가 성공했을 때만 반영
            monsterDataDic = loadedDatas;
        }
        catch (Exception exception)
        {
            Debug.LogError($"몬스터 데이터 로드 실패: {exception.Message}");
        }
    }

    private void LoadBossAttackData()
    {
        if (bossAttackDataDic != null) return;

        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(GameConstants.Paths.BossAttackData_Json_Path);
            if (jsonFile == null)
                throw new InvalidOperationException("JSON 파일이 없습니다: " + GameConstants.Paths.BossAttackData_Json_Path);

            JObject root = JObject.Parse(jsonFile.text);
            JArray rows = root["datas"] as JArray;
            if (rows == null || rows.Count == 0) 
                throw new InvalidOperationException("BossAttack 데이터 목록이 비어 있습니다.");

            var loadedDatas = new Dictionary<int, BossAttackData>();
            foreach (JToken row in rows)
            {
                if (!(row is JObject))
                    throw new InvalidOperationException("BossAttack 데이터 항목이 객체 형식이 아닙니다.");

                BossAttackData data = row.ToObject<BossAttackData>();
                if (data == null || data.bossAttackID <= 0)
                    throw new InvalidOperationException("BossAttack 데이터 또는 ID가 올바르지 않습니다.");

                if (loadedDatas.ContainsKey(data.bossAttackID))
                    throw new InvalidOperationException($"중복된 BossAttack ID: {data.bossAttackID}");

                data.OnLoaded();
                loadedDatas.Add(data.bossAttackID, data);
            }

            bossAttackDataDic = loadedDatas;
        }
        catch (Exception exception)
        {
            Debug.LogError($"BossAttack 데이터 로드 실패: {exception.Message}");
        }
    }
    
    private void LoadWaveData()
    {
        if (waveEntryDataDic != null) return;

        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(GameConstants.Paths.WaveData_Json_Path);
            if (jsonFile == null)
                throw new InvalidOperationException("JSON 파일이 없습니다: " + GameConstants.Paths.WaveData_Json_Path);

            JObject root = JObject.Parse(jsonFile.text);
            JArray rows = root["datas"] as JArray;
            if (rows == null || rows.Count == 0)
                throw new InvalidOperationException("Wave 데이터 목록이 비어 있습니다.");

            var loadedDatas = new Dictionary<int, WaveEntryData>();
            foreach (JToken row in rows)
            {
                if (!(row is JObject))
                    throw new InvalidOperationException("Wave 데이터 항목이 객체 형식이 아닙니다.");

                WaveEntryData data = row.ToObject<WaveEntryData>();
                if (data == null || data.waveEntryID <= 0)
                    throw new InvalidOperationException("Wave 데이터 또는 ID가 올바르지 않습니다.");

                if (loadedDatas.ContainsKey(data.waveEntryID))
                    throw new InvalidOperationException($"중복된 Wave 항목 ID: {data.waveEntryID}");

                loadedDatas.Add(data.waveEntryID, data);
            }

            waveEntryDataDic = loadedDatas;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Wave 데이터 로드 실패: {exception.Message}");
        }
    }

    private void LoadSpawnPatternData()
    {
        if (spawnPatternDataDic != null) return;

        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(GameConstants.Paths.SpawnPatternData_Json_Path);
            if (jsonFile == null)
                throw new InvalidOperationException("JSON 파일이 없습니다: " + GameConstants.Paths.SpawnPatternData_Json_Path);

            JObject root = JObject.Parse(jsonFile.text);
            JArray rows = root["datas"] as JArray;
            if (rows == null || rows.Count == 0)
                throw new InvalidOperationException("SpawnPattern 데이터 목록이 비어 있습니다.");

            var loadedDatas = new Dictionary<int, SpawnPatternData>();
            foreach (JToken row in rows)
            {
                if (!(row is JObject))
                    throw new InvalidOperationException("SpawnPattern 데이터 항목이 객체 형식이 아닙니다.");

                SpawnPatternData data = row.ToObject<SpawnPatternData>();
                if (data == null || data.patternID <= 0)
                    throw new InvalidOperationException("SpawnPattern 데이터 또는 ID가 올바르지 않습니다.");

                if (loadedDatas.ContainsKey(data.patternID))
                    throw new InvalidOperationException($"중복된 SpawnPattern ID: {data.patternID}");

                if (data.spawnCount < 1)
                    throw new InvalidOperationException($"SpawnPattern {data.patternID}의 spawnCount는 1 이상이어야 합니다.");

                if (!data.OnLoaded())
                    throw new InvalidOperationException($"SpawnPattern {data.patternID}의 eventType \"{data.eventType}\" 또는 formation \"{data.formation}\"을(를) 알 수 없습니다.");

                loadedDatas.Add(data.patternID, data);
            }

            spawnPatternDataDic = loadedDatas;
        }
        catch (Exception exception)
        {
            Debug.LogError($"SpawnPattern 데이터 로드 실패: {exception.Message}");
        }
    }
    
    private void LoadStageData()
    {
        if (stageDataDic != null) return;

        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(GameConstants.Paths.StageData_Json_Path);
            if (jsonFile == null)
                throw new InvalidOperationException("JSON 파일이 없습니다: " + GameConstants.Paths.StageData_Json_Path);

            JObject root = JObject.Parse(jsonFile.text);
            JArray rows = root["datas"] as JArray;
            if (rows == null || rows.Count == 0)
                throw new InvalidOperationException("Stage 데이터 목록이 비어 있습니다.");

            var loadedDatas = new Dictionary<int, StageData>();
            foreach (JToken row in rows)
            {
                if (!(row is JObject))
                    throw new InvalidOperationException("Stage 데이터 항목이 객체 형식이 아닙니다.");

                StageData data = row.ToObject<StageData>();
                if (data == null || data.stageID <= 0)
                    throw new InvalidOperationException("Stage 데이터 또는 ID가 올바르지 않습니다.");

                if (loadedDatas.ContainsKey(data.stageID))
                    throw new InvalidOperationException($"중복된 Stage ID: {data.stageID}");
                
                data.OnLoaded();
                loadedDatas.Add(data.stageID, data);
            }

            stageDataDic = loadedDatas;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Stage 데이터 로드 실패: {exception.Message}");
        }
    }
    
    private void LoadSkillData()
    {
        if (skillDataDic != null) return;
    
        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(GameConstants.Paths.SkillData_Json_Path);
            if (jsonFile == null)
                throw new InvalidOperationException($"JSON 파일이 없습니다: {GameConstants.Paths.SkillData_Json_Path}");
    
            JObject root = JObject.Parse(jsonFile.text);
            JArray rows = root["datas"] as JArray;
    
            if (rows == null || rows.Count == 0)
                throw new InvalidOperationException("Skill 데이터 목록이 비어 있습니다.");
    
            var loadedDatas = new Dictionary<int, SkillData>();
    
            foreach (JToken row in rows)
            {
                if (!(row is JObject))
                    throw new InvalidOperationException("Skill 데이터 항목이 객체 형식이 아닙니다.");
    
                SkillData data = row.ToObject<SkillData>();
    
                if (data == null || data.ID <= 0)
                    throw new InvalidOperationException("Skill 데이터 또는 ID가 올바르지 않습니다.");
    
                if (loadedDatas.ContainsKey(data.ID))
                    throw new InvalidOperationException($"중복된 Skill ID: {data.ID}");
    
                // Newtonsoft 역직렬화 후 Unity 콜백을 직접 호출하여
                // Type 문자열을 TargetType 열거형으로 변환합니다.
                data.OnAfterDeserialize();
    
                loadedDatas.Add(data.ID, data);
            }
    
            skillDataDic = loadedDatas;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Skill 데이터 로드 실패: {exception.Message}");
        }
    }

    private void LoadDropItemData()
    {
        if (dropItemDataDic != null) return;

        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(GameConstants.Paths.DropItemData_Json_Path);
            if (jsonFile == null)
                throw new InvalidOperationException("JSON 파일이 없습니다: " + GameConstants.Paths.DropItemData_Json_Path);

            JObject root = JObject.Parse(jsonFile.text);
            JArray rows = root["datas"] as JArray;
            if (rows == null || rows.Count == 0)
                throw new InvalidOperationException("DropItem 데이터 목록이 비어 있습니다.");

            var loadedDatas = new Dictionary<DropItemType, DropItemData>();
            var loadedIDs = new HashSet<int>();
            foreach (JToken row in rows)
            {
                if (!(row is JObject))
                    throw new InvalidOperationException("DropItem 데이터 항목이 객체 형식이 아닙니다.");

                DropItemData data = row.ToObject<DropItemData>();
                if (data == null || data.dropItemID <= 0)
                    throw new InvalidOperationException("DropItem 데이터 또는 ID가 올바르지 않습니다.");

                if (!loadedIDs.Add(data.dropItemID))
                    throw new InvalidOperationException($"중복된 DropItem ID: {data.dropItemID}");

                if (!data.OnLoaded())
                    throw new InvalidOperationException($"DropItem {data.dropItemID}의 dropItemType \"{data.dropItemType}\"을(를) 알 수 없습니다.");

                if (loadedDatas.ContainsKey(data.Type))
                    throw new InvalidOperationException($"중복된 DropItem 종류: {data.Type}");

                loadedDatas.Add(data.Type, data);
            }

            dropItemDataDic = loadedDatas;
        }
        catch (Exception exception)
        {
            Debug.LogError($"DropItem 데이터 로드 실패: {exception.Message}");
        }
    }

    private void LoadDropTableData()
    {
        if (dropTableDic != null) return;

        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(GameConstants.Paths.DropTableData_Json_Path);
            if (jsonFile == null)
                throw new InvalidOperationException("JSON 파일이 없습니다: " + GameConstants.Paths.DropTableData_Json_Path);

            JObject root = JObject.Parse(jsonFile.text);
            JArray rows = root["datas"] as JArray;
            if (rows == null || rows.Count == 0)
                throw new InvalidOperationException("DropTable 데이터 목록이 비어 있습니다.");

            var loadedDatas = new Dictionary<int, List<DropTableEntryData>>();
            foreach (JToken row in rows)
            {
                if (!(row is JObject))
                    throw new InvalidOperationException("DropTable 데이터 항목이 객체 형식이 아닙니다.");

                DropTableEntryData data = row.ToObject<DropTableEntryData>();
                if (data == null || data.dropTableID <= 0)
                    throw new InvalidOperationException("DropTable 데이터 또는 ID가 올바르지 않습니다.");

                if (data.group < 0)
                    throw new InvalidOperationException($"DropTable {data.dropTableID}의 group은 0 이상이어야 합니다.");

                if (data.weight < 1)
                    throw new InvalidOperationException($"DropTable {data.dropTableID}의 weight는 1 이상이어야 합니다.");

                if (data.count < 1)
                    throw new InvalidOperationException($"DropTable {data.dropTableID}의 count는 1 이상이어야 합니다.");

                if (!data.OnLoaded())
                    throw new InvalidOperationException($"DropTable {data.dropTableID}의 dropItemType \"{data.dropItemType}\"을(를) 알 수 없습니다.");

                if (!loadedDatas.TryGetValue(data.dropTableID, out List<DropTableEntryData> entries))
                {
                    entries = new List<DropTableEntryData>();
                    loadedDatas.Add(data.dropTableID, entries);
                }
                entries.Add(data);
            }

            foreach (List<DropTableEntryData> entries in loadedDatas.Values)
                entries.Sort((a, b) => a.group.CompareTo(b.group));

            dropTableDic = loadedDatas;
        }
        catch (Exception exception)
        {
            Debug.LogError($"DropTable 데이터 로드 실패: {exception.Message}");
        }
    }

    private void LoadAccountConstData()
    {
        if (accountConstData != null) return;

        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(GameConstants.Paths.AccountConstData_Json_Path);
            if (jsonFile == null)
                throw new InvalidOperationException("JSON 파일이 없습니다: " + GameConstants.Paths.AccountConstData_Json_Path);

            JObject root = JObject.Parse(jsonFile.text);
            JArray rows = root["datas"] as JArray;
            if (rows == null || rows.Count != 1)
                throw new InvalidOperationException("AccountConst 데이터는 행이 하나여야 합니다.");

            if (!(rows[0] is JObject))
                throw new InvalidOperationException("AccountConst 데이터 항목이 객체 형식이 아닙니다.");

            accountConstData = rows[0].ToObject<AccountConstData>();
        }
        catch (Exception exception)
        {
            Debug.LogError($"AccountConst 데이터 로드 실패: {exception.Message}");
        }
    }
    #endregion
}
