using UnityEngine;

public static class ItemDatabase
{
    public static Sprite GetGradeIcon(EquipSlotType slotType, ItemGrade grade)
    {
        var folder = slotType == EquipSlotType.Weapon ? "Weapon" : "Equip";
        return Resources.Load<Sprite>($"Equip/Inven/Item/{folder}/Grade/{grade}");
    }

}
