using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class LocalLoginManager : MonoSingleton<LocalLoginManager>
{
    public bool isLoggedIn => GameManager.LocalSaveLoad.isPlayerDataLoaded;
    public string currentPlayerID => GameManager.LocalSaveLoad.currentData?.playerID;

    public bool SignUp(string playerID, string password, out string message)
    {
        if (isLoggedIn)
        {
            message = "로그아웃 후 회원가입 하세요.";
            return false;
        }

        playerID = NormalizePlayerID(playerID);

        if (!Regex.IsMatch(playerID, @"^[a-z0-9_]{3,20}$"))
        {
            message = "아이디는 영문, 숫자, 밑줄로 3 ~ 20자 입력해주세요.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            message = "비밀번호를 입력해주세요.";
            return false;
        }

        try
        {
            AccountSaveData accountData = SaveLoadHelper.LoadAccounts();

            if (accountData.accounts == null)
            {
                message = "계정 목록 데이터가 올바르지 않습니다.";
                return false;
            }

            bool alreadyExists = accountData.accounts.Exists(account =>
                account != null && string.Equals(account.playerID, playerID, StringComparison.OrdinalIgnoreCase)
            );

            if (alreadyExists)
            {
                message = "이미 사용 중인 아이디입니다.";
                return false;
            }

            // 초기 플레이어 파일을 먼저 저장합니다.
            // 기존 파일이 있으면 덮어쓰지 않고 가입을 중단합니다.
            // 계정 목록 저장이 실패했을 때 남을 수 있는 파일도 보호합니다.
            if (SaveLoadHelper.LoadPlayer(playerID) != null)
            {
                message = "해당 아이디의 게임 데이터가 이미 있습니다. "
                          + "저장 상태를 확인해 주세요.";
                return false;
            }

            var playerData = new PlayerSaveData
            {
                playerID = playerID,
                gold = 0,
                exp = 0
            };

            SaveLoadHelper.SavePlayer(playerData);
            accountData.accounts.Add(new LocalAccountData
            {
                playerID = playerID,
                password = password
            });

            SaveLoadHelper.SaveAccounts(accountData);
            message = "회원가입이 완료되었습니다.";
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"회원가입 처리 실패: {e.Message}");
            message = "회원가입 저장에 실패했습니다. " + "저장 상태를 확인해주세요.";

            return false;
        }
    }

    public bool Login(string playerID, string password, out string message)
    {
        if (isLoggedIn)
        {
            message = "이미 로그인되어 있습니다.";
            return false;
        }
        
        playerID = NormalizePlayerID(playerID);
        
        if (string.IsNullOrEmpty(playerID) || string.IsNullOrEmpty(password))
        {
            message = "아이디와 비밀번호를 입력해 주세요.";
            return false;
        }

        try
        {
            AccountSaveData accountData = SaveLoadHelper.LoadAccounts();

            if (accountData.accounts == null)
            {
                message = "계정 목록 데이터가 올바르지 않습니다.";
                return false;
            }

            LocalAccountData account = accountData.accounts.Find(item =>
                item != null && string.Equals(item.playerID, playerID, StringComparison.OrdinalIgnoreCase)
            );

            if (account == null || account.password != password)
            {
                message = "아이디 또는 비밀번호가 일치하지 않습니다.";
                return false;
            }

            // 입력값 대신 계정에 기록된 ID로 파일을 조회합니다.
            if (!GameManager.LocalSaveLoad.LoadPlayerAfterLogin(account.playerID))
            {
                message = "플레이어 데이터를 불러오지 못했습니다.";
                return false;
            }

            message = "로그인되었습니다.";
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"로그인 처리 실패: {e.Message}");
            message = "로그인 처리 중 오류가 발생했습니다.";
            return false;
        }
    }

    public bool Logout(out string message)
    {
        if (!GameManager.LocalSaveLoad.Logout())
        {
            message = "저장에 실패하여 로그아웃하지 못했습니다.";
            return false;
        }
        
        message = "로그아웃되었습니다.";
        return true;
    }

    private static string NormalizePlayerID(string playerID)
    {
        return (playerID ?? string.Empty).Trim().ToLowerInvariant();
    }
}
