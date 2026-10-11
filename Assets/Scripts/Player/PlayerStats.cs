using System;
using System.Collections.Generic;
using System.Linq;
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
        var equipped = GetEquipped(data);

        // 장비
        int equipAtk = equipped.Sum(item => item.ScaledAttack);
        int equipHp = equipped.Sum(item => item.ScaledHp);

        // 진화
        var evolution = EvolutionRules.Data;
        if (evolution == null)
            Debug.LogWarning("[PlayerStats] 진화 데이터가 없어 진화 스탯을 0으로 계산합니다.");

        int evolutionAtk = EvolutionRules.GetBonus(evolution, data.playerStat, PlayerStatType.Attack);
        int evolutionHp = EvolutionRules.GetBonus(evolution, data.playerStat, PlayerStatType.Hp);

        // 장비 데이터에 공격력·체력 보너스 % 필드가 없어 0%로 계산 (필드 추가 시 교체)
        finalAtk = CalculateStat(Base.playerBaseAttack + evolutionAtk + equipAtk, 0);
        finalHp = CalculateStat(Base.playerBaseHp + evolutionHp + equipHp, 0);

        LoadWeapon(equipped);

        Debug.Log("[PlayerStats] 공격력 " + finalAtk + " (기본 " + Base.playerBaseAttack + " + 진화 " + evolutionAtk + " + 장비 " + equipAtk + ")" +
                  " / 체력 " + finalHp + " (기본 " + Base.playerBaseHp + " + 진화 " + evolutionHp + " + 장비 " + equipHp + ")" +
                  " / 무기: " + (hasWeapon ? equippedWeaponName + " (" + equippedWeaponType + ")" : "없음") +
                  " / 시작 스킬: " + (startingSkillId == WeaponSkillTable.NoSkill ? "없음" : startingSkillId.ToString()));
    }

    // 메인·테스트 씬은 인벤토리, 전투 씬은 계정 데이터에서 장착 장비를 가져온다
    private static List<OwnedItem> GetEquipped(PlayerSaveData data)
    {
        var result = new List<OwnedItem>();

        if (PlayerInventory.Instance != null)
        {
            foreach (EquipSlotType slot in Enum.GetValues(typeof(EquipSlotType)))
            {
                var item = PlayerInventory.Instance.GetEquipped(slot);
                if (item != null)
                    result.Add(item);
            }
            return result;
        }

        InventoryRestorer.Restore(data.inventoryList, out var equipped);
        result.AddRange(equipped.Values);
        return result;
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

    private static int CalculateStat(int sum, int bonusPercent)
        => sum * (100 + bonusPercent) / 100;

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