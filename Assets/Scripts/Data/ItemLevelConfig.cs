using UnityEngine;


public static class ItemLevelConfig
{
    public const int MaxLevel = 10;

    private const int BaseCost = 1000;
    private const float StatGrowthPerLevel = 0.1f;


    // 장비 합성 갯수
    public const int SynthesisMaterialCount = 2;


    private static readonly float[] GradeStatMultiplier = { 1f, 1.75f, 2.75f };

    public static int GetLevelUpCost(int currentLevel) => BaseCost * currentLevel;

    public static float GetStatMultiplier(int level) => 1f + StatGrowthPerLevel * (level - 1);

    public static float GetGradeMultiplier(ItemGrade grade) => GradeStatMultiplier[(int)grade];
}
