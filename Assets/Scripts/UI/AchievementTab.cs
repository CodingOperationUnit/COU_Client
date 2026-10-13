using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// 업적 탭. Header는 고정, List(ScrollRect)의 Content 안에서 카드만 스크롤된다.
// 탭이 켜질 때 정적 업적 데이터 수만큼 카드를 던진다.
// 열 때 정적 데이터로 카드를 만들고, 서버 진행도를 받아 다시 그린다. 카드를 누르면 보상을 받는다
public class AchievementTab : UIView
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private AchievementCard cardPrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private Button topButton;

    private readonly List<AchievementCard> cards = new();

    private void Awake()
    {
        backButton.onClick.AddListener(Close);
        topButton.onClick.AddListener(ScrollToTop);
    }
    
    // Open()으로 열 때도, 인스펙터에서 직접 켤 때도 다시 채운다
    private void OnEnable()
    {
        Refresh();
        ScrollToTop();
        
        GameManager.ServerAchievement.Load((success, message) =>
        {
            if (success)
                Refresh();
            else 
                Debug.LogWarning("[AchievementTab] " + message);
        });
    }
    
    public void Refresh()
    {
        var datas = JsonDataManager.Instance.AchievementDataDic;
        if (datas == null)
        {
            Debug.LogWarning("[AchievementTab] 업적 데이터가 없습니다.");
            return;
        }

        // 조건 종류(처치 → 수집 → 연마 → 경험치) 순, 같은 종류 안에서는 ID 순
        var sorted = datas.Values
            .OrderBy(d => d.ConditionType)
            .ThenBy(d => d.achievementId)
            .ToList();

        // 부족한 카드만 새로 만든다
        while (cards.Count < sorted.Count)
        {
            var card = Instantiate(cardPrefab, scrollRect.content);
            card.Clicked += OnCardClicked;
            cards.Add(card);
        }

        // 데이터 수만큼 켜서 초기화하고, 남는 카드는 숨긴다
        var manager = ServerAchievementManager.Instance;
        for (int i = 0; i < cards.Count; i++)
        {
            bool used = i < sorted.Count;
            cards[i].gameObject.SetActive(used);
            if (used)
                cards[i].Set(sorted[i], manager.GetProgress(sorted[i].achievementId));
        }
    }
    
    private void OnCardClicked(AchievementCard card)
    {
        if (!card.Progress.CanClaim)
        {
            Debug.Log($"[AchievementTab] {card.Data.achievementName}: 받을 수 있는 보상이 없습니다.");
            return;
        }

        // 받을 단계의 보상을 미리 읽어 둔다 (수령 후에는 claimedStep이 올라간다)
        AchievementData data = card.Data;
        int step = card.Progress.claimedStep;
        AchievementRewardType rewardType = data.GetRewardType(step);
        int amount = data.GetRewardAmount(step);

        GameManager.ServerAchievement.Claim(data.achievementId, (success, message) =>
        {
            if (!success)
            {
                Debug.LogWarning($"[AchievementTab] {data.achievementName} 보상 수령 실패: {message}");
                return;
            }

            card.Set(data, GameManager.ServerAchievement.GetProgress(data.achievementId));

            // TODO: 보상 화면이 생기면 이 로그 대신 보상 팝업을 띄운다
            Debug.Log($"[AchievementTab] 보상 수령: {data.achievementName} {step + 1}단계 → {rewardType} {amount:N0}" +
                      $" (보유 골드 {GameManager.PlayerData.currentData.currency.currencyGold:N0}," +
                      $" 보석 {GameManager.PlayerData.currentData.currency.currencyGem:N0})");
        });
    }
    
    private void ScrollToTop()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        scrollRect.StopMovement();
        scrollRect.verticalNormalizedPosition = 1f;
    }
}
