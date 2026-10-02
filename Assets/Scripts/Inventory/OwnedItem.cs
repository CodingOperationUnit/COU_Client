using System;
using UnityEngine;


[Serializable]
public class OwnedItem
{
    public string instanceId;
    public long itemId;
    public bool isEquipped;
    public int level = 1;

    public ItemGrade grade;

    public ItemData Data => ItemDatabase.Get(itemId);
    public ItemGrade Grade => grade;

    public int MaxLevel => ItemLevelConfig.MaxLevel;
    public bool CanLevelUp => level < MaxLevel;
    public int NextLevelUpCost => ItemLevelConfig.GetLevelUpCost(level);

    public bool CanSynthesize => grade != ItemGrade.Rare;
    public ItemGrade NextGrade => grade + 1;

    private float StatMultiplier => ItemLevelConfig.GetStatMultiplier(level) * ItemLevelConfig.GetGradeMultiplier(grade);

    public int ScaledHp => Mathf.RoundToInt(Data.hpBonus * StatMultiplier);
    public int ScaledAttack => Mathf.RoundToInt(Data.attackBonus * StatMultiplier);
    public int ScaledMoveSpeed => Mathf.RoundToInt(Data.moveSpeedBonus * StatMultiplier);

    public OwnedItem(long itemId)
    {
        instanceId = Guid.NewGuid().ToString();
        this.itemId = itemId;
        grade = Data.Grade;
    }

}
