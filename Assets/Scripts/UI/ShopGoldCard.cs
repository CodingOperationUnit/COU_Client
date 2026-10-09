using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ShopGoldCard : MonoBehaviour
{
    [SerializeField] private Button buyButton;
    [SerializeField] private int productId;  
    [SerializeField] private int gemCost;     
    [SerializeField] private int goldAmount;  

    [SerializeField] private Color affordableColor = new Color(0.243f, 0.702f, 0.008f, 1f);
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.15f);

    private void Awake()
    {
        buyButton.onClick.AddListener(OnCardClicked);
    }

    private void OnEnable()
    {
        PlayerInventory.Instance.OnInventoryChanged += UpdateAffordability;
        UpdateAffordability();
    }

    private void OnDisable()
    {
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnInventoryChanged -= UpdateAffordability;
    }

    private void UpdateAffordability()
    {
        var affordable = PlayerInventory.Instance.Gem >= gemCost;
        buyButton.targetGraphic.color = affordable ? affordableColor : normalColor;
    }

    private void OnCardClicked()
    {
        if (PlayerInventory.Instance.Gem < gemCost)
        {
            UIManager.Instance.Get<LogPopup>().Show("알림", "보석이 부족합니다.");
            return;
        }

        buyButton.interactable = false;  
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

        UIManager.Instance.Get<LogPopup>().Show("구매 완료!", "골드 구매가 완료되었습니다!");
    }
}
