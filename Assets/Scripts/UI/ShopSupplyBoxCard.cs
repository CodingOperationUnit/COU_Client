using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// 지원품 상자 카드(ShopContent.prefab의 SupplyBox_*) 하나에 붙는 컴포넌트.
// 카드를 누르면 보석을 소모하고, 성공하면 무작위 장비를 지급한다.
public class ShopSupplyBoxCard : MonoBehaviour
{
    [SerializeField] private Button actionButton;
    [SerializeField] private ItemGrade minGrade;
    [SerializeField] private ItemGrade maxGrade;
    [SerializeField] private int gemCost;

    [SerializeField] private Color affordableColor = new Color(0.243f, 0.702f, 0.008f, 1f);
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.15f);

    private void Awake()
    {
        actionButton.onClick.AddListener(OnActionButtonClicked);
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
        actionButton.targetGraphic.color = affordable ? affordableColor : normalColor;
    }

    private void OnActionButtonClicked()
    {
        if (!PlayerInventory.Instance.TrySpendGem(gemCost))
        {
            UIManager.Instance.Get<LogPopup>().Show("알림", "보석이 부족합니다.");
            return;
        }

        var pool = ItemDatabase.GetAll()
            .Where(data => data.Grade >= minGrade && data.Grade <= maxGrade)
            .ToList();

        if (pool.Count == 0)
            return;

        var picked = pool[Random.Range(0, pool.Count)];
        var owned = PlayerInventory.Instance.AddItem(picked.itemId);
        UIManager.Instance.Get<SupplyResultPopup>().Show(owned);
    }
}
