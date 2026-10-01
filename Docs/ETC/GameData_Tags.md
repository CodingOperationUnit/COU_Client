# 게임 데이터 태그

기획자 없이 역설계하고 있어서 게임 데이터 선언이 JSON, 코드 상수, 인스펙터, 테스트 드라이버에 흩어져 있다. 백엔드를 붙이기 전에 값의 성격을 구분하려고 태그를 정의하고, 현재 값을 개념 단위로 분류한다. 각 값에는 현행 상태의 태그와 구조상 이상적인 상태의 태그를 함께 적는다.

태그는 코드에 붙이지 않고 이 문서에서만 관리한다. 씬 이름, 리소스 경로, 성능 튜닝 상수 같은 기술 상수는 게임 데이터가 아니므로 분류하지 않는다.

## 1. 1차 태그
값 하나에 하나만 붙인다.

| 태그 | 정의 | 수명 | 백엔드 연동 시 |
|---|---|---|---|
| `DESIGN_TABLE` 기획-테이블 | ID로 구분되는 행 단위 정의 데이터 | 패치 단위 | 서버와 클라이언트가 같은 테이블을 가진다. 버전 관리가 필요하다 |
| `DESIGN_CONST` 기획-상수 | ID 없이 하나뿐인 수치나 공식 계수 | 패치 단위 | 서버의 검증 공식에 쓴다 |
| `ACCOUNT` 계정 | 인증·식별 정보 | 영구 | 인증 서버로 옮긴다. 클라이언트에 저장하지 않는다 |
| `USER` 유저 | 유저별로 쌓이는 진행 상태 | 영구 | DB 저장 대상이다 |
| `SESSION` 세션 | 전투 한 번 동안만 쓰는 값 | 전투가 끝나면 버린다 | 저장하지 않고 결과만 서버로 보낸다 |
| `CLIENT_PREF` 로컬설정 | 기기별 환경 설정 | 기기에 영구 저장 | 서버가 필요 없다. 현재 해당 값이 없다 |
| `PRESENTATION` 연출 | 게임 결과에 영향이 없는 표현용 수치 | 빌드 단위 | 관리 대상에서 빼고 인스펙터에 둔다 |
| `DEBUG` 테스트 | 개발용 임시값 | — | 빌드에서 뺀다 |

## 2. 판별 순서
1. 개발용 임시값이면 `DEBUG`
2. 게임 결과에 영향이 없으면 `PRESENTATION`
3. 모든 유저에게 같은 값이면, ID로 구분되는 행은 `DESIGN_TABLE`, 아니면 `DESIGN_CONST`
4. 유저마다 다른 값이면, 인증 정보는 `ACCOUNT`, 기기 설정은 `CLIENT_PREF`, 전투가 끝나면 버리는 값은 `SESSION`, 나머지는 `USER`

## 3. 권위 태그
`USER`, `SESSION`에만 붙인다.

| 태그 | 정의 | 대상 |
|---|---|---|
| `SERVER_AUTH` | 서버가 계산하고 클라이언트는 결과만 받는다 | 재화 증감, 장비 획득·강화 비용 차감, 뽑기 결과, 보상 지급 |
| `CLIENT_AUTH` | 클라이언트가 계산하고 서버는 결과를 받거나 사후 검증한다 | 이동, 전투 중 판정, 생존 시간, 킬 수 |

## 4. 현행 태그와 이상 태그
개념 단위 목록에서는 값마다 태그를 두 개 붙인다.

| 열 | 기준 |
|---|---|
| 현행 | 지금 코드에서 값이 선언되고 쓰이는 방식 |
| 이상 | 구조를 정리했을 때 값이 가져야 할 태그. 2장의 판별 순서를 따른다 |

현행 태그는 다음 기준으로 붙인다.
- 테스트 드라이버의 더미 값과 임시 코드는 `DEBUG`
- 게임 결과에 영향을 주는 값이 인스펙터나 코드 상수로 하나씩 들어 있으면 `DESIGN_CONST`
- 서버가 없어 현재는 모든 값을 클라이언트가 계산한다. 그래서 현행 태그에는 권위 태그를 붙이지 않는다

표기
- `삭제`: 다른 값으로 대체되거나 개발용이라서 이상 상태에서는 없어야 하는 값
- `—`: 아직 정의되지 않아 현재 없는 값
- 이상 태그의 1차 태그가 현행과 다르면 굵게 표시한다. 권위 태그가 붙은 것만으로는 다르다고 보지 않는다

