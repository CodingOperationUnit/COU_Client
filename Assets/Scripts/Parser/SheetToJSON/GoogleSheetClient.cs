#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace COU.EditorTools.SheetToJSON
{
    public sealed class SheetInfo
    {
        public string Name;
        public int Id;
    }

    public static class GoogleSheetClient
    {
        public static bool TryGetSpreadsheetId(string url, out string id)
        {
            id = null;
            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
                || uri.Scheme != "https" || uri.Host != "docs.google.com") return false;
            var match = Regex.Match(uri.AbsolutePath, @"^/spreadsheets/d/([a-zA-Z0-9_-]+)(?:/|$)");
            if (!match.Success) return false;
            id = match.Groups[1].Value;
            return true;
        }

        public static IEnumerator FetchSheetList(string apiUrl, Action<List<SheetInfo>> success, Action<string> failure)
        {
            if (!Uri.TryCreate(apiUrl.Trim(), UriKind.Absolute, out var uri)
                || uri.Scheme != "https" || uri.Host != "script.google.com"
                || !uri.AbsolutePath.EndsWith("/exec", StringComparison.Ordinal))
            {
                failure("Apps Script의 https://script.google.com/.../exec 배포 주소를 입력하세요.");
                yield break;
            }
            string text = null, error = null;
            yield return Download(uri.AbsoluteUri, value => text = value, value => error = value);
            if (error != null) { failure(error); yield break; }
            List<SheetInfo> sheets;
            try
            {
                var array = JObject.Parse(text)["sheetData"] as JArray;
                if (array == null) throw new FormatException("sheetData 배열이 없습니다.");
                sheets = new List<SheetInfo>();
                foreach (var entry in array)
                {
                    string name = entry.Value<string>("sheetName");
                    var idToken = entry["sheetId"];
                    if (string.IsNullOrWhiteSpace(name) || idToken == null || idToken.Type != JTokenType.Integer)
                        throw new FormatException("sheetName 또는 sheetId가 올바르지 않습니다.");
                    sheets.Add(new SheetInfo { Name = name, Id = idToken.Value<int>() });
                }
                if (sheets.Count == 0) throw new FormatException("시트 목록이 비어 있습니다.");
            }
            catch (Exception e) { failure("시트 목록 해석 실패: " + e.Message); yield break; }
            success(sheets);
        }

        public static IEnumerator DownloadSheetTsv(string spreadsheetUrl, int gid, Action<string> success, Action<string> failure)
        {
            if (!TryGetSpreadsheetId(spreadsheetUrl, out string id))
            { failure("올바른 Google 스프레드시트 주소를 입력하세요."); yield break; }
            yield return Download($"https://docs.google.com/spreadsheets/d/{id}/export?format=tsv&gid={gid}", success, failure);
        }

        private static IEnumerator Download(string url, Action<string> success, Action<string> failure)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = 30;
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                    failure($"다운로드 실패 (HTTP {request.responseCode}): {request.error}");
                else success(request.downloadHandler.text);
            }
        }
    }
}
#endif
