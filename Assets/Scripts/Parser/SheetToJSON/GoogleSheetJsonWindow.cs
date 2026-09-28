#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace COU.EditorTools.SheetToJSON
{
    public sealed class GoogleSheetJsonWindow : EditorWindow
    {
        [SerializeField] private string spreadsheetUrl = "";
        [SerializeField] private string apiUrl = "";
        [SerializeField] private string outputFolder = "Assets/Resources/JsonFiles";
        [SerializeField] private string fileName = "Sheet.json";
        private List<SheetInfo> sheets = new List<SheetInfo>();
        private int selected;
        private bool busy;
        private string message = "주소를 입력하고 시트 목록을 불러오세요.";
        private ConversionResult result;
        private Vector2 scroll;
        private string PreferenceKey => "COU.SheetToJSON." + Application.dataPath + ".";

        [MenuItem("Tools/Google Sheets/JSON Exporter")]
        public static void Open() => GetWindow<GoogleSheetJsonWindow>("Sheet to JSON");

        private void OnEnable()
        {
            minSize = new Vector2(540, 430);
            spreadsheetUrl = EditorPrefs.GetString(PreferenceKey + "sheet", spreadsheetUrl);
            apiUrl = EditorPrefs.GetString(PreferenceKey + "api", apiUrl);
            outputFolder = EditorPrefs.GetString(PreferenceKey + "folder", outputFolder);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Google Sheets → JSON", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("1행: 설명 / 2행: 필드명 / 3행: 자료형 / 4행부터: 데이터\n로그인 없이 접근 가능한 시트와 같은 문서를 참조하는 Apps Script가 필요합니다.", MessageType.Info);
            using (new EditorGUI.DisabledScope(busy))
            {
                EditorGUI.BeginChangeCheck();
                spreadsheetUrl = EditorGUILayout.TextField("스프레드시트 주소", spreadsheetUrl);
                apiUrl = EditorGUILayout.TextField("Apps Script 주소", apiUrl);
                if (EditorGUI.EndChangeCheck()) { sheets.Clear(); selected = 0; result = null; }
                outputFolder = EditorGUILayout.TextField("저장 폴더 (Assets 아래)", outputFolder);
                if (GUILayout.Button("시트 목록 불러오기"))
                {
                    result = null;
                    sheets.Clear(); selected = 0;
                    if (!GoogleSheetClient.TryGetSpreadsheetId(spreadsheetUrl, out _))
                        message = "올바른 Google 스프레드시트 주소를 입력하세요.";
                    else
                    {
                        EditorPrefs.SetString(PreferenceKey + "sheet", spreadsheetUrl);
                        EditorPrefs.SetString(PreferenceKey + "api", apiUrl);
                        Begin(GoogleSheetClient.FetchSheetList(apiUrl, list =>
                        {
                            sheets = list; selected = 0; SelectSheet();
                            message = $"{sheets.Count}개 시트를 불러왔습니다.";
                        }, Fail));
                    }
                }
                if (sheets.Count > 0)
                {
                    int next = EditorGUILayout.Popup("시트 선택", selected, sheets.Select(s => s.Name).ToArray());
                    if (next != selected) { selected = next; SelectSheet(); }
                }
                fileName = EditorGUILayout.TextField("출력 파일명", fileName);
                using (new EditorGUI.DisabledScope(sheets.Count == 0))
                {
                    if (GUILayout.Button("다운로드 및 검증"))
                    {
                        result = null;
                        Begin(GoogleSheetClient.DownloadSheetTsv(spreadsheetUrl, sheets[selected].Id, tsv =>
                        {
                            result = SheetJsonConverter.Convert(tsv);
                            message = result.Success ? $"검증 완료: {result.RowCount}행 / 오류 0개"
                                : string.Join("\n", result.Errors.Take(30));
                            if (result.Errors.Count > 30) message += $"\n외 {result.Errors.Count - 30}개 오류";
                        }, Fail));
                    }
                }
            }
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.HelpBox(busy ? "다운로드 중..." : message,
                result != null && !result.Success ? MessageType.Error : MessageType.Info);
            if (result != null && result.Success)
            {
                EditorGUILayout.LabelField("JSON 미리보기 (최대 20,000자)");
                string preview = result.Json.Length > 20000 ? result.Json.Substring(0, 20000) + "\n… 미리보기 생략" : result.Json;
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.TextArea(preview, GUILayout.MinHeight(180));
            }
            EditorGUILayout.EndScrollView();
            using (new EditorGUI.DisabledScope(busy || result == null || !result.Success))
                if (GUILayout.Button("JSON 저장")) SaveJson();
        }

        private void SelectSheet()
        {
            result = null;
            string name = sheets[selected].Name;
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            fileName = name + ".json";
        }

        private void Fail(string error) { message = error; result = null; }
        private void Begin(IEnumerator operation)
        {
            busy = true;
            EditorCoroutineUtility.StartCoroutine(Run(operation), this);
        }

        private IEnumerator Run(IEnumerator operation)
        {
            // Flatten nested enumerators so errors also release the UI lock.
            var stack = new Stack<IEnumerator>(); stack.Push(operation);
            try
            {
                while (stack.Count > 0)
                {
                    bool moved; object current = null;
                    try { moved = stack.Peek().MoveNext(); if (moved) current = stack.Peek().Current; }
                    catch (Exception e) { Fail(e.Message); break; }
                    if (!moved) { (stack.Pop() as IDisposable)?.Dispose(); continue; }
                    if (current is IEnumerator nested) stack.Push(nested);
                    else yield return current;
                }
            }
            finally
            {
                while (stack.Count > 0) (stack.Pop() as IDisposable)?.Dispose();
                busy = false;
                if (this != null) Repaint();
            }
        }

        private void SaveJson()
        {
            string temporary = null;
            try
            {
                if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
                    || !fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    throw new IOException("파일명은 경로 문자가 없는 .json 파일명이어야 합니다.");
                string project = Path.GetDirectoryName(Application.dataPath);
                string folder = Path.GetFullPath(Path.Combine(project, outputFolder));
                string assets = Path.GetFullPath(Application.dataPath);
                if (!folder.Equals(assets, StringComparison.OrdinalIgnoreCase)
                    && !folder.StartsWith(assets + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("저장 폴더는 이 프로젝트의 Assets 아래여야 합니다.");
                string path = Path.Combine(folder, fileName);
                if (File.Exists(path) && !EditorUtility.DisplayDialog("JSON 덮어쓰기", path + "\n기존 파일을 교체할까요?", "저장", "취소")) return;
                Directory.CreateDirectory(folder);
                temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
                File.WriteAllText(temporary, result.Json, new UTF8Encoding(false));
                if (File.Exists(path)) File.Replace(temporary, path, null);
                else File.Move(temporary, path);
                temporary = null;
                EditorPrefs.SetString(PreferenceKey + "folder", outputFolder);
                AssetDatabase.Refresh();
                message = $"저장 완료: {path} ({result.RowCount}행)";
            }
            catch (Exception e) { message = "저장 실패: " + e.Message; }
            finally { if (temporary != null && File.Exists(temporary)) File.Delete(temporary); }
        }
    }
}
#endif
