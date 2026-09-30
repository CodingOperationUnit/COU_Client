using UnityEngine;
using UnityEngine.UI;


public class ShopGoldCard : MonoBehaviour
{
    [SerializeField] private Button buyButton;
    [SerializeField] private int gemCost;
    [SerializeField] private int goldAmount;

    private void Awake()
    {
        buyButton.onClick.AddListener(OnCardClicked);
    }

    private void OnCardClicked()
    {
        if (!PlayerInventory.Instance.TrySpendGem(gemCost))
            return;

        PlayerInventory.Instance.AddGold(goldAmount);
        UIManager.Instance.Get<GoldPurchaseSuccessPopup>().Show();
    }
}
