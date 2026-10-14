using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 친구와의 1:1 채팅 화면
public class ChatPopup : UIPopup
{
    private const int MaxMessageLength = 200;
    private const int MaxViews = 300;                
    private const float StickToBottomThreshold = 0.05f; 

    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text titleText;

    [Header("메시지 목록")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform listRoot;
    [SerializeField] private ChatMessageView messagePrefab;
    [SerializeField] private TMP_Text statusText;

    [Header("입력")]
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private Button sendButton;

    private readonly List<ChatMessageView> views = new();

    private long friendId;
    private bool isSending;
    private Coroutine scrollRoutine;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        sendButton.onClick.AddListener(Send);

        messageInput.characterLimit = MaxMessageLength;
        messageInput.onSubmit.AddListener(_ => Send());
    }

    // 친구와의 대화를 연다
    public void Show(long friendPlayerId, string nickname)
    {
        friendId = friendPlayerId;
        titleText.text = nickname;
        Open();
    }

    public override void Open()
    {
        base.Open();

        ClearMessages();
        messageInput.SetTextWithoutNotify(string.Empty);
        isSending = false;
        sendButton.interactable = true;
        ShowStatus("불러오는 중...");

        var chat = GameManager.ServerChat;
        chat.MessagesReceived -= OnMessagesReceived;
        chat.MessagesReceived += OnMessagesReceived;
        chat.StartConversation(friendId, OnInitialLoaded);
    }

    public override void Close()
    {
        var chat = GameManager.ServerChat;
        chat.MessagesReceived -= OnMessagesReceived;
        chat.StopConversation();

        messageInput.DeactivateInputField();
        base.Close();
    }

    private void OnInitialLoaded(bool success, string message, List<ChatMessageResponse> list)
    {
        if (!success)
        {
            ShowStatus(message);
            return;
        }

        ClearMessages();
        foreach (var m in list) AddMessage(m);

        if (list.Count == 0) ShowStatus("첫 메시지를 보내보세요!");
        else HideStatus();

        ScrollToBottom();
    }
    // 폴링으로 새 메시지가 왔을 때 (내가 방금 보낸 것 포함)
    private void OnMessagesReceived(List<ChatMessageResponse> list)
    {
        bool stick = scrollRect.verticalNormalizedPosition <= StickToBottomThreshold
                     || listRoot.rect.height <= scrollRect.viewport.rect.height;

        bool hasMine = false;
        foreach (var m in list)
        {
            AddMessage(m);
            hasMine |= m.mine;
        }

        HideStatus();

        if (stick || hasMine) ScrollToBottom();
    }

    private void Send()
    {
        if (isSending) return;

        string content = messageInput.text.Trim();
        if (content.Length == 0) return;

        isSending = true;
        sendButton.interactable = false;

        GameManager.ServerChat.Send(content, (success, message) =>
        {
            isSending = false;
            sendButton.interactable = true;

            if (!success)
            {
                UIManager.Instance.Get<MessagePopup>().ShowAlert(message);
                return;
            }

            messageInput.text = string.Empty;
            messageInput.ActivateInputField();
        });
    }

    private void AddMessage(ChatMessageResponse message)
    {
        var view = Instantiate(messagePrefab, listRoot);
        view.Setup(message);
        views.Add(view);

        if (views.Count > MaxViews)
        {
            Destroy(views[0].gameObject);
            views.RemoveAt(0);
        }
    }

    private void ClearMessages()
    {
        foreach (var view in views)
            if (view != null) Destroy(view.gameObject);
        views.Clear();
    }

    private void ScrollToBottom()
    {
        if (scrollRoutine != null) StopCoroutine(scrollRoutine);
        scrollRoutine = StartCoroutine(ScrollToBottomNextFrame());
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;  
        LayoutRebuilder.ForceRebuildLayoutImmediate(listRoot);
        scrollRect.verticalNormalizedPosition = 0f;
        scrollRoutine = null;
    }

    private void ShowStatus(string text)
    {
        statusText.text = text;
        statusText.gameObject.SetActive(true);
    }

    private void HideStatus()
        => statusText.gameObject.SetActive(false);
}
