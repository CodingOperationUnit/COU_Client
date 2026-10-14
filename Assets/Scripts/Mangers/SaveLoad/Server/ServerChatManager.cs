using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerChatManager : MonoSingleton<ServerChatManager>
{
    private const float PollInterval = 2f;   // 폴링 주기
    private const int PageSize = 50;

    // 새 메시지가 도착했을 때
    public event Action<List<ChatMessageResponse>> MessagesReceived;

    public long CurrentFriendId { get; private set; }
    public bool InConversation => CurrentFriendId != 0;

    // 측정용: 폴링 요청 횟수 (WebSocket과 비교 예정)
    public int PollRequestCount { get; private set; }

    private int session;            // 대화가 바뀔 때마다 증가. 지난 대화의 늦은 응답을 버리는 용도
    private long lastMessageId;     // 마지막으로 받은 메시지 ID
    private bool isRequesting;
    private Coroutine pollRoutine;

    // 대화 시작: 최근 메시지를 불러오고, 성공하면 폴링을 시작한다
    public void StartConversation(long friendPlayerId, Action<bool, string, List<ChatMessageResponse>> onComplete)
    {
        StopConversation();
        CurrentFriendId = friendPlayerId;
        int mySession = session;

        StartCoroutine(ChatApi.GetMessages(friendPlayerId, 0, PageSize, r =>
        {
            if (mySession != session) return;   // 그 사이 대화가 닫히거나 바뀜

            if (!r.IsSuccess)
            {
                onComplete?.Invoke(false, r.ToUserMessage("대화를 불러오지 못했습니다."), null);
                return;
            }

            var list = r.Data ?? new List<ChatMessageResponse>();
            UpdateLastId(list);
            onComplete?.Invoke(true, null, list);
            pollRoutine = StartCoroutine(PollLoop(mySession, friendPlayerId));
        }));
    }

    // 대화 종료 
    public void StopConversation()
    {
        session++;
        CurrentFriendId = 0;
        lastMessageId = 0;
        isRequesting = false;

        if (pollRoutine != null)
        {
            StopCoroutine(pollRoutine);
            pollRoutine = null;
        }
    }

    // 메시지 보내기
    public void Send(string content, Action<bool, string> onComplete)
    {
        if (!InConversation)
        {
            onComplete?.Invoke(false, "대화 상대가 없습니다.");
            return;
        }

        int mySession = session;
        long friendId = CurrentFriendId;

        StartCoroutine(ChatApi.Send(friendId, content, r =>
        {
            if (!r.IsSuccess)
            {
                onComplete?.Invoke(false, r.ToUserMessage("메시지를 보내지 못했습니다."));
                return;
            }

            onComplete?.Invoke(true, null);

            if (mySession == session)
                StartCoroutine(PollOnce(mySession, friendId));
        }));
    }

    private IEnumerator PollLoop(int mySession, long friendId)
    {
        var wait = new WaitForSeconds(PollInterval);

        while (mySession == session)
        {
            yield return wait;
            yield return PollOnce(mySession, friendId);
        }
    }

    private IEnumerator PollOnce(int mySession, long friendId)
    {
        while (isRequesting) yield return null;   // 진행 중인 요청이 끝날 때까지 기다린다 
        if (mySession != session) yield break;

        isRequesting = true;
        PollRequestCount++;

        yield return ChatApi.GetMessages(friendId, lastMessageId, PageSize, r =>
        {
            if (mySession != session) return;   // 지난 대화의 응답이면 버린다
            isRequesting = false;

            if (!r.IsSuccess || r.Data == null || r.Data.Count == 0) return;

            UpdateLastId(r.Data);
            MessagesReceived?.Invoke(r.Data);
        });
    }

    private void UpdateLastId(List<ChatMessageResponse> list)
    {
        if (list.Count > 0)
            lastMessageId = Math.Max(lastMessageId, list[list.Count - 1].messageId);
    }
}