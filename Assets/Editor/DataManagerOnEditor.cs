using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataManager))]
public class DataManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // isDontDestroy 등 기존 Inspector 항목 표시
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(
            "현재 플레이어 데이터",
            EditorStyles.boldLabel);

        var manager = (DataManager)target;
        var data = manager.currentData;

        if (data == null)
        {
            EditorGUILayout.HelpBox(
                "로드된 플레이어 데이터가 없습니다.",
                MessageType.Info);
            return;
        }

        // 표시만 하고 값을 변경하거나 대입하지 않음
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.TextField(
                "Player ID", data.playerID ?? string.Empty);

            EditorGUILayout.IntField("Gold", data.gold);
            EditorGUILayout.IntField("Exp", data.exp);
        }
    }

    // 플레이 모드에서 Inspector를 지속적으로 갱신
    public override bool RequiresConstantRepaint()
    {
        return EditorApplication.isPlaying;
    }
}