using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private readonly List<OwnedItem> items = new();
    private readonly Dictionary<EquipSlotType, OwnedItem> equipped = new();

    public event Action OnInventoryChanged;

    public IReadOnlyList<OwnedItem> Items => items;

    private void Awake()
    {
        Instance = this;
        ItemDatabase.Load();
    }

    public void AddItem(long itemId)
    {
        items.Add(new OwnedItem(itemId));
        OnInventoryChanged?.Invoke();
    }

    public OwnedItem GetEquipped(EquipSlotType slot)
        => equipped.GetValueOrDefault(slot);

    public void Equip(OwnedItem item)
    {
        var slot = item.Data.SlotType;

        if (equipped.TryGetValue(slot, out var current))
            current.isEquipped = false;

        item.isEquipped = true;
        equipped[slot] = item;
        OnInventoryChanged?.Invoke();
    }

    public void Unequip(EquipSlotType slot)
    {
        if (!equipped.TryGetValue(slot, out var item))
            return;

        item.isEquipped = false;
        equipped.Remove(slot);
        OnInventoryChanged?.Invoke();
    }

    public int GetTotalStat(Func<ItemData, int> selector)
        => equipped.Values.Sum(item => selector(item.Data));
}
