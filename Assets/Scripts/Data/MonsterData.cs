using System;
using UnityEngine;

// 몬스터 등급. Monster.json의 "type" 문자열을 enum으로 바꿔서 쓴다.
public enum MonsterType { Normal, Elite, Boss, Box }

// 몬스터 기본 정보
[Serializable]
public class MonsterData
{
    public int monsterID;               // 1001~ 일반, 2001~ 엘리트, 3001~ 보스
    public string monsterName;
    public string type;                 // "Normal" / "Elite" / "Boss" / "Box"
    public int monsterMaxHealthPoint;
    public int monsterExp;
    public float monsterMoveSpeed;
    public int monsterAttackPoint;
    public string monsterAsset;         // Resources 기준 스프라이트 경로 (예: "Enemy/goblin")

    [NonSerialized] private MonsterType parsedType;
    public MonsterType Type => parsedType;

    // JsonDataManager에서 JSON을 읽은 직후 한 번 호출
    public void OnLoaded()
    {
        if (!Enum.TryParse(type, out parsedType))
            Debug.LogWarning($"[MonsterData] {monsterID}의 type \"{type}\"을(를) 알 수 없어 Normal로 처리합니다.");
    }
}