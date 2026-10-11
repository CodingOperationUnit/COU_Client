using System.Linq;
using UnityEngine;

// 전투 플레이어 스탯
// 숫자 스탯은 서버 최종 스탯(PlayerServerStats)을 그대로 쓰고, 계산하지 않는다
// 장착 무기 정보(외형, 공격 모션, 시작 스킬)는 계정 데이터의 장착 장비에서 읽는다
public class PlayerStats : MonoBehaviour
{
    private bool isInitialized;
    private bool hasWeapon;
    private WeaponType equippedWeaponType;
    private string equippedWeaponName;
    private int startingSkillId;

    private static FinalStatsResponse Server => PlayerServerStats.Stats;

    public bool HasServerStats => PlayerServerStats.HasStats;

    // 서버 값이 없으면 0 (에러 후 씬 이동 처리됨)
    public int FinalAtk { get { EnsureInitialized(); return Server?.finalAttack ?? 0; } }
    public int FinalHp { get { EnsureInitialized(); return Server?.finalHp ?? 0; } }
    public int CriticalDamage { get { EnsureInitialized(); return Server?.criticalDamage ?? 0; } }
    public int CriticalChance { get { EnsureInitialized(); return Server?.criticalChance ?? 0; } }
    public int SkillDamage { get { EnsureInitialized(); return Server?.skillDamage ?? 0; } }
    public float Speed { get { EnsureInitialized(); return Server?.moveSpeed ?? 0f; } }
    public float MaxSpeed { get { EnsureInitialized(); return Server?.maxMoveSpeed ?? 0f; } }
    public float LootRadius { get { EnsureInitialized(); return Server?.lootRadius ?? 0f; } }

    public bool HasWeapon { get { EnsureInitialized(); return hasWeapon; } }
    public WeaponType EquippedWeaponType { get { EnsureInitialized(); return equippedWeaponType; } }
    public string EquippedWeaponName { get { EnsureInitialized(); return equippedWeaponName; } }
    public int StartingSkillId { get { EnsureInitialized(); return startingSkillId; } }   // 0이면 시작 스킬 없음

    private void Start()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (isInitialized) return;
        isInitialized = true;

        LoadEquippedWeapon();

        if (!PlayerServerStats.HasStats)
        {
            HandleMissingServerStats();
            return;
        }

        Debug.Log("[PlayerStats] 서버 최종 스탯 / 공격력 " + Server.finalAttack + ", 체력 " + Server.finalHp +
                  ", 이동 속도 " + Server.moveSpeed + ", 루팅 범위 " + Server.lootRadius +
                  " / 무기: " + (hasWeapon ? equippedWeaponName + " (" + equippedWeaponType + ")" : "없음") +
                  " / 시작 스킬: " + (startingSkillId == WeaponSkillTable.NoSkill ? "없음" : startingSkillId.ToString()));
    }

    private void LoadEquippedWeapon()
    {
        startingSkillId = WeaponSkillTable.NoSkill;
        hasWeapon = false;

        if (!IsPlayerDataLoaded()) return;

        InventoryRestorer.Restore(GameManager.PlayerData.currentData.inventoryList, out var equipped);
        var weapon = equipped.Values.FirstOrDefault(item => item.Data.SlotType == EquipSlotType.Weapon);

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

    // 서버 값 없이 전투에 들어온 경우: 에러 후 이동 (클라이언트 계산으로 대체하지 않음)
    private void HandleMissingServerStats()
    {
        bool loggedIn = IsPlayerDataLoaded();
        string target = loggedIn ? GameConstants.SceneNames.MAIN_SCENE : GameConstants.SceneNames.LOGIN_SCENE;

        Debug.LogError("[PlayerStats] 서버 최종 스탯이 없습니다. 전투 시작 버튼을 거쳐 진입해야 합니다. " +
                       (loggedIn ? "메인" : "로그인") + " 화면으로 이동합니다.");

        GameManager.Scene.ChangeScene(target);
    }

    private static bool IsPlayerDataLoaded()
    {
        var dataManager = GameManager.PlayerData;
        return dataManager != null && dataManager.isPlayerDataLoaded;
    }
}