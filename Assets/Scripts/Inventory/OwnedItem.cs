using System;
using UnityEngine;


[Serializable]
public class OwnedItem
{
    public string instanceId;
    public long itemId;
    public bool isEquipped;
    public int level = 1;

    public ItemData Data => ItemDatabase.Get(itemId);
    public ItemGrade Grade => Data.Grade;

    public bool CanLevelUp => level < ItemLevelConfig.MaxLevel;
    public int NextLevelUpCost => ItemLevelConfig.GetLevelUpCost(level);

    public int ScaledHp => Mathf.RoundToInt(Data.hpBonus * ItemLevelConfig.GetStatMultiplier(level));
    public int ScaledAttack => Mathf.RoundToInt(Data.attackBonus * ItemLevelConfig.GetStatMultiplier(level));
    public int ScaledMoveSpeed => Mathf.RoundToInt(Data.moveSpeedBonus * ItemLevelConfig.GetStatMultiplier(level));

    public OwnedItem(long itemId)
    {
        instanceId = Guid.NewGuid().ToString();
        this.itemId = itemId;
    }

}
