using UnityEngine;

// 진화 규칙 (서버와 같은 공식)
// - 진화 단계 = 4개 진화 레벨의 합
// - 다음 스탯 = evolutionStatOrder[단계 % 배열 길이]
// - 다음 비용 = evolutionBaseCost + evolutionCostPerStep × 단계
// - 최대 단계 = evolutionMaxStep > 0 이고 단계 ≥ evolutionMaxStep
// - 진화 스탯 = 스탯별 레벨 × 그 스탯의 evolutionIncreasePerLevel
public static class EvolutionRules
{
    public static EvolutionData Data => GameManager.JsonData != null ? GameManager.JsonData.EvolutionData : null;

    public static int GetStep(PlayerStatData stat)
    {
        if (stat == null) return 0;
        return stat.playerStatAttackLevel + stat.playerStatHpLevel + stat.playerStatDefenseLevel + stat.playerStatPotionRecoveryLevel;
    }

    public static int GetLevel(PlayerStatData stat, string statType)
    {
        if (stat == null) return 0;

        return statType switch
        {
            PlayerStatType.Attack => stat.playerStatAttackLevel,
            PlayerStatType.Hp => stat.playerStatHpLevel,
            PlayerStatType.Defense => stat.playerStatDefenseLevel,
            PlayerStatType.PotionRecovery => stat.playerStatPotionRecoveryLevel,
            _ => 0
        };
    }

    public static int GetIncreasePerLevel(EvolutionData data, string statType)
    {
        if (data == null) return 0;

        int index = System.Array.IndexOf(data.evolutionStatOrder, statType);
        return index < 0 ? 0 : data.evolutionIncreasePerLevel[index];
    }

    public static int GetBonus(EvolutionData data, PlayerStatData stat, string statType)
        => GetLevel(stat, statType) * GetIncreasePerLevel(data, statType);

    public static string GetNextStatType(EvolutionData data, int step)
    {
        if (data == null || data.evolutionStatOrder.Length == 0) return null;
        return data.evolutionStatOrder[step % data.evolutionStatOrder.Length];
    }

    public static int GetNextCost(EvolutionData data, int step)
        => data == null ? 0 : data.evolutionBaseCost + data.evolutionCostPerStep * step;

    public static bool IsMaxStep(EvolutionData data, int step)
        => data != null && data.evolutionMaxStep > 0 && step >= data.evolutionMaxStep;
}
