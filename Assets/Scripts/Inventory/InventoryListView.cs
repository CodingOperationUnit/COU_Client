using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryListView : MonoBehaviour
{
    // 슬롯: 무기가 먼저, 나머지 부위 순서는 상관없음
    // 레벨: 레벨 높은 순
    // 등급: 등급 높은 순
    public enum SortMode { Slot, Level, Grade }

    [SerializeField] private Transform content;
    [SerializeField] private ItemSlotView itemSlotPrefab;

    private readonly List<ItemSlotView> pool = new();

    public SortMode CurrentSortMode { get; private set; } = SortMode.Slot;

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

    // 정렬 버튼을 누를 때마다 슬롯 -> 레벨 -> 등급 -> 슬롯 순으로 정렬 기준을 바꾼다.
    public SortMode CycleSortMode()
    {
        CurrentSortMode = (SortMode)(((int)CurrentSortMode + 1) % 3);
        Refresh();
        return CurrentSortMode;
    }

    private void Refresh()
    {
        IEnumerable<OwnedItem> items = PlayerInventory.Instance.Items.Where(i => !i.isEquipped);

        items = CurrentSortMode switch
        {
            SortMode.Slot => items.OrderBy(i => i.Data.SlotType == EquipSlotType.Weapon ? 0 : 1),
            SortMode.Level => items.OrderByDescending(i => i.level),
            SortMode.Grade => items.OrderByDescending(i => (int)i.Grade),
            _ => items
        };

        var list = items.ToList();

        while (pool.Count < list.Count)
            pool.Add(Instantiate(itemSlotPrefab, content));

        for (var i = 0; i < pool.Count; i++)
        {
            var active = i < list.Count;
            pool[i].gameObject.SetActive(active);
            if (active)
                pool[i].Setup(list[i]);
        }
    }
}
