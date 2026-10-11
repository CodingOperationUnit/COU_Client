using System.Collections.Generic;
using UnityEngine;

// 전투 플레이어 스탯
// 최종 = (기본 + 진화 + 장비 합계) × (100 + 보너스%) / 100, 버림
// - 기본: PlayerBaseStat (정적 데이터)
// - 진화: 계정 데이터의 진화 레벨 × Evolution 증가량 (정적 데이터). 데이터가 없으면 0
// - 장비: 장착 장비의 레벨·등급 반영 스탯 합 (클라이언트 계산)
public class PlayerStats : MonoBehaviour
{
    private bool isCalculated;
    private int finalAtk;
    private int finalHp;
    private bool hasWeapon;
    private WeaponType equippedWeaponType;
    private string equippedWeaponName;
    private int startingSkillId;

    private static PlayerBaseStatData Base => GameManager.JsonData.PlayerBaseStatData;

    public int FinalAtk { get { EnsureCalculated(); return finalAtk; } }
    public int FinalHp { get { EnsureCalculated(); return finalHp; } }
    public bool HasWeapon { get { EnsureCalculated(); return hasWeapon; } }
    public WeaponType EquippedWeaponType { get { EnsureCalculated(); return equippedWeaponType; } }
    public string EquippedWeaponName { get { EnsureCalculated(); return equippedWeaponName; } }
    public int StartingSkillId { get { EnsureCalculated(); return startingSkillId; } }   // 0이면 시작 스킬 없음

    public int CriticalDamage => Base.playerBaseCriticalDamage;
    public int CriticalChance => Base.playerBaseCriticalChance;
    public int SkillDamage => Base.playerBaseSkillDamage;
    public float Speed => Base.playerBaseMoveSpeed;
    public float MaxSpeed => Base.playerBaseMaxMoveSpeed;
    public float LootRadius => Base.playerBaseLootRadius;

    private void Start()
    {
        EnsureCalculated();
    }

    // 누가 먼저 읽든 처음 한 번만 실행
    private void EnsureCalculated()
    {
        if (isCalculated) return;
        isCalculated = true;

        startingSkillId = WeaponSkillTable.NoSkill;

        if (!IsPlayerDataLoaded())
        {
            HandleNotLoggedIn();
            return;
        }

        var data = GameManager.PlayerData.currentData;
        var equipped = PlayerStatCalculator.GetEquipped(data);
        var stat = PlayerStatCalculator.Calculate(Base, EvolutionRules.Data, data.playerStat, equipped);

        if (!stat.HasEvolutionData)
            Debug.LogWarning("[PlayerStats] 진화 데이터가 없어 진화 스탯을 0으로 계산합니다.");

        finalAtk = stat.FinalAttack;
        finalHp = stat.FinalHp;

        LoadWeapon(equipped);

        Debug.Log("[PlayerStats] 공격력 " + finalAtk + " (기본 " + stat.BaseAttack + " + 진화 " + stat.EvolutionAttack + " + 장비 " + stat.EquipmentAttack + ")" +
                  " / 체력 " + finalHp + " (기본 " + stat.BaseHp + " + 진화 " + stat.EvolutionHp + " + 장비 " + stat.EquipmentHp + ")" +
                  " / 무기: " + (hasWeapon ? equippedWeaponName + " (" + equippedWeaponType + ")" : "없음") +
                  " / 시작 스킬: " + (startingSkillId == WeaponSkillTable.NoSkill ? "없음" : startingSkillId.ToString()));
    }
        
    private void LoadWeapon(List<OwnedItem> equipped)
    {
        var weapon = equipped.Find(item => item.Data.SlotType == EquipSlotType.Weapon);
        hasWeapon = weapon != null;
        if (!hasWeapon) return;

        equippedWeaponName = weapon.Data.itemName;
        startingSkillId = WeaponSkillTable.GetSkillByItem(weapon.itemId);

        if (!WeaponSkillTable.TryGetTypeBySkill(startingSkillId, out equippedWeaponType))
        {
            equippedWeaponType = WeaponType.Blunt;
            Debug.LogWarning("[PlayerStats] 무기 매핑이 없습니다: " + equippedWeaponName + " (" + weapon.itemId + "). 스킬 없이 시작, 무기 종류는 Blunt");
        }
    }

    // 로그인 없이 전투에 들어온 경우: 에러 후 로그인 화면 (예외 없음 정책)
    private void HandleNotLoggedIn()
    {
        Debug.LogError("[PlayerStats] 로그인 데이터가 없습니다. 로그인 화면으로 이동합니다.");
        GameManager.Scene.ChangeScene(GameConstants.SceneNames.LOGIN_SCENE);
    }

    private static bool IsPlayerDataLoaded()
    {
        var dataManager = GameManager.PlayerData;
        return dataManager != null && dataManager.isPlayerDataLoaded;
    }
}