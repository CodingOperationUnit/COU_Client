using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;


public class JsonDataManager : MonoSingleton<JsonDataManager>
{
    private Dictionary<int, MonsterData> monsterDataDic;

    public Dictionary<int, MonsterData> MonsterDataDic => monsterDataDic;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;

        LoadMonsterData();
    }

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
}
