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
        UIManager.Instance.Get<LogPopup>().Show("구매 완료!", "보석이 성공적으로 지급되었습니다!");
    }
}
