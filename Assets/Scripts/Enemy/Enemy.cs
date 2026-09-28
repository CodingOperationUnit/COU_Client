using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Enemy : MonoBehaviour, IPoolable
{
    public EnemyData data;

    // 몬스터의 개별 상태
    public int currentHp;
    public bool isDead;

    // 추적할 Player
    public GameObject player;

    // IPoolable : 풀로 돌아가기 직전에 알림 (스포너가 구독해서 사용)
    public event Action<GameObject> OnBeforeReturn;

    // 몬스터 초기화
    public void Init()
    {
        // 몬스터 임시값
        // TODO : 구글 시트에서 값 불러왔을 때 다시 구현해야함
        data = new EnemyData(101, 10, 5, 1.0f, 2);
        currentHp = data.maxHp;
        isDead = false;
    }

    // IPoolable: 풀에서 꺼내질 때마다 호출됨
    public void OnSpawn()
    {
        Init();
    }

    // IPoolable: 풀로 돌아갈 때 정리
    public void OnDespawn()
    {
        // TODO : 풀로 돌아갈 때 정리할 내용
        // 추후 공격 코루틴, 상태이상, 타이머 등이 생기면 여기서 정리
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
        // 몬스터가 죽은 상태면 return
        if (isDead) { return; }

        isDead = true;

        // 반납 알림
        OnBeforeReturn?.Invoke(gameObject);
        // 몬스터가 죽은 상태면 비활성화(Pool로 반납)
        GameManager.ObjectPool.ReturnObject(gameObject);

        // TODO : exp 보상 구현 Pool 이용하기
        //player.exp += data.exp;
    }
}
