# MVP 작업 가이드: 몬스터 (승희님)

MVP 전투 루프는 `이동 → 몬스터 스폰 → 스킬 공격 → 몬스터 처치 → 경험치 드롭 → 루팅`이다.
몬스터는 이 루프의 한가운데 있다. 지금은 **플레이어를 공격하지 못하고, 스킬에 맞지 못하고, 죽어도 아무것도 떨어뜨리지 않는다.**
아래 1~6을 순서대로 끝내면 몬스터 쪽 연결이 완성된다.

## 작업 규칙
- `Feat/monster` 브랜치에서만 작업한다. 시작 전에 `origin/dev`를 머지한다.
- 수정해도 되는 파일: `Scripts/Enemy/*`, `Scripts/Mangers/MonsterSpawner.cs`, `Prefabs/Enemy/Enemy.prefab`, `Scenes/TestScenes/enemy.unity`
- 그 밖의 씬, 프리팹, `ProjectSettings`는 건드리지 않는다. 필요하면 담당자에게 요청한다.
- 레벨업 선택과 일시정지는 `Time.timeScale = 0`으로 게임을 멈춘다(BattleManager가 관리). 이동·스폰은 `Time.deltaTime`, 공격 간격은 `Time.time`을 써서 함께 멈추게 하고, `Time.timeScale`을 직접 바꾸지 않는다.

---

## 1. 테스트용 스페이스바 피격 코드 제거

`Enemy.Update()`의 스페이스바 코드는 **화면의 모든 몬스터**에 피해를 준다. 통합 씬에서는 문제가 되므로 지운다.

```csharp
// 삭제
if(Keyboard.current != null &&
    Keyboard.current.spaceKey.wasPressedThisFrame)
{
    Damaged(5);
}
```

`using UnityEngine.InputSystem;`도 함께 지운다.
테스트가 필요하면 `PlayerHealth`처럼 `[ContextMenu]`를 쓴다. 인스펙터에서 컴포넌트 우클릭으로 실행된다.

```csharp
[ContextMenu("TestDamaged")]
private void TestDamaged() => Damaged(5);
```

## 2. Monster.json 불러오기

지금 `Init()`은 `new EnemyData(101, 10, 5, 1.0f, 2)`로 하드코딩되어 있다.
`Resources/JsonFiles/Monster.json`을 읽도록 바꾼다. 인벤토리의 `ItemDatabase`(`Scripts/Data/ItemDataBase.cs`)와 같은 방식이다.

**2-1. `Scripts/Enemy/MonsterData.cs` 새로 만들기**

JSON 키 이름과 필드 이름이 **글자까지 같아야** `JsonUtility`가 값을 채운다.

```csharp
using System;

[Serializable]
public class MonsterData
{
    public int monsterID;
    public string monsterName;
    public int monsterMaxHealthPoint;
    public int monsterExp;
    public float monsterMoveSpeed;
    public int monsterAttackPoint;
}

[Serializable]
public class MonsterDataListWrapper
{
    public MonsterData[] datas;   // Monster.json 최상위 키가 "datas"
}
```

**2-2. `Scripts/Enemy/MonsterDatabase.cs` 새로 만들기**

```csharp
using System.Collections.Generic;
using UnityEngine;

public static class MonsterDatabase
{
    private static Dictionary<int, MonsterData> monsters;

    public static void Load()
    {
        if (monsters != null) return;

        // 경로에 Resources/와 확장자(.json)는 붙이지 않는다
        var json = Resources.Load<TextAsset>("JsonFiles/Monster").text;
        var wrapper = JsonUtility.FromJson<MonsterDataListWrapper>(json);

        monsters = new Dictionary<int, MonsterData>();
        foreach (var monster in wrapper.datas)
            monsters[monster.monsterID] = monster;
    }

    public static MonsterData Get(int monsterId)
    {
        Load();
        return monsters[monsterId];
    }
}
```

**2-3. `Enemy.Init()` 수정**

어떤 몬스터인지는 프리팹 인스펙터에서 정한다.

```csharp
[SerializeField] private int monsterId = 1001;

public void Init()
{
    MonsterData m = MonsterDatabase.Get(monsterId);
    data = new EnemyData(m.monsterID, m.monsterMaxHealthPoint, m.monsterExp,
                         m.monsterMoveSpeed, m.monsterAttackPoint);
    currentHp = data.maxHp;
    isDead = false;
    nextAttackTime = 0f;   // 3번에서 추가하는 필드
}
```

## 3. 플레이어 공격 (접촉 피해)

몬스터가 플레이어에 닿아 있는 동안 **일정 간격으로** 피해를 준다.
간격이 없으면 매 프레임(초당 60번) 피해가 들어가서 플레이어가 바로 죽는다.

**3-1. 플레이어 연결 방식 변경**

지금은 스포너가 `enemy.player = player;`로 필드에 바로 넣는다.
피해를 주려면 `PlayerHealth`도 필요하므로, 연결할 때 함께 찾아 둔다.

> `Init()`에서 찾으면 안 된다. `GetObject()` 안에서 `OnSpawn → Init`이 먼저 실행되고, 스포너가 `player`를 넣는 것은 그 **다음**이다. 그래서 `Init()` 시점에는 `player`가 비어 있다.

```csharp
// Enemy.cs
private PlayerHealth playerHealth;

public void SetTarget(GameObject target)
{
    player = target;
    playerHealth = target.GetComponent<PlayerHealth>();
}
```

```csharp
// MonsterSpawner.Spawn()
// enemy.player = player;   ← 이 줄을 아래로 교체
enemy.SetTarget(player);
```

**3-2. `Attack()` 구현**

