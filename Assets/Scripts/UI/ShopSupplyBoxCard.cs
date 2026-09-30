using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopSupplyBoxCard : MonoBehaviour
{
    [SerializeField] private Button actionButton;
    [SerializeField] private ItemGrade minGrade;
    [SerializeField] private ItemGrade maxGrade;

    private void Awake()
    {
        actionButton.onClick.AddListener(OnActionButtonClicked);
    }

    private void OnActionButtonClicked()
    {
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
