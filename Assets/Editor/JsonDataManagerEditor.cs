using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(JsonDataManager))]
public class JsonDataManagerEditor : Editor
{
    // 데이터 종류마다 펼침 상태를 분리합니다. 같은 ID가 있어도 서로 영향을 주지 않습니다.
    private readonly HashSet<int> expandedMonsters = new();
    private readonly HashSet<int> expandedMonsterAttacks = new();
    private readonly HashSet<int> expandedWaveEntries = new();
    private readonly HashSet<int> expandedStages = new();
    private readonly HashSet<int> expandedSkills = new();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var manager = (JsonDataManager)target;
        DrawDictionary("몬스터 데이터", manager.MonsterDataDic, expandedMonsters,
            data => data.monsterName, DrawMonster);
        DrawDictionary("몬스터 공격 데이터", manager.MonsterAttackDataDic, expandedMonsterAttacks,
            data => $"몬스터 {data.monsterId} / {data.monsterAttackType}", DrawMonsterAttack);
        DrawDictionary("웨이브 데이터", manager.WaveEntryDataDic, expandedWaveEntries,
            data => $"웨이브 {data.waveId} / 패턴 {data.patternId}", DrawWaveEntry);
        DrawDictionary("스테이지 데이터", manager.StageDataDic, expandedStages,
            data => data.stageName, DrawStage);
        DrawDictionary("스킬 데이터", manager.SkillDataDic, expandedSkills,
            data => data.skillName, DrawSkill);
    }

    private static void DrawDictionary<TKey, T>(string title, IReadOnlyDictionary<TKey, T> datas,
        HashSet<TKey> expandedIds, Func<T, string> getLabel, Action<T> drawData)
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
                expandedIds.Contains(id), $"{id} / {getLabel(data)}", true);

            if (expanded)
                expandedIds.Add(id);
            else
                expandedIds.Remove(id);

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
        EditorGUILayout.IntField("ID", data.monsterId);
        EditorGUILayout.TextField("이름", data.monsterName ?? string.Empty);
        EditorGUILayout.TextField("종류 문자열", data.monsterType ?? string.Empty);
        EditorGUILayout.EnumPopup("종류", data.Type);
        EditorGUILayout.IntField("최대 체력", data.monsterMaxHealthPoint);
        EditorGUILayout.FloatField("이동 속도", data.monsterMoveSpeed);
        EditorGUILayout.IntField("접촉 피해량", data.monsterContactDamage);
        EditorGUILayout.TextField("에셋 경로", data.monsterAsset ?? string.Empty);
    }

    private static void DrawMonsterAttack(MonsterAttackData data)
    {
        EditorGUILayout.IntField("공격 ID", data.monsterAttackId);
        EditorGUILayout.IntField("몬스터 ID", data.monsterId);
        EditorGUILayout.TextField("공격 종류 문자열", data.monsterAttackType ?? string.Empty);
        EditorGUILayout.EnumPopup("공격 종류", data.AttackType);
        EditorGUILayout.FloatField("쿨다운 (초)", data.monsterAttackCooldown);
        EditorGUILayout.FloatField("공격 시작 거리", data.monsterAttackTriggerRange);
        EditorGUILayout.IntField("피해량", data.monsterAttackDamage);
        EditorGUILayout.IntField("발 수 / 장판 수", data.monsterAttackCount);
        EditorGUILayout.FloatField("산탄 각도", data.monsterAttackAngle);
        EditorGUILayout.FloatField("독 장판 유지 시간 (초)", data.monsterAttackDuration);
    }

    private static void DrawWaveEntry(WaveEntryData data)
    {
        EditorGUILayout.IntField("웨이브 항목 ID", data.waveEntryId);
        EditorGUILayout.IntField("웨이브 ID", data.waveId);
        EditorGUILayout.FloatField("시작 시간 (초)", data.patternStartTime);
        EditorGUILayout.IntField("패턴 ID", data.patternId);
        EditorGUILayout.IntField("몬스터 ID", data.monsterId);
    }

    private static void DrawStage(StageData data)
    {
        EditorGUILayout.IntField("스테이지 ID", data.stageId);
        EditorGUILayout.TextField("이름", data.stageName ?? string.Empty);
        EditorGUILayout.FloatField("진행 시간 (초)", data.stageDuration);
        EditorGUILayout.IntField("웨이브 ID", data.waveId);
        EditorGUILayout.TextField("설명", data.stageDescription ?? string.Empty);
        EditorGUILayout.TextField("UI 색깔 문자열", data.stageIllustrationColor ?? string.Empty);
        EditorGUILayout.ColorField("UI 색깔 (파싱됨)", data.IllustrationColor);
    }

    private static void DrawSkill(SkillData data)
    {
        EditorGUILayout.IntField("스킬 ID", data.skillId);
        EditorGUILayout.TextField("이름", data.skillName ?? string.Empty);
        EditorGUILayout.EnumPopup("카테고리", data.skillCategory);
        EditorGUILayout.TextField("설명", data.skillDescription ?? string.Empty);
        EditorGUILayout.TextField("타겟 종류 문자열", data.skillType ?? string.Empty);
        EditorGUILayout.EnumPopup("타겟 종류", data.TargetType);
        EditorGUILayout.FloatField("쿨다운 (초)", data.skillCooldown);
        EditorGUILayout.FloatField("속도", data.skillSpeed);
        EditorGUILayout.FloatField("피해량", data.skillDamage);
        EditorGUILayout.FloatField("사거리", data.skillRange);
        EditorGUILayout.TextField("레벨1 설명", data.level1SkillDescription ?? string.Empty);
        EditorGUILayout.TextField("레벨2 설명", data.level2SkillDescription ?? string.Empty);
        EditorGUILayout.TextField("레벨3 설명", data.level3SkillDescription ?? string.Empty);
        EditorGUILayout.TextField("레벨4 설명", data.level4SkillDescription ?? string.Empty);
        EditorGUILayout.TextField("레벨5 설명", data.level5SkillDescription ?? string.Empty);
    }

    public override bool RequiresConstantRepaint()
    {
        return EditorApplication.isPlaying;
    }
}
