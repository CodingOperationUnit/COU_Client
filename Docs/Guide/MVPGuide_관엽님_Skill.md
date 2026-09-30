# MVP 작업 가이드: 스킬 (관엽님)

스킬은 전투 루프(`스킬 공격 → 처치 → 드롭 → 루팅`)의 시작점이다.
현재 dev에서 `SkillController`를 켜면 **첫 프레임부터 예외가 나고**, 예외를 피해도 발사·피격이 되지 않는다.
구조(SkillBase / Factory / Projectile / Pool)는 그대로 두고 연결 지점만 채운다.

## 작업 규칙
- `Feat/Skill`에서만 작업한다. 시작 전에 `origin/dev`를 머지한다.
- 수정 범위: `Scripts/Skills/*`, `Prefabs/Skills/*`, `Resources/Data/Skills/*`, `Scenes/TestScenes/SkillTestScene.unity`
- `Player.prefab`은 진영님 담당이다. `SkillController` 부착은 진영님에게 요청한다(7번).
- 레벨업 선택과 일시정지는 `Time.timeScale = 0`으로 게임을 멈춘다(BattleManager가 관리). 쿨다운과 발사체 이동은 `Time.deltaTime`을 써서 함께 멈추게 하고, `Time.timeScale`을 직접 바꾸지 않는다.

## 현재 막힌 지점

