using Newtonsoft.Json;

// 서버 플레이어 시스템 API 응답 형식 (필드 이름은 서버 응답과 동일하게 유지)

// statType 값 (서버 대문자 문자열 그대로 비교)
public static class PlayerStatType
{
    public const string Attack = "ATTACK";
    public const string Hp = "HP";
    public const string Defense = "DEFENSE";
    public const string PotionRecovery = "POTION_RECOVERY";
}

// PlayerStat (진화 강화 진행도)
public class PlayerStatDto
{
    public long playerId;
    public int playerStatAttackLevel;
    public int playerStatHpLevel;
    public int playerStatDefenseLevel;
    public int playerStatPotionRecoveryLevel;
}

// POST /api/players/me/evolution/upgrade
public class EvolutionUpgradeResponse
{
    public string upgradedStatType;
    public int evolutionStep;
    public string nextStatType;
    public int goldSpent;
    public PlayerStatDto playerStat;
}

// GET /api/players/me/stats
public class FinalStatsResponse
{
    public int finalAttack;
    public int finalHp;
    public int finalDefense;
    public int finalPotionRecovery;
    public int criticalDamage;
    public int criticalChance;
    public int skillDamage;
    public float moveSpeed;
    public float maxMoveSpeed;
    public float lootRadius;
    public StatBreakdown breakdown;
}

public class StatBreakdown
{
    [JsonProperty("base")]
    public StatParts baseStats;
    public StatParts equipment;
    public StatParts evolution;
}

public class StatParts
{
    public int attack;
    public int hp;
    public int defense;
    public int potionRecovery;
}