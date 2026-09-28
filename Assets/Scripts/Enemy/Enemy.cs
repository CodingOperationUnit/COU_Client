using UnityEngine;
using UnityEngine.InputSystem;

public class Enemy : MonoBehaviour
{
    public EnemyData data;

    // 몬스터의 개별 상태
    public int currentHp;
    public bool isDead;

    // 추적할 Player
    public GameObject player;

    // 몬스터 초기화
    public void Init()
    {
        // 몬스터 임시값
        // TODO : 구글 시트에서 값 불러왔을 때 다시 구현해야함
        data = new EnemyData(101, 10, 5, 1.0f, 2);
        currentHp = data.maxHp;
        isDead = false;
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        // 몬스터가 죽은 상태면 return
        if (isDead) { return; }

        Move();

        // 피격 임시 로직
        if(Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Damaged(5);
        }
    }

    // 몬스터의 움직임
    public void Move()
    {
        // 몬스터가 죽은 상태면 return
        if (isDead) { return; }

        // player 방향 찾기
        Vector3 direction = player.transform.position - this.transform.position;
        direction.Normalize();

        this.transform.position += direction * data.speed * Time.deltaTime;
    }

    // 몬스터의 공격
    public void Attack()
    {
        // 몬스터가 죽은 상태면 return
        if (isDead) { return; }

        // TODO :  공격 로직 구현
        //player.hp -= data.attack;
    }

    // 몬스터의 피격
    public void Damaged(int damage)
    {
        // 몬스터가 죽은 상태면 return
        if (isDead) { return; }

        currentHp -= damage;

        // 몬스터가 죽으면
        if (currentHp <= 0)
        {
            Die();
        }
    }

    // 몬스터가 죽었을 때
    public void Die()
    {
        isDead = true;

        // 몬스터가 죽은 상태면 비활성화
        gameObject.SetActive(false);

        // TODO : exp 보상 구현
        //player.exp += data.exp;
    }
}
