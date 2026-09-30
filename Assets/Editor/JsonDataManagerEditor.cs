using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(JsonDataManager))]
public class JsonDataManagerEditor : Editor
{
    // 몬스터별 펼침 상태
    private readonly HashSet<int> expandedIDs = new();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(
            "몬스터 데이터",
            EditorStyles.boldLabel);

        var manager = (JsonDataManager)target;
        var datas = manager.MonsterDataDic;

        if (datas == null)
        {
            EditorGUILayout.HelpBox(
                "아직 로드된 몬스터 데이터가 없습니다.",
                MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField("등록 수", datas.Count.ToString());

        foreach (var pair in datas)
        {
            MonsterData data = pair.Value;

            if (data == null)
            {
                EditorGUILayout.LabelField($"{pair.Key}: 데이터 없음");
                continue;
            }

            bool expanded = EditorGUILayout.Foldout(
                expandedIDs.Contains(pair.Key),
                $"{pair.Key} / {data.monsterName}",
                true);

            if (expanded)
                expandedIDs.Add(pair.Key);
            else
                expandedIDs.Remove(pair.Key);

            if (!expanded)
                continue;

            EditorGUI.indentLevel++;

            // 값을 표시만 하고 변경하지 않습니다.
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("ID", data.monsterID);

                EditorGUILayout.TextField(
                    "이름", data.monsterName ?? string.Empty);

                EditorGUILayout.TextField(
                    "종류 문자열", data.type ?? string.Empty);

                EditorGUILayout.EnumPopup("종류", data.Type);

                EditorGUILayout.IntField(
                    "최대 체력", data.monsterMaxHealthPoint);

                EditorGUILayout.IntField(
                    "경험치", data.monsterExp);

                EditorGUILayout.FloatField(
                    "이동 속도", data.monsterMoveSpeed);

                EditorGUILayout.IntField(
                    "공격력", data.monsterAttackPoint);

                EditorGUILayout.TextField(
                    "에셋 경로", data.monsterAsset ?? string.Empty);
            }

            EditorGUI.indentLevel--;
        }
    }

    public override bool RequiresConstantRepaint()
    {
        return EditorApplication.isPlaying;
    }
}