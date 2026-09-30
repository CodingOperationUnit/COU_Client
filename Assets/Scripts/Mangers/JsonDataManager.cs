using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;


public class JsonDataManager : MonoSingleton<JsonDataManager>
{
    private Dictionary<int, MonsterData> monsterDataDic;
    public Dictionary<int, MonsterData> MonsterDataDic => monsterDataDic;

    private Dictionary<int, BossAttackData> bossAttackDataDic;
    public IReadOnlyDictionary<int, BossAttackData> BossAttackDataDic => bossAttackDataDic;

    private Dictionary<int, SpawnEventData> spawnEventDataDic;
    public IReadOnlyDictionary<int, SpawnEventData> SpawnEventDataDic => spawnEventDataDic;

    private Dictionary<int, StageData> stageDataDic;
    public IReadOnlyDictionary<int, StageData> StageDataDic => stageDataDic;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;

        LoadMonsterData();
        LoadBossAttackData();
        LoadSpawnData();
        LoadStageData();

    }

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
    public SpawnEventData GetSpawnEventDataFromJson(int spawnEventID)
    {
        if (spawnEventDataDic == null)
        {
            Debug.LogError("Spawn 데이터가 초기화되지 않았습니다.");
            return null;
        }

        if (spawnEventDataDic.TryGetValue(spawnEventID, out SpawnEventData data))
            return data;

        Debug.LogWarning($"등록되지 않은 Spawn ID: {spawnEventID}");
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
    
    private void LoadSpawnData()
    {
        if (spawnEventDataDic != null) return;

        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(GameConstants.Paths.SpawnData_Json_Path);
            if (jsonFile == null)
                throw new InvalidOperationException("JSON 파일이 없습니다: " + GameConstants.Paths.SpawnData_Json_Path);

            JObject root = JObject.Parse(jsonFile.text);
            JArray rows = root["datas"] as JArray;
            if (rows == null || rows.Count == 0)
                throw new InvalidOperationException("Spawn 데이터 목록이 비어 있습니다.");

            var loadedDatas = new Dictionary<int, SpawnEventData>();
            foreach (JToken row in rows)
            {
                if (!(row is JObject))
                    throw new InvalidOperationException("Spawn 데이터 항목이 객체 형식이 아닙니다.");

                SpawnEventData data = row.ToObject<SpawnEventData>();
                if (data == null || data.spawnEventID <= 0)
                    throw new InvalidOperationException("Spawn 데이터 또는 ID가 올바르지 않습니다.");

                if (loadedDatas.ContainsKey(data.spawnEventID))
                    throw new InvalidOperationException($"중복된 Spawn ID: {data.spawnEventID}");

                data.OnLoaded();
                loadedDatas.Add(data.spawnEventID, data);
            }

            spawnEventDataDic = loadedDatas;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Spawn 데이터 로드 실패: {exception.Message}");
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

                loadedDatas.Add(data.stageID, data);
            }

            stageDataDic = loadedDatas;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Stage 데이터 로드 실패: {exception.Message}");
        }
    }

    #endregion
}
