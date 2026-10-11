using UnityEngine;

// 플레이어 외형: 좌우 반전, 장착 무기 외형, 공격 모션
// 실제 그림·애니메이션은 Visual 아래 SPUM 캐릭터의 PlayerAppearance가 담당
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Transform visual;                // 좌우 반전 대상 (SPUM 캐릭터의 부모)
    [SerializeField] private PlayerAppearance appearance;     // Visual 아래 SPUM 캐릭터
    [SerializeField] private bool artFacesRight = false;      // 원본 그림이 오른쪽을 보는가 (SPUM은 왼쪽 → 끔)

    private PlayerMovement playerMovement;
    private PlayerStats playerStats;

    private bool isFacingRight = true;

    public PlayerAppearance Appearance => appearance;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerStats = GetComponent<PlayerStats>();

        if (appearance == null)
            Debug.LogError("[PlayerVisual] Appearance(SPUM 캐릭터)가 연결되지 않았습니다.", this);
    }

    private void Start()
    {
        if (appearance != null)
            appearance.SetWeapon(playerStats.HasWeapon ? playerStats.EquippedWeaponItemId : null);
    }

    private void Update()
    {
        float x = playerMovement.FacingDirection.x;

        if (x > 0.01f)
            isFacingRight = true;
        else if (x < -0.01f)
            isFacingRight = false;

        // 원본 방향과 바라볼 방향이 다르면 뒤집는다
        bool flip = isFacingRight != artFacesRight;
        visual.localScale = new Vector3(flip ? -1f : 1f, 1f, 1f);
    }

    public void PlayAttackMotion()
    {
        if (appearance == null) return;

        if (!playerStats.HasWeapon)
        {
            Debug.Log("[PlayerVisual] 장착 무기 없음: 공격 모션 없음");
            return;
        }

        appearance.PlayAttack();
        Debug.Log("[PlayerVisual] 공격 모션 재생: " + appearance.AttackIndex + "번");
    }
}