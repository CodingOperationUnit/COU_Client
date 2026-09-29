using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Transform visual;
    [SerializeField] private GameObject weapon;

    private PlayerMovement playerMovement;
    private PlayerStats playerStats;

    private bool isFacingRight = true;
    private string attackMotion;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        var equippedWeapon = playerStats.EquippedWeapon;

        weapon.SetActive(equippedWeapon != null);

        if (equippedWeapon != null)
            attackMotion = GetAttackMotion(equippedWeapon.weaponType);
    }

    private void Update()
    {
        float x = playerMovement.FacingDirection.x;

        if (x > 0.01f)
            isFacingRight = true;
        else if (x < -0.01f)
            isFacingRight = false;

        visual.localScale = new Vector3(isFacingRight ? 1f : -1f, 1f, 1f);
    }

    public void PlayAttackMotion()
    {
        if (attackMotion == null)
        {
            Debug.Log("[PlayerVisual] 장착 무기 없음: 공격 모션 없음");
            return;
        }

        // 추후 애니메이션 연결: animator.SetTrigger(attackMotion);
        Debug.Log("[PlayerVisual] 공격 모션 재생: " + attackMotion);
    }

    private static string GetAttackMotion(WeaponType weaponType) => weaponType switch
    {
        WeaponType.Sword => "Attack_Sword",
        WeaponType.Blunt => "Attack_Blunt",
        WeaponType.Bow => "Attack_Bow",
        WeaponType.Gun => "Attack_Gun",
        WeaponType.Throw => "Attack_Throw",
        _ => "Attack_Default"
    };
}