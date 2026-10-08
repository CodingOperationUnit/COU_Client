using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class ServerLoginManager : MonoSingleton<ServerLoginManager>
{
    // 로그인 후 다른 API 요청에 사용할 토큰과 계정 정보
    public string AccessToken { get; private set; }
    public ServerAccountData CurrentAccount { get; private set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken);

    // 인증이 필요한 요청의 Authorization 헤더 값
    public string AuthorizationHeader => "Bearer " + AccessToken;

    // 버튼을 연타해도 요청이 한 번만 나가도록 막는다
    private bool isRequesting;
    
    // 회원가입: onComplete(성공 여부, 안내 메시지)
    public IEnumerator SignUp(string accountLoginId, string password, Action<bool, string> onComplete)
    {
        if (IsLoggedIn)
        {
            onComplete?.Invoke(false, "로그아웃 후 회원가입하세요.");
            yield break;
        }

        accountLoginId = NormalizeLoginId(accountLoginId);
        
        // 서버와 같은 규칙으로 먼저 검사해서 불필요한 요청을 줄인다
        if (!Regex.IsMatch(accountLoginId, @"^[a-z0-9_]{3,20}$"))
        {
            onComplete?.Invoke(false, "아이디는 영문, 숫자, 밑줄로 3 ~ 20자 입력해주세요.");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            onComplete?.Invoke(false, "비밀번호를 입력해주세요.");
            yield break;
        }

        var body = new SignupRequest
        {
            accountLoginId = accountLoginId,
            password = password,
        };

        yield return PostJson(GameConstants.Server.SIGNUP_API, body, (code, text) =>
        {
            if (code == 201)
            {
                onComplete?.Invoke(true, "회원가입이 완료되었습니다.");
                return;
            }

            onComplete?.Invoke(false, GetErrorMessage(code, text, "회원가입에 실패했습니다."));
        });
    }
    
    // 로그인: 성공하면 토큰과 계정 정보를 보관한다.
    public IEnumerator Login(string accountLoginId, string password, Action<bool, string> onComplete)
    {
        if (IsLoggedIn)
        {
            onComplete?.Invoke(false, "이미 로그인되어 있습니다.");
            yield break;
        }

        accountLoginId = NormalizeLoginId(accountLoginId);

        if (string.IsNullOrEmpty(accountLoginId) || string.IsNullOrEmpty(password))
        {
            onComplete?.Invoke(false, "아이디와 비밀번호를 입력해주세요.");
            yield break;
        }

        var body = new LoginRequest
        {
            accountLoginId = accountLoginId,
            password = password
        };

        yield return PostJson(GameConstants.Server.LOGIN_API, body, (code, text) =>
        {
            if (code != 200)
            {
                onComplete?.Invoke(false, GetErrorMessage(code, text, "로그인에 실패했습니다."));
                return;
            }

            LoginResponse response;
            try
            {
                response = JsonConvert.DeserializeObject<LoginResponse>(text);
            }
            catch (JsonException e)
            {
                Debug.LogError($"[ServerLogin] 로그인 응답 변환 실패: {e.Message}");
                onComplete?.Invoke(false, "로그인 응답을 읽지 못했습니다.");
                return;
            }

            if (string.IsNullOrEmpty(response?.accessToken))
            {
                onComplete?.Invoke(false, "로그인 응답에 토큰이 없습니다.");
                return;
            }

            AccessToken = response.accessToken;
            CurrentAccount = response.account;
            onComplete?.Invoke(true, "로그인되었습니다.");
        });
    }
    
    // 로그아웃: 서버에 따로 알릴 필요 없이 토큰과 플레이어 데이터를 지운다
    public void Logout()
    {
        AccessToken = null;
        CurrentAccount = null;
        GameManager.PlayerData.ClearPlayerData();
    }
    
    // JSON 본문으로 POST 요청을 보낸다. onResponse(응답 코드, 응답 본문)
    private IEnumerator PostJson(string path, object body, Action<long, string> onResponse)
    {
        if (isRequesting)
        {
            onResponse(GameConstants.Value.ALREADY_REQUESTING, null);
            yield break;
        }

        isRequesting = true;

        string json = JsonConvert.SerializeObject(body);

        using (var request = new UnityWebRequest(GameConstants.Server.BASE_URL + path, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = GameConstants.Value.REQUEST_TIMEOUT_SECONDS;

            yield return request.SendWebRequest();

            isRequesting = false;
            
            // 서버가 꺼져있거나 주소가 틀린 경우 (4xx/5xx는 ProtocolError라 아래로 내려감)
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning($"[ServerLogin] 서버 연결 실패: {request.error}");
                onResponse(GameConstants.Value.CONNECTION_FAILED, null);
                yield break;
            }

            onResponse(request.responseCode, request.downloadHandler.text);
        }
    }
    
    // 실패 응답을 화면에 보여줄 문구로 바꾼다.
    private static string GetErrorMessage(long code, string responseText, string fallback)
    {
        if (code == GameConstants.Value.CONNECTION_FAILED)
            return "서버에 연결할 수 없습니다. 잠시 후 다시 시도해 주세요.";

        if (code == GameConstants.Value.ALREADY_REQUESTING)
            return "요청을 처리하고 있습니다. 잠시만 기다려 주세요.";

        // 아이디가 없거나 비밀번호가 틀린 경우를 구분하지 않는다 (서버와 같은 방침)
        if (code == 401)
            return "아이디 또는 비밀번호가 일치하지 않습니다.";
        
        // 서버가 보낸 message가 있으면 그대로 사용
        if (!string.IsNullOrEmpty(responseText))
        {
            try
            {
                var error = JsonConvert.DeserializeObject<ErrorResponse>(responseText);
                if (!string.IsNullOrEmpty(error?.message))
                    return error.message;
            }
            catch (JsonException)
            {
                // 형식이 다르면 아래 기본 문구 사용
            }
        }

        return code == 409 ? "이미 사용 중인 아이디 또는 닉네임입니다." : fallback;
    }

    private static string NormalizeLoginId(string accountLoginId)
        => (accountLoginId ?? string.Empty).Trim().ToLowerInvariant();
}