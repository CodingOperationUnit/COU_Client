using UnityEngine;

// Enemy 기본 정보
public class EnemyData
{
    // 데이터 테이블에 들어갈 정보
    // 몬스터의 고정 능력치
    public int id;
    public int maxHp;
    public int exp;
    public float speed;
    public int attack;

    public EnemyData(int id, int maxHp, int exp, float speed, int attack)
    {
        this.id = id;
        this.maxHp = maxHp;
        this.exp = exp;
        this.speed = speed;
        this.attack = attack;
    }
}
