using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopGemCard : MonoBehaviour
{
    [SerializeField] private Button buyButton;
    [SerializeField] private int productId;  
    [SerializeField] private int gemAmount;   

    private void Awake()
    {
        buyButton.onClick.AddListener(OnCardClicked);
    }

    private void OnCardClicked()
    {
        buyButton.interactable = false;   // 응답이 올 때까지 연타 방지
        GameManager.ServerShop.Purchase(productId, OnPurchased);
    }

    private void OnPurchased(bool success, string message, List<OwnedItem> rewardedItems)
    {
        if (this == null) return;   // 응답이 오기 전에 씬이 바뀐 경우

        buyButton.interactable = true;

        if (!success)
        {
            UIManager.Instance.Get<LogPopup>().Show("알림", message);
            return;
        }

        UIManager.Instance.Get<LogPopup>().Show("구매 완료!", "보석이 성공적으로 지급되었습니다!");
    }
}
