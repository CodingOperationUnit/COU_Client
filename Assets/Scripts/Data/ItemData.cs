using System;
using UnityEngine;

[Serializable]
public class ItemData
{
    public long itemId;
    public string itemName;
    public string description;
    public string iconPath;
    public string slotType;
    public string grade;           
    public int hpBonus;
    public int attackBonus;
    public string[] gradeSkills; 

    public EquipSlotType SlotType => Enum.Parse<EquipSlotType>(slotType);
    public ItemGrade Grade => Enum.Parse<ItemGrade>(grade);
}

[Serializable]
public class ItemDataListWrapper
{
    public ItemData[] items;
}