| # | 위치 | 문제 |
|---|---|---|
| A | `SkillController.cs:28-54` | legacy `UnityEngine.Input` 사용. 프로젝트가 `activeInputHandler: 1`(New Input System 전용)이라 매 프레임 `InvalidOperationException`이 난다 |
| B | `SkillController` ↔ `SkillFactory` | 테스트 키는 ID 11/21/31을 쓰고, Factory 키는 1/2/3이다. 항상 `null`이 반환된다 |
| C | `Resources/Data/Skills` | SkillData 에셋이 없다. `Get()`이 `null`을 반환해 `Initialize(null)`의 `data.Cooldown`에서 NRE가 난다 |
| D | `SkillData` | 발사체 프리팹 참조 필드가 없다. 스킬이 무엇을 쏠지 알 수 없다 |
| E | `SkillBase` | 발사 위치와 방향을 알 방법이 없다(순수 C# 클래스라 Transform이 없음) |
| F | `Skill_Revolver`, `Skill_Katana` | `FireLevel1~5`가 `NotImplementedException`이라 쿨다운이 돌아오면 예외가 난다 |
| G | `LinearProjectile` | 3D `OnTriggerEnter(Collider)`와 `Vector3.forward`를 쓴다. 2D에서는 호출되지 않고, 발사체가 z축(화면 안쪽)으로 날아간다 |
| H | 발사체 프리팹 | Collider2D가 `Is Trigger` 꺼짐이고 Rigidbody2D가 없다 |

## 1. 입력 (A)

- WASD로 `_dir`를 만드는 부분은 삭제한다. 방향은 플레이어 이동 방향을 쓴다(3번).
- 테스트용 키는 New Input System으로 바꾼다.

```csharp
using UnityEngine.InputSystem;

var kb = Keyboard.current;
if (kb != null && kb.numpad1Key.wasPressedThisFrame) EquipSkill(...);
```

## 2. 데이터 (B, C, D)

- **ID를 하나로 통일한다**: `SkillFactory` 키 = `SkillData.SkillId` = `EquipSkill` 인자. 체계(1/2/3이든 11/21/31이든)는 정해서 쓰면 된다.
- `SkillData`에 발사체 필드를 추가한다.

```csharp
public GameObject ProjectilePrefab;
```

- `Assets/Resources/Data/Skills/`에 `Create > Skill > SkillData`로 스킬별 에셋을 만든다. `SkillDataBase.Load()`가 이 경로를 읽는다.
- 에셋의 `SkillName`을 채운다. 레벨업 선택 창에 그대로 표시된다.
- `SkillFactory.Create`에서 data가 `null`이면 경고를 찍고 `null`을 반환한다. 에셋 누락이 NRE 대신 로그로 드러난다.

## 3. 발사 위치·방향 (E)

`SkillBase`가 자신을 가진 `SkillController`를 알게 한다. 컨트롤러는 Player 루트에 붙으므로 위치와 이동 방향을 제공할 수 있다.

```csharp
// SkillController
private PlayerMovement movement;
public Vector2 Origin => transform.position;
public Vector2 AimDirection => movement.FacingDirection;   // Awake에서 GetComponent
```

```csharp
// SkillBase
protected SkillController owner;
public void Initialize(SkillData data, SkillController owner) { ... }
```

`SkillFactory.Create(skillId, owner)`로 전달한다.

## 4. 타겟 탐색

`MonsterSpawner.Instance.SpawnedEnemies`(`IReadOnlyList<Enemy>`)를 쓴다. 승희님이 추가하는 API다.
`FindNearestTarget()`은 이 목록에서 `owner.Origin`과의 거리가 가장 짧은 것을 고른다. 없으면 `null`을 반환하고 `AimDirection`으로 쏜다.

## 5. 발사와 피격 (F, G)

**발사체 초기화에 방향을 추가한다.**

```csharp
public void Init(float damageValue, Vector2 direction)
```

**LinearProjectile**

```csharp
private void Update()
{
    transform.position += (Vector3)(direction * _speed * Time.deltaTime);
    ...
}

private void OnTriggerEnter2D(Collider2D other)
{
    if (!other.TryGetComponent(out Enemy enemy)) return;   // 플레이어 등은 무시
    ApplyDamage(enemy);
    ReturnToPool();
}
```

- `ApplyDamage(Enemy target)` → `target.Damaged(Mathf.RoundToInt(damage))`. `Enemy.Damaged`는 int를 받는다.
- 한 물리 스텝에서 적 두 마리에 동시에 닿으면 `ReturnToPool()`이 두 번 불린다. 관통하지 않는 발사체는 `_hasHit` 플래그로 첫 번째만 처리한다.

**MeleeProjectile (범위 판정)**

`SpawnedEnemies`를 순회하며 `_range` 안의 적에게 피해를 준다.
단, **역순 `for`로 순회한다.** `Damaged → Die → OnBeforeReturn`이 순회 도중 스포너 목록에서 그 적을 제거한다. 그래서 `foreach`를 쓰면 `InvalidOperationException`이 나고, 정순 `for`를 쓰면 다음 적을 건너뛴다.

```csharp
var enemies = MonsterSpawner.Instance.SpawnedEnemies;
for (int i = enemies.Count - 1; i >= 0; i--) { ... }
```

**FireLevel**

- MVP는 **스킬 하나의 Level 1이 실제로 발사되면 된다.**
- 나머지 레벨의 `throw new NotImplementedException()`은 `FireLevel1()` 호출로 바꿔 둔다. 예외만 막으면 된다.
- 실제로 발사되는 스킬은 전투 씬에서 시작 스킬로 자동 장착된다. 시작 스킬이 발사되지 않으면 몬스터를 잡을 수 없어 레벨업이 오지 않는다. 어떤 스킬인지 민영님에게 알린다.

## 6. 발사체 프리팹 (H)

| 컴포넌트 | 설정 |
|---|---|
| 기존 `CircleCollider2D` / `BoxCollider2D` | **Is Trigger 체크** |
| `Rigidbody2D` 추가 | **Body Type: Dynamic, Gravity Scale 0** |

Enemy는 Kinematic Rigidbody2D + Trigger로 맞춘다(승희님 담당). 2D 트리거는 한쪽이 Dynamic이어야 확실하게 발생하므로 발사체를 Dynamic으로 둔다.

## 7. Player 연결

- 진영님에게 `Player.prefab` 루트에 `SkillController`를 붙여 달라고 요청한다.

## 8. 스킬 레벨업 API (민영님 요청)

레벨업 선택 창(민영님 담당)에서 이미 장착한 스킬을 올릴 때 쓴다. 지금은 `EquipSkill`(새로 장착하면서 Lv1)만 있어서, 장착한 스킬의 레벨을 올릴 방법이 없다.

```csharp
// SkillBase
public const int MaxLevel = 5;   // Levelup/LevelDown의 Math.Clamp 상한도 이 상수로 교체

// SkillController
public const int MaxSkillSlots = 3;   // private → public

public bool LevelUpSkill(int skillId)
{
    SkillBase skill = _activeSkills.Find(s => s.SkillId == skillId);

    if (skill == null || skill.Level >= SkillBase.MaxLevel)
    {
        return false;
    }

    skill.Levelup();
    return true;
}
```

- BattleManager는 `ActiveSkills`, `SkillBase.SkillId`, `SkillBase.Level`, `SkillBase.MaxLevel`, `SkillController.MaxSkillSlots`를 읽어 후보를 고른다. `MaxSkillSlots`는 지금 `private`이므로 `public`으로 바꾸고, 나머지는 public으로 유지한다.
- 새 스킬 장착은 기존 `EquipSkill`을 그대로 쓴다.
- 슬롯이 가득 찼을 때의 정책(`EquipSkill`의 TODO)은 선택 창에서 가득 찬 상태의 새 스킬을 후보에서 빼는 것으로 처리한다. 지금처럼 실패를 반환하면 된다.

## 9. 풀 통합 (MVP 직후, 용진님 작업 후)

`ISkillPoolable`과 `IPoolable`은 시그니처가 같고, `SkillObjectPool`과 `ObjectPoolManager`도 로직이 같다.
용진님이 `ObjectPoolManager`를 수정하면(반납 시 `OnDespawn` 호출, 이중 반납 방지) 다음과 같이 정리한다.
- `SkillProjectile : IPoolable`로 바꾸고, `GameManager.ObjectPool.GetObject/ReturnObject`를 사용한다.
- `ReturnToPool()`의 수동 `OnDespawn()` 호출을 삭제한다(이중 호출 방지).
- `SkillObjectPool.cs`, `ISkillPoolable.cs`를 삭제한다.

## 테스트 (`SkillTestScene`)
`Player.prefab` + `MonsterSpawner`(Enemy.prefab 연결)를 배치 → Play → 스킬이 자동 발사되고, 몬스터가 맞아 죽고, 예외가 없는지 확인한다.

## 완료 체크리스트
- [ ] `UnityEngine.Input` 사용 0건
- [ ] ID 통일, SkillData 에셋 생성(`SkillName` 입력), `ProjectilePrefab` 연결
- [ ] `owner`로 위치·방향 획득
- [ ] `OnTriggerEnter2D` + `Enemy.Damaged` 적용
- [ ] Melee 역순 순회
- [ ] `NotImplementedException` 0건, 실제로 발사되는 스킬 ID를 민영님에게 알림
- [ ] 발사체 프리팹 Trigger + Dynamic(Gravity 0)
- [ ] `SkillBase.MaxLevel`, `SkillController.LevelUpSkill` 추가, `SkillController.MaxSkillSlots` public 변경 (민영님에게 알림)
- [ ] dev에 머지하고 진영님에게 `SkillController` 부착 요청