## 5. 개념 단위별 데이터
위치의 표기는 다음과 같다.
- **JSON**: Resources 아래 JSON 파일
- **const**: 코드 상수
- **인스펙터**: `[SerializeField]` 값
- **더미**: 테스트 드라이버가 넣는 값
- **없음**: 아직 정의되지 않은 값

### 5.1 계정
| 값 | 위치 | 현행 | 이상 | 비고 |
|---|---|---|---|---|
| playerID, password | `LocalAccountData` → `accounts.json` | `ACCOUNT` | `ACCOUNT` | 현재 password를 평문으로 로컬에 저장한다 |
| 아이디 규칙(영문·숫자·밑줄 3~20자) | `LocalLoginManager.SignUp` 정규식 | `DESIGN_CONST` | `DESIGN_CONST` | 서버와 같은 규칙을 써야 한다 |

### 5.2 플레이어 기본 스탯
| 값 | 위치 | 현행 | 이상 | 비고 |
|---|---|---|---|---|
| attack, hp, criticalDamage, criticalChance, skillDamage, moveSpeed, maxMoveSpeed, lootRadius | `Resources/Data/player/player.json` | `DESIGN_CONST` | `DESIGN_CONST` | |
| 최종 스탯 공식 `(기본 + 장비) × (100 + 보너스%) / 100` | `PlayerStats.CalculateStat` | `DESIGN_CONST` | `DESIGN_CONST` | 장비 보너스%는 `ItemData`에 필드가 없어 0으로 계산한다 |
| maxLives 2, invulnerableTime 0.5 | `PlayerHealth` 인스펙터 | `DESIGN_CONST` | `DESIGN_CONST` | |
| inventoryWeaponType | `PlayerStats` 인스펙터 | `DEBUG` | `삭제` | `ItemData`에 무기 종류 필드가 생기면 대체된다 |
| dummyEquipments | `PlayerStats` 인스펙터(`DummyEquipment`) | `DEBUG` | `삭제` | 인벤토리가 없는 씬에서만 쓴다 |

### 5.3 재화
| 값 | 위치 | 현행 | 이상 | 비고 |
|---|---|---|---|---|
| gold, gem | `PlayerSaveData` | `USER` | `USER` · `SERVER_AUTH` | 현재 이 값을 읽거나 쓰는 코드가 없다(6.1) |
| gold, gem | `PlayerInventory.SaveData` | `USER` | `삭제` | `PlayerSaveData`로 일원화한다(6.1) |
| stamina | `PlayerSaveData` | `USER` | `USER` · `SERVER_AUTH` | |
| 초기 재화(gold 0, gem 0, stamina 60) | `PlayerSaveData.CreateDefault` | `DESIGN_CONST` | `DESIGN_CONST` | |
| 최대 스태미나, 회복 주기 | 없음 | `—` | **`DESIGN_CONST`** | `TopBar`는 더미 `(67, 60)`을 표시한다 |
| 스태미나 회복 기준 시각 | 없음 | `—` | **`USER`** · `SERVER_AUTH` | 회복을 계산하려면 필요하다 |
| DNA | `EvolutionInfo.dna` 더미 | `DEBUG` | **`USER`** · `SERVER_AUTH` | `PlayerSaveData`에 없다 |
| gold 500000 | `PlayerInventory` 인스펙터 | `DEBUG` | `삭제` | |

### 5.4 계정 성장
| 값 | 위치 | 현행 | 이상 | 비고 |
|---|---|---|---|---|
| accountLevel, accountExp | `PlayerSaveData` | `USER` | `USER` · `SERVER_AUTH` | |
| 계정 레벨별 필요 경험치 | 없음 | `—` | **`DESIGN_TABLE`** | |
| 진화 노드(level, name, value, description, cost) | `MainTestDriver` 더미(`EvolutionNodeInfo`) | `DEBUG` | **`DESIGN_TABLE`** | 골드 노드와 DNA 노드 두 종류. value가 문자열이라 효과를 계산할 수 없다 |
| 레벨당 골드 노드 수 3 | `EvolutionTab.NodesPerLevel` const | `DESIGN_CONST` | `삭제` | 진화 노드 테이블의 level 열에서 결정된다 |
| 해금한 진화 노드 수 | `EvolutionInfo` 더미 | `DEBUG` | **`USER`** · `SERVER_AUTH` | `PlayerSaveData`에 없다 |
| 진화 트리 간격·여백 | `EvolutionTab` 인스펙터 | `PRESENTATION` | `PRESENTATION` | |

