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

    private void Awake()
    {
        actionButton.onClick.AddListener(OnActionButtonClicked);
    }

    private void OnActionButtonClicked()
    {
        if (!PlayerInventory.Instance.TrySpendGem(gemCost))
            return;

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
