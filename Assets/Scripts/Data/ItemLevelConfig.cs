using UnityEngine;


public static class ItemLevelConfig
{
    public const int MaxLevel = 10;

    private const int BaseCost = 1000;
    private const float StatGrowthPerLevel = 0.1f; 

    public static int GetLevelUpCost(int currentLevel) => BaseCost * currentLevel;

    public static float GetStatMultiplier(int level) => 1f + StatGrowthPerLevel * (level - 1);
}
