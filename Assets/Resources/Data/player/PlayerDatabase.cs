using UnityEngine;

public static class PlayerDatabase
{
    private const string BaseStatPath = "Data/player/player";

    private static PlayerBaseStatData baseStats;

    public static PlayerBaseStatData BaseStats
    {
        get
        {
            if (baseStats == null)
                Load();

            return baseStats;
        }
    }

    public static void Load()
    {
        if (baseStats != null) return;

        var textAsset = Resources.Load<TextAsset>(BaseStatPath);

        if (textAsset == null)
        {
            Debug.LogError("[PlayerDatabase] Resources/" + BaseStatPath + ".json을 찾을 수 없습니다. 기본값을 사용합니다.");
            baseStats = new PlayerBaseStatData();
        }
        else
        {
            baseStats = JsonUtility.FromJson<PlayerBaseStatData>(textAsset.text) ?? new PlayerBaseStatData();
        }

        baseStats.Sanitize();
    }
}