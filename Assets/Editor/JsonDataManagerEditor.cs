using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(JsonDataManager))]
public class JsonDataManagerEditor : Editor
{
    // 데이터 종류마다 펼침 상태를 분리합니다. 같은 ID가 있어도 서로 영향을 주지 않습니다.
    private readonly HashSet<int> expandedMonsters = new();
    private readonly HashSet<int> expandedBossAttacks = new();
    private readonly HashSet<int> expandedSpawnEvents = new();
    private readonly HashSet<int> expandedStages = new();
    private readonly HashSet<int> expandedSkills = new();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var manager = (JsonDataManager)target;
        DrawDictionary("몬스터 데이터", manager.MonsterDataDic, expandedMonsters,
            data => data.monsterName, DrawMonster);
        DrawDictionary("보스 공격 데이터", manager.BossAttackDataDic, expandedBossAttacks,
            data => $"몬스터 {data.monsterID} / {data.attackType}", DrawBossAttack);
        DrawDictionary("스폰 이벤트 데이터", manager.SpawnEventDataDic, expandedSpawnEvents,
            data => $"스테이지 {data.stageID} / {data.eventType}", DrawSpawnEvent);
        DrawDictionary("스테이지 데이터", manager.StageDataDic, expandedStages,
            data => data.stageName, DrawStage);
        DrawDictionary("스킬 데이터", manager.SkillDataDic, expandedSkills,
            data => data.Name, DrawSkill);
    }

    private static void DrawDictionary<TKey, T>(string title, IReadOnlyDictionary<TKey, T> datas,
        HashSet<TKey> expandedIDs, Func<T, string> getLabel, Action<T> drawData)
        where TKey : IComparable<TKey>
        where T : class
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

        if (datas == null)
        {
            EditorGUILayout.HelpBox($"아직 로드된 {title}가 없습니다.", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField("등록 수", datas.Count.ToString());

        var ids = new List<TKey>(datas.Keys);
        ids.Sort();

        foreach (TKey id in ids)
        {
            T data = datas[id];
            if (data == null)
            {
                EditorGUILayout.LabelField($"{id}: 데이터 없음");
                continue;
            }

            bool expanded = EditorGUILayout.Foldout(
                expandedIDs.Contains(id), $"{id} / {getLabel(data)}", true);

            if (expanded)
                expandedIDs.Add(id);
            else
                expandedIDs.Remove(id);

            if (!expanded)
                continue;

            using (new EditorGUI.IndentLevelScope())
            using (new EditorGUI.DisabledScope(true))
            {
                // 표시만 하고 반환값을 원본 데이터에 대입하지 않습니다.
                drawData(data);
            }
        }
    }

    private static void DrawMonster(MonsterData data)
    {
        EditorGUILayout.IntField("ID", data.monsterID);
        EditorGUILayout.TextField("이름", data.monsterName ?? string.Empty);
        EditorGUILayout.TextField("종류 문자열", data.type ?? string.Empty);
        EditorGUILayout.EnumPopup("종류", data.Type);
        EditorGUILayout.IntField("최대 체력", data.monsterMaxHealthPoint);
        EditorGUILayout.IntField("경험치", data.monsterExp);
        EditorGUILayout.FloatField("이동 속도", data.monsterMoveSpeed);
        EditorGUILayout.IntField("공격력", data.monsterAttackPoint);
        EditorGUILayout.TextField("에셋 경로", data.monsterAsset ?? string.Empty);
    }

    private static void DrawBossAttack(BossAttackData data)
    {
        EditorGUILayout.IntField("공격 ID", data.bossAttackID);
        EditorGUILayout.IntField("몬스터 ID", data.monsterID);
        EditorGUILayout.TextField("공격 종류 문자열", data.attackType ?? string.Empty);
        EditorGUILayout.EnumPopup("공격 종류", data.AttackType);
        EditorGUILayout.FloatField("쿨다운 (초)", data.cooldown);
        EditorGUILayout.FloatField("사거리", data.range);
        EditorGUILayout.IntField("피해량", data.damage);
    }

    private static void DrawSpawnEvent(SpawnEventData data)
    {
        EditorGUILayout.IntField("스폰 이벤트 ID", data.spawnEventID);
        EditorGUILayout.IntField("스테이지 ID", data.stageID);
        EditorGUILayout.TextField("이벤트 종류 문자열", data.eventType ?? string.Empty);
        EditorGUILayout.EnumPopup("이벤트 종류", data.EventType);
        EditorGUILayout.FloatField("시작 시간 (초)", data.startTime);
        EditorGUILayout.FloatField("종료 시간 (초)", data.endTime);
        EditorGUILayout.IntField("몬스터 ID", data.monsterID);
        EditorGUILayout.FloatField("스폰 간격 (초)", data.spawnInterval);
        EditorGUILayout.IntField("스폰 수", data.spawnCount);
        EditorGUILayout.Toggle("반복", data.repeat);
    }

    private static void DrawStage(StageData data)
    {
        EditorGUILayout.IntField("스테이지 ID", data.stageID);
        EditorGUILayout.TextField("이름", data.stageName ?? string.Empty);
        EditorGUILayout.FloatField("진행 시간 (초)", data.duration);
    }

    private static void DrawSkill(SkillData data)
    {
        EditorGUILayout.IntField("스킬 ID", data.ID);
        EditorGUILayout.TextField("이름", data.Name ?? string.Empty);
        EditorGUILayout.TextField("설명", data.Description ?? string.Empty);
        EditorGUILayout.TextField("타겟 종류 문자열", data.Type ?? string.Empty);
        EditorGUILayout.EnumPopup("타겟 종류", data.TargetType);
        EditorGUILayout.FloatField("쿨다운 (초)", data.Cooldown);
        EditorGUILayout.FloatField("속도", data.Speed);
        EditorGUILayout.FloatField("피해량", data.Damage);
        EditorGUILayout.FloatField("사거리", data.Range);
    }

    public override bool RequiresConstantRepaint()
    {
        return EditorApplication.isPlaying;
    }
}