### 5.5 장비
| 값 | 위치 | 현행 | 이상 | 비고 |
|---|---|---|---|---|
| itemId, itemName, description, iconPath, slotType, grade, hpBonus, attackBonus, moveSpeedBonus, gradeSkills | `Resources/Data/items.json` | `DESIGN_TABLE` | `DESIGN_TABLE` | gradeSkills가 문자열이라 효과를 계산할 수 없다 |
| 부위(`EquipSlotType`), 장비 등급(`ItemGrade`) | `EquipDefine.cs` enum | `DESIGN_CONST` | `DESIGN_CONST` | |
| UI 등급(`Grade`) | `UI/Grade.cs` enum | `DESIGN_CONST` | `DESIGN_CONST` | `ItemGrade`와 하나로 통합해야 한다. 어느 쪽을 남길지는 미정이다(6.3) |
| 무기 종류(`WeaponType`) | `WeaponType.cs` enum | `DESIGN_CONST` | `DESIGN_CONST` | 임시 정의. `ItemData`에 필드가 없다 |
| 강화: MaxLevel 10, BaseCost 1000, StatGrowthPerLevel 0.1 | `ItemLevelConfig` const | `DESIGN_CONST` | `DESIGN_CONST` | 비용 = `1000 × 현재 레벨`, 배율 = `1 + 0.1 × (레벨 - 1)` |
| 보유 장비(instanceId, itemId, level, isEquipped) | `PlayerSaveData.equipmentList` | `USER` | `USER` · `SERVER_AUTH` | 현재 이 값을 읽거나 쓰는 코드가 없다(6.1) |
| 보유 장비(instanceId, itemId, level, isEquipped) | `PlayerInventory.SaveData.items` | `USER` | `삭제` | `PlayerSaveData`로 일원화한다(6.1) |

### 5.6 상점
| 값 | 위치 | 현행 | 이상 | 비고 |
|---|---|---|---|---|
| 보석 상품 지급량(gemAmount) | `ShopGemCard` 인스펙터 | `DESIGN_CONST` | **`DESIGN_TABLE`** | 상품 ID로 구분되는 상품 테이블. 현재는 결제 없이 바로 지급한다 |
| 골드 상품 가격·지급량(gemCost, goldAmount) | `ShopGoldCard` 인스펙터 | `DESIGN_CONST` | **`DESIGN_TABLE`** | 상품 테이블 |
| 지원품 상자 가격·등급 범위(gemCost, minGrade, maxGrade) | `ShopSupplyBoxCard` 인스펙터 | `DESIGN_CONST` | **`DESIGN_TABLE`** | 상품 테이블 |
| 지원품 상자 확률 | 없음 | `—` | **`DESIGN_TABLE`** | 현재는 등급 범위 안에서 균등 랜덤이다 |
| 구매·뽑기 결과 | 클라이언트에서 계산 | `USER` | `USER` · `SERVER_AUTH` | |
| 카드·등급 색상 | 각 카드 인스펙터 | `PRESENTATION` | `PRESENTATION` | |

### 5.7 스테이지·스폰
| 값 | 위치 | 현행 | 이상 | 비고 |
|---|---|---|---|---|
| stageID, stageName, duration | `Resources/JsonFiles/Stage.json` | `DESIGN_TABLE` | `DESIGN_TABLE` | 불러오지만 아무 데서도 조회하지 않는다(6.4) |
| 스테이지 설명 | `StageInfo` 더미 | `DEBUG` | **`DESIGN_TABLE`** | 스테이지 테이블의 열 |
| 전투 입장 스태미나 비용 | `MainTestDriver` 더미(5) | `DEBUG` | **`DESIGN_TABLE`** | 스테이지 테이블의 열 |
| 스테이지 클리어 보상 | 없음 | `—` | **`DESIGN_TABLE`** | |
| 스폰 이벤트(spawnEventID, stageID, eventType, startTime, endTime, monsterID, spawnInterval, spawnCount, repeat) | `Resources/JsonFiles/Spawn.json` | `DESIGN_TABLE` | `DESIGN_TABLE` | 불러오지만 아무 데서도 조회하지 않는다(6.4) |
| spawnInterval 1 | `MonsterSpawner` 인스펙터 | `DESIGN_CONST` | `삭제` | `Spawn.json`의 spawnInterval로 대체된다(6.3) |
| spawnRadius 10 | `MonsterSpawner` 인스펙터 | `DESIGN_CONST` | `DESIGN_CONST` | |
| 스테이지 기록(stageID, isCleared, bestSurvivalSeconds) | `PlayerSaveData.stageRecordList` | `USER` | `USER` · `CLIENT_AUTH` | 현재 이 값을 읽거나 쓰는 코드가 없다(6.1) |

