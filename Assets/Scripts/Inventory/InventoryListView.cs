using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryListView : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private ItemSlotView itemSlotPrefab;

    private readonly List<ItemSlotView> pool = new();

    private void OnEnable()
    {
        PlayerInventory.Instance.OnInventoryChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnInventoryChanged -= Refresh;
    }

    private void Refresh()
    {
        var items = PlayerInventory.Instance.Items.Where(i => !i.isEquipped).ToList();

        while (pool.Count < items.Count)
            pool.Add(Instantiate(itemSlotPrefab, content));

        for (var i = 0; i < pool.Count; i++)
        {
            var active = i < items.Count;
            pool[i].gameObject.SetActive(active);
            if (active)
                pool[i].Setup(items[i]);
        }
    }
}
