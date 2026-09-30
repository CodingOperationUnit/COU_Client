using System;
using UnityEngine;

// Resources/Data/player.json의 형식. 필드 이름이 JSON 키와 같아야 한다.
[Serializable]
public class PlayerBaseStatData
{
    // JSON에 없는 항목은 아래 기본값이 사용된다.
    public int attack = 100;
    public int hp = 1000;
    public int criticalDamage = 200;
    public int criticalChance = 5;
    public int skillDamage = 100;
    public float moveSpeed = 9f;
    public float maxMoveSpeed = 16f;
    public float lootRadius = 2f;   // 추가

    // 파일 값은 Inspector의 [Min], [Range]가 적용되지 않으므로 직접 보정
    public void Sanitize()
    {
        attack = Mathf.Max(1, attack);
        hp = Mathf.Max(1, hp);
        criticalDamage = Mathf.Max(1, criticalDamage);
        skillDamage = Mathf.Max(1, skillDamage);
        moveSpeed = Mathf.Max(0.1f, moveSpeed);
        maxMoveSpeed = Mathf.Max(moveSpeed, maxMoveSpeed);
        lootRadius = Mathf.Max(0f, lootRadius);

        int clampedChance = Mathf.Clamp(criticalChance, 0, 100);
        if (clampedChance != criticalChance)
        {
            Debug.LogWarning("[PlayerBaseStatData] criticalChance는 0~100이어야 합니다. " + criticalChance + " → " + clampedChance);
            criticalChance = clampedChance;
        }
    }
}