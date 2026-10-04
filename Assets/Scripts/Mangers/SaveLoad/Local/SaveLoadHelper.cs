using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class SaveLoadHelper
{
    private static string AccountPath =>
        Path.Combine(Application.persistentDataPath, GameConstants.Paths.ACCOUNT_SAVE_PATH);

    private static string PlayerDirectory =>
        Path.Combine(Application.persistentDataPath, GameConstants.Paths.PLAYER_DIRECTORY);

    // 계정 목록 저장
    public static void SaveAccounts(AccountSaveData data)
    {
        SaveJson(AccountPath, data);
    }

    public static AccountSaveData LoadAccounts()
    {
        // 첫 실행에는 계정 파일이 없으므로 빈 목록을 반환
        return LoadJson<AccountSaveData>(AccountPath) ?? new AccountSaveData();
    }

    // 플레이어 데이터
    public static void SavePlayer(PlayerSaveData data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        SaveJson(GetPlayerPath(data.playerId), data);
    }

    public static PlayerSaveData LoadPlayer(string playerId)
    {
        PlayerSaveData data = LoadJson<PlayerSaveData>(GetPlayerPath(playerId));

        // 다른 계정의 데이터를 잘못 적용하지 않도록 확인
        if (data != null && data.playerId != playerId)
        {
            throw new InvalidDataException("요청한 계정과 저장 데이터의 playerId가 다릅니다.");
        }

        return data;
    }

    // 공통 파일 처리
    private static void SaveJson<T>(string path, T data) where T : class
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        Directory.CreateDirectory(Path.GetDirectoryName(path));

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);

        File.WriteAllText(path, json);
    }

    private static T LoadJson<T>(string path) where T : class
    {
        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        T data = JsonConvert.DeserializeObject<T>(json);

        if (data == null)
        {
            throw new InvalidDataException($"저장 파일에 데이터가 없습니다: {path}");
        }

        return data;
    }

    private static string GetPlayerPath(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId)
            || playerId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || playerId.Contains("/")
            || playerId.Contains("\\")
            || playerId == "."
            || playerId == "..")
        {
            throw new ArgumentException("파일명으로 사용할 수 없는 playerId입니다.", nameof(playerId));
        }

        return Path.Combine(PlayerDirectory, playerId + ".json");
    }
}