```csharp
[SerializeField] private float attackRange = 0.6f;     // 이 거리 안이면 닿은 것으로 본다
[SerializeField] private float attackInterval = 1f;    // 공격 간격(초)
private float nextAttackTime;

public void Attack()
{
    if (isDead) { return; }
    if (Time.time < nextAttackTime) { return; }

    float distance = Vector2.Distance(transform.position, player.transform.position);
    if (distance > attackRange) { return; }

    playerHealth.GetDamage(data.attack);
    nextAttackTime = Time.time + attackInterval;
}
```

`Update()`에서 `Move();` 다음 줄에 `Attack();`을 호출한다.
플레이어가 이미 죽었거나 무적이면 `PlayerHealth.GetDamage`가 알아서 무시하므로 따로 확인할 필요가 없다.

## 4. 죽으면 경험치 드롭 + 처치 알림

**4-1. 드롭과 처치 이벤트**

`Die()`에서 **풀에 반납하기 전에** 드롭을 만들고 처치를 알린다. 반납하면 오브젝트가 꺼지므로 그 전에 처리한다.

```csharp
// Enemy.cs
public event Action<Enemy> OnDied;

public void Die()
{
    if (isDead) { return; }

    isDead = true;

    GameManager.DropItem.Spawn(DropItemType.ExpGem1, transform.position);
    OnDied?.Invoke(this);

    OnBeforeReturn?.Invoke(gameObject);
    GameManager.ObjectPool.ReturnObject(gameObject);
}
```

- MVP에서는 `ExpGem1` 고정으로 충분하다. `data.exp`에 따른 잼 등급 분기는 MVP 이후에 한다.
- 기존 `// TODO : exp 보상 구현` 주석은 지운다.

**4-2. 스포너에서 처치 이벤트 공개** (민영님 요청: 킬 수 집계용)

BattleManager가 몬스터 한 마리 한 마리를 구독하기는 어렵다. 그래서 스포너가 모아서 하나의 이벤트로 내보낸다.
구독 방식은 기존 `OnBeforeReturn`과 같다.

```csharp
// MonsterSpawner.cs  (파일 맨 위에 using System; 추가)
public event Action<Enemy> OnEnemyKilled;

// Spawn() 안, OnBeforeReturn 구독 코드 옆에 추가
enemy.OnDied -= OnEnemyDied;
enemy.OnDied += OnEnemyDied;

private void OnEnemyDied(Enemy enemy)
{
    OnEnemyKilled?.Invoke(enemy);
}
```

> 킬 수를 `OnBeforeReturn`으로 세지 않는 이유: `OnBeforeReturn`은 "풀로 돌아간다"는 뜻이다. 나중에 플레이어와 너무 멀어진 몬스터를 치우는 기능이 생기면, 죽지 않았는데도 반납된다. 처치와 반납은 별도의 이벤트로 둔다.

## 5. 스킬이 맞출 수 있게 하기

스킬 담당(관엽님)이 두 가지를 쓴다. 둘 다 몬스터 쪽에서 준비해 줘야 한다.

**5-1. `Enemy.prefab`에 물리 컴포넌트 추가** (지금은 SpriteRenderer와 스크립트뿐이다)

| 컴포넌트 | 설정 |
|---|---|
| `CircleCollider2D` | **Is Trigger 체크**, 반지름은 스프라이트 크기에 맞춘다 |
| `Rigidbody2D` | **Body Type: Kinematic** |

- 반드시 **2D** 컴포넌트(`...2D`)를 쓴다. 3D `SphereCollider`나 `Rigidbody`를 붙이면 충돌이 일어나지 않는다.
- Kinematic은 물리에 밀리지 않고 스크립트가 위치를 정한다는 뜻이다. 지금의 `transform.position` 이동 방식을 그대로 쓸 수 있다.

**5-2. 살아 있는 몬스터 목록 공개**

스킬이 "가장 가까운 몬스터"를 찾을 때 쓴다. `MonsterSpawner`에 한 줄 추가한다.

```csharp
public IReadOnlyList<Enemy> SpawnedEnemies => spawnedEnemies;
```

`IReadOnlyList`로 공개하면 다른 사람이 읽기만 하고 목록을 바꿀 수는 없다.

## 6. 테스트 (`enemy.unity`)

1. 씬에 `Player.prefab`을 놓고, `MonsterSpawner`의 Player 칸에 연결한다.
2. Play → 몬스터가 다가와서 닿으면 Console에 `현재 체력 : ...`이 1초 간격으로 찍히는지 확인한다.
3. Enemy 컴포넌트 우클릭 → `TestDamaged`를 두 번 실행해 몬스터를 죽인다.
   드롭까지 보려면 `DropItemTestScene`의 `DropItemManager` 오브젝트를 복사해 씬에 넣는다. 이 오브젝트에는 드롭 프리팹 목록이 연결되어 있다.
   없으면 `Spawn`에서 에러가 나지만 몬스터 쪽 문제는 아니다.

## 완료 체크리스트
- [ ] 스페이스바 피격 코드와 `using UnityEngine.InputSystem` 삭제
- [ ] `MonsterData`, `MonsterDatabase` 추가, `Init()`이 JSON 값을 사용
- [ ] `SetTarget()` 추가, 스포너가 이것을 호출
- [ ] `Attack()`이 간격을 두고 `PlayerHealth.GetDamage` 호출
- [ ] `Die()`에서 `ExpGem1` 드롭 + `OnDied` 발생
- [ ] `MonsterSpawner.OnEnemyKilled` 공개 (민영님에게 알림)
- [ ] `Enemy.prefab`에 `CircleCollider2D`(Trigger)와 `Rigidbody2D`(Kinematic)
- [ ] `MonsterSpawner.SpawnedEnemies` 공개
- [ ] dev에 머지하고, 5번이 끝났다고 관엽님에게 알림
