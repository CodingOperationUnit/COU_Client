using UnityEngine;
using UnityEngine.UI;

public class ShopGemCard : MonoBehaviour
{
    [SerializeField] private Button buyButton;
    [SerializeField] private int gemAmount;

    private void Awake()
    {
        buyButton.onClick.AddListener(OnCardClicked);
    }

    private void OnCardClicked()
    {
        PlayerInventory.Instance.AddGem(gemAmount);
        UIManager.Instance.Get<GemPurchaseSuccessPopup>().Show();
    }
}