### 5.8 도전
| 값 | 위치 | 현행 | 이상 | 비고 |
|---|---|---|---|---|
| 챕터별 도전 조건(name, description) | `MainTestDriver` 더미(`ChallengeCondition`) | `DEBUG` | **`DESIGN_TABLE`** | 설명만 있고 효과를 계산할 수 없다 |
| 도전 보상(grade, amount, techPart) | `MainTestDriver` 더미(`ChallengeReward`) | `DEBUG` | **`DESIGN_TABLE`** | |
| 보상 수령 여부(rewarded) | `MainTestDriver` 더미 | `DEBUG` | **`USER`** · `SERVER_AUTH` | `PlayerSaveData`에 없다 |

### 5.9 몬스터
| 값 | 위치 | 현행 | 이상 | 비고 |
|---|---|---|---|---|
| monsterID, monsterName, type, monsterMaxHealthPoint, monsterExp, monsterMoveSpeed, monsterAttackPoint, monsterAsset | `Resources/JsonFiles/Monster.json` | `DESIGN_TABLE` | `DESIGN_TABLE` | 불러오지만 아무 데서도 조회하지 않는다(6.4) |
| 보스 공격(bossAttackID, monsterID, attackType, cooldown, range, damage) | `Resources/JsonFiles/BossAttack.json` | `DESIGN_TABLE` | `DESIGN_TABLE` | 불러오지만 아무 데서도 조회하지 않는다(6.4) |
| `EnemyData(101, 10, 5, 1.0, 2)` | `Enemy.Init` 하드코딩 | `DEBUG` | `삭제` | `Monster.json`으로 대체된다 |
| 몬스터 현재 HP | `Enemy.currentHp` | `SESSION` | `SESSION` · `CLIENT_AUTH` | |

### 5.10 스킬
| 값 | 위치 | 현행 | 이상 | 비고 |
|---|---|---|---|---|
| ID, Name, Type, Cooldown, Speed, Damage, Range, Description | `Resources/JsonFiles/Skill.json` | `DESIGN_TABLE` | `DESIGN_TABLE` | 현재 실제로 읽는 값은 Cooldown뿐이다 |
| 투사체 속도(_speed 10), 근접 범위(_range 1.5) | `LinearProjectile`, `MeleeProjectile` 인스펙터 | `DESIGN_CONST` | `삭제` | `Skill.json`의 Speed, Range로 대체된다(6.3) |
| 투사체 수명(_lifeTime 3), 근접 지속 시간(_effectDuration 0.3) | `LinearProjectile`, `MeleeProjectile` 인스펙터 | `DESIGN_CONST` | **`DESIGN_TABLE`** | 스킬 테이블의 열 |
| 최대 스킬 슬롯 3 | `SkillController.MaxSkillSlots` const | `DESIGN_CONST` | `DESIGN_CONST` | |
| 스킬 레벨별 수치, 최대 레벨 | 없음 | `—` | **`DESIGN_TABLE`** | 행운열차 UI는 더미 `Lv.n`을 표시한다 |
| 전투 중 보유 스킬·레벨 | `SkillController._activeSkills` | `SESSION` | `SESSION` · `CLIENT_AUTH` | |

### 5.11 전투 진행
| 값 | 위치 | 현행 | 이상 | 비고 |
|---|---|---|---|---|
| 경과 시간, 킬 수, 전투 레벨, 경험치, 대기 레벨업 수 | `BattleManager` 필드 | `SESSION` | `SESSION` · `CLIENT_AUTH` | |
| 레벨업 필요 경험치(base 20, 레벨당 +6) | `BattleManager` 인스펙터 | `DESIGN_CONST` | `DESIGN_CONST` | 필요 경험치 = `20 + 6 × (레벨 - 1)` |
| 승리 시간 900 | `BattleManager.victorySeconds` 인스펙터 | `DESIGN_CONST` | `삭제` | `Stage.json`의 duration으로 대체된다(6.3) |
| 현재 HP, 남은 목숨 | `PlayerHealth` | `SESSION` | `SESSION` · `CLIENT_AUTH` | |
| 전투 결과(승패, 생존 시간, 킬 수) | `BattleResultWindow` 표시만 | `SESSION` | `SESSION` · `CLIENT_AUTH` | 서버로 보낼 결과다. 현재 스테이지 기록과 보상에 반영하는 경로가 없다 |
| 행운열차 결과(선택 슬롯, 골드) | `HUDTestDriver` 더미 | `DEBUG` | **`SESSION`** · `CLIENT_AUTH` | |
| 행운열차 확률·골드량 | 없음 | `—` | **`DESIGN_TABLE`** | |
| 행운열차 회전 시간·바퀴 수·공개 간격 | `LuckTrainWindow` 인스펙터 | `PRESENTATION` | `PRESENTATION` | |

