using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerDataManager))]
public class PlayerDataManagerEditor : Editor
{
    private bool showEquipment = true;
    private bool showStageRecords = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("현재 플레이어 데이터", EditorStyles.boldLabel);

        var manager = (PlayerDataManager)target;
        var data = manager.currentData;

        if (data == null)
        {
            EditorGUILayout.HelpBox("로드된 플레이어 데이터가 없습니다.", MessageType.Info);
            return;
        }

        // 조회만 수행합니다. 원본 데이터와 리스트를 생성하거나 수정하지 않습니다.
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.TextField("플레이어 ID", data.playerId ?? string.Empty);
            EditorGUILayout.IntField("골드", data.gold);
            EditorGUILayout.IntField("보석", data.gem);
            EditorGUILayout.IntField("현재 스태미나", data.currentStamina);
            EditorGUILayout.IntField("최대 스태미나", data.maxStamina);
            EditorGUILayout.IntField("계정 레벨", data.accountLevel);
            EditorGUILayout.IntField("계정 경험치", data.accountExp);
        }

        DrawEquipment(data);
        DrawStageRecords(data);
    }

    private void DrawEquipment(PlayerSaveData data)
    {
        EditorGUILayout.Space();
        showEquipment = EditorGUILayout.Foldout(showEquipment,
            $"장비 목록 ({data.equipmentList?.Count ?? 0})", true);

        if (!showEquipment)
            return;

        if (data.equipmentList == null)
        {
            EditorGUILayout.HelpBox("장비 목록이 null입니다.", MessageType.Info);
            return;
        }

        if (data.equipmentList.Count == 0)
        {
            EditorGUILayout.HelpBox("보유 장비가 없습니다.", MessageType.Info);
            return;
        }

        using (new EditorGUI.IndentLevelScope())
        {
            for (int i = 0; i < data.equipmentList.Count; i++)
            {
                var equipment = data.equipmentList[i];
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField($"장비 {i + 1}", EditorStyles.boldLabel);
                    if (equipment == null)
                    {
                        EditorGUILayout.HelpBox("장비 데이터가 null입니다.", MessageType.Warning);
                        continue;
                    }

                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.TextField("고유 ID", equipment.instanceId ?? string.Empty);
                        EditorGUILayout.LongField("종류 ID", equipment.itemId);
                        EditorGUILayout.IntField("강화 레벨", equipment.level);
                        EditorGUILayout.Toggle("장착 여부", equipment.isEquipped);
                    }
                }
            }
        }
    }

    private void DrawStageRecords(PlayerSaveData data)
    {
        EditorGUILayout.Space();
        showStageRecords = EditorGUILayout.Foldout(showStageRecords,
            $"스테이지 기록 ({data.stageRecordList?.Count ?? 0})", true);

        if (!showStageRecords)
            return;

        if (data.stageRecordList == null)
        {
            EditorGUILayout.HelpBox("스테이지 기록 목록이 null입니다.", MessageType.Info);
            return;
        }

        if (data.stageRecordList.Count == 0)
        {
            EditorGUILayout.HelpBox("스테이지 기록이 없습니다.", MessageType.Info);
            return;
        }

        using (new EditorGUI.IndentLevelScope())
        {
            for (int i = 0; i < data.stageRecordList.Count; i++)
            {
                var record = data.stageRecordList[i];
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField($"기록 {i + 1}", EditorStyles.boldLabel);
                    if (record == null)
                    {
                        EditorGUILayout.HelpBox("스테이지 기록이 null입니다.", MessageType.Warning);
                        continue;
                    }

                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.IntField("스테이지 ID", record.stageId);
                        EditorGUILayout.Toggle("클리어 여부", record.isCleared);
                        EditorGUILayout.FloatField("최장 생존시간 (초)", record.bestSurvivalSeconds);
                    }
                }
            }
        }
    }

    public override bool RequiresConstantRepaint()
    {
        return EditorApplication.isPlaying;
    }
}
