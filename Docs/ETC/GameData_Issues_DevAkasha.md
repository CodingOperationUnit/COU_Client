# 게임 데이터 이슈 - DevAkasha 작업분

`GameData_Tags.md`에서 확인한 이슈 중 DevAkasha가 작성한 코드에 해당하는 것만 모은다. 작성자는 `git blame` 기준이다. 괄호 안 번호는 `GameData_Tags.md`의 절 번호다.

## 1. 전투 진행
- `BattleManager`의 accountExpPerKill, accountExpPerSecond가 코드 주석에 임시값으로 표시돼 있다 (5.11)

## 2. 드롭·보상
- `Enemy.GiveReward`
  - 경험치 드롭이 ExpGem1로 고정돼 있어 `Monster.json`의 monsterExp와 `DropItem.json`의 ExpGem2~4를 쓰지 않는다(TODO) (5.12, 6.3)
  - 엘리트·보스의 상자 드롭 수가 하드코딩돼 있다. 몬스터별 드롭 테이블로 옮겨야 한다 (5.12)
- `Box.dropWeights` 상자 드롭 가중치가 const다. 몬스터별 드롭 테이블로 옮겨야 한다 (5.12)
- `PlayerInventory.ClaimBattleResult`
  - 보상상자 내용물이 하드코딩돼 있다. 상자 하나당 기본 등급 General 장비를 균등 랜덤으로 지급한다 (5.12)
  - 보상 반영을 클라이언트가 한다. 보상 지급이라 `SERVER_AUTH` 대상이다 (5.11)
  - accountExp만 더하고 accountLevel을 올리지 않는다. 계정 레벨별 필요 경험치가 정의돼 있지 않다 (5.4, 6.6)

## 3. 스킬
- `SkillBase`의 MaxLevel 5와 레벨당 배율(쿨타임 0.758, 피해 1.2)이 const이고 모든 스킬이 같은 배율을 쓴다. 스킬 레벨 테이블로 옮겨야 한다 (5.10)

## 4. 메인 UI
- `Grade`가 `ItemGrade`와 같은 의미로 중복 정의돼 있다. 어느 쪽을 남길지는 미정이다 (5.5, 6.2)
- `StageInfo`는 사용하는 코드가 없고 `StageData`와 중복이다. 주석 처리된 `MainTestDriver`에서만 참조한다 (6.2)
- `BattleTab.SetStaminaCost`를 부르는 코드가 없다. 전투 입장 스태미나 비용이 정의돼 있지 않고 입장할 때 차감하지 않는다 (5.7, 6.6)
- `TopBar.SetExp`는 비율(0~1)을 받는데 `MainUIAccountBinder`가 accountExp 원값을 넘긴다. accountExp가 1 이상이면 경험치 바가 가득 찬다 (5.4). `MainUIAccountBinder`는 DevAkasha 작성 코드가 아니다
- 진화(`EvolutionTab`, `EvolutionInfo`)
  - 진화 노드 데이터가 없다. `EvolutionNodeInfo` 구조체만 있고 value가 문자열이라 효과를 계산할 수 없다 (5.4, 6.6)
  - `EvolutionTab.NodesPerLevel` const는 진화 노드 테이블의 level 열에서 결정돼야 한다 (5.4)
  - 해금한 진화 노드 수와 DNA가 저장 구조에 없다 (5.3, 5.4, 6.4)
- 도전(`ChallengeInfo` 외)
  - 도전 조건·보상 데이터가 없다. `ChallengeCondition`은 설명만 있어 효과를 계산할 수 없다 (5.8, 6.6)
  - 보상 수령 여부(rewarded)가 `PlayerSaveData`에 없다 (5.8, 6.4)

## 5. 범위에서 뺀 부분
- 다른 작성자의 코드: `PlayerLuckTrain`, `PlayerHealth`, `PlayerLootReceiver`, `MonsterSpawner`의 기본 stageId, `LinearProjectile`의 lifeTime, `Skill_*`의 레벨별 효과
- DevAkasha 작성 코드 중 연출 값(`PRESENTATION`)만 있는 `DropItemManager`, `LuckTrainWindow`는 이슈가 없다