### 5.12 드롭 아이템
| 값 | 위치 | 현행 | 이상 | 비고 |
|---|---|---|---|---|
| 드롭 종류(`DropItemType`) | `DropItemType.cs` enum | `DESIGN_CONST` | `DESIGN_CONST` | ExpGem1~4, Gold1~4, LuckyBox, RewardBox, Potion, Magnet, Bomb |
| 경험치 잼·골드 단계별 획득량 | 없음 | `—` | **`DESIGN_TABLE`** | |
| 몬스터별 드롭 종류·확률 | 없음 | `—` | **`DESIGN_TABLE`** | `Enemy.Die`의 경험치 지급은 TODO 상태다 |
| 폭탄 피해량·범위 | 없음 | `—` | **`DESIGN_CONST`** | |
| 포션 회복량 300 | `PlayerLootReceiver` 인스펙터 | `DESIGN_CONST` | `DESIGN_CONST` | 코드 주석에 임시값으로 표시돼 있다 |
| 흡수 연출(recoilDistance, recoilDuration, chaseStartSpeed, chaseAcceleration, arriveRadius) | `DropItemManager` 인스펙터 | `PRESENTATION` | `PRESENTATION` | |

## 6. 발견된 문제
분류하면서 확인한 사실만 적는다.

### 6.1 `USER` 데이터 이중 저장
| 저장 구조 | 파일 | 내용 |
|---|---|---|
| `PlayerSaveData` | `Players/{playerID}.json` | gold, gem, stamina, 계정 레벨·경험치, equipmentList, stageRecordList |
| `PlayerInventory.SaveData` | `inventory_save.json` | gold, gem, items |

gold, gem, 보유 장비가 계정과 무관한 파일에 따로 저장된다. 상점, 장비 화면, `PlayerStats`는 `PlayerInventory`만 쓴다. `PlayerSaveData`의 gold, gem, equipmentList, stageRecordList를 읽거나 쓰는 코드는 없다.

### 6.2 기획 값의 저장 방식이 네 가지다
- JSON이 두 폴더에 나뉘어 있다: `Resources/JsonFiles`(Monster, BossAttack, Spawn, Stage, Skill), `Resources/Data`(items, player)
- 코드 상수: `ItemLevelConfig`, `MaxSkillSlots`, `NodesPerLevel`
- 인스펙터: `BattleManager`, `PlayerHealth`, `MonsterSpawner`, 상점 카드, 투사체 프리팹
- 테스트 드라이버 더미: 진화, 도전, 스테이지 설명, 행운열차

### 6.3 같은 의미의 중복 정의
| 의미 | 정의 1 | 정의 2 |
|---|---|---|
| 전투 시간 | `Stage.json` duration 150 | `BattleManager.victorySeconds` 900 |
| 스폰 간격 | `Spawn.json` spawnInterval | `MonsterSpawner.spawnInterval` 1 |
| 스킬 속도·범위 | `Skill.json` Speed, Range | 투사체 프리팹 _speed, _range |
| 등급 | `ItemGrade`(General, Super, Rare) | `Grade`(Common, Good, Rare, Elite, Epic) |

### 6.4 불러오지만 쓰지 않는 테이블
`JsonDataManager`는 Monster, BossAttack, Spawn, Stage를 불러오지만 다른 클래스에서 조회하지 않는다. 몬스터는 `Enemy.Init`의 하드코딩 값을 쓰고, 스폰은 `MonsterSpawner`의 고정 간격으로 돈다.

### 6.5 저장 구조에 없는 `USER` 값
DNA, 해금한 진화 노드 수, 도전 보상 수령 여부, 스태미나 회복 기준 시각

### 6.6 정의되지 않은 기획 데이터
계정 레벨별 필요 경험치, 최대 스태미나와 회복 주기, 스테이지 클리어 보상, 경험치 잼·골드 획득량, 몬스터 드롭 테이블, 스킬 레벨별 수치, 지원품 상자·행운열차 확률

### 6.7 계정 정보 평문 저장
`accounts.json`에 password가 평문으로 저장된다.
