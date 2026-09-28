using System;
using UnityEngine;


[Serializable]
public class ItemData
{
    public string itemId;
    public string itemName;
    public string description;
    public string iconPath;
    public string slotType;     
    public string skillStartGrade;  // 등급 스킬
    public int hpBonus;
    public int attackBonus;
    public string[] gradeSkills;  

    public EquipSlotType SlotType => Enum.Parse<EquipSlotType>(slotType);
    public ItemGrade SkillStartGrade => Enum.Parse<ItemGrade>(skillStartGrade);

    public ItemGrade GetSkillGrade(int index) => (ItemGrade)((int)SkillStartGrade + index);
}

[Serializable]
public class ItemDataListWrapper
{
    public ItemData[] items;
}