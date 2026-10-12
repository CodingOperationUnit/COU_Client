using UnityEngine;
using UnityEngine.LowLevel;


[RequireComponent(typeof(SPUM_Prefabs))]
public class PlayerAppearance : MonoBehaviour
{
    [SerializeField] private WeaponAppearanceSet weaponSet;
    [SerializeField] private SpriteRenderer rightWeapon;
    [SerializeField] private SpriteRenderer leftWeapon;

    [Header("Test")]
    [SerializeField] private long testItemId = 6001;
    [SerializeField] private int testAttackIndex = 0;

    private SPUM_Prefabs spum;
    private bool isReady;
    private PlayerState currentState = PlayerState.IDLE;
    private float oneShotEndTime;
    private bool isDead;

    private Sprite defaultRight;
    private Sprite defaultLeft;

    public int AttackIndex { get; private set; }

    public void Awake()
    {
        spum = GetComponent<SPUM_Prefabs>();

        defaultRight = rightWeapon != null ? rightWeapon.sprite : null;
        defaultLeft = leftWeapon != null ? leftWeapon.sprite : null;

        if(spum._anim == null)
        {
            Debug.LogError("[PlayerAppearance] SPUM_Prefabs의 Anim(Animator)이 비어 있습니다.", this);
            return;
        }

        spum.OverrideControllerInit();
        isReady = true;
    }

    // === 무기 ===
    public void SetWeapon(long? itemId)
    {
        if (itemId == null)
        {
            SetWeaponSprites(null, null);
            AttackIndex = 0;
            return;
        }

        if (weaponSet != null && weaponSet.TryGet(itemId.Value, out var entry))
        {
            SetWeaponSprites(entry.rightHand, entry.leftHand);
            AttackIndex = entry.attackAnimationIndex;
            return;
        }

        SetWeaponSprites(defaultRight, defaultLeft);
        AttackIndex = 0;
        Debug.LogWarning("[PlayerAppearance] 무기 외형이 등록되지 않았습니다: " + itemId.Value + ". 기본 무기를 표시합니다.", this);
    }

    private void SetWeaponSprites(Sprite right, Sprite left)
    {
        if (rightWeapon != null) rightWeapon.sprite = right;
        if (leftWeapon != null) leftWeapon.sprite = left;
    }

    // ===== 애니메이션 =====

    public void PlayIdle() => PlayLoop(PlayerState.IDLE);
    public void PlayMove() => PlayLoop(PlayerState.MOVE);

    public void PlayAttack()
    {
        if (isDead) return;
        PlayOneShot(PlayerState.ATTACK, AttackIndex);
    }

    public void PlayDamaged()
    {
        if (isDead) return;

        // 피격 모션이 재생 중이면 다시 시작하지 않음 (연속 피해 시 떨림 방지)
        if (currentState == PlayerState.DAMAGED && Time.time < oneShotEndTime) return;

        PlayOneShot(PlayerState.DAMAGED, 0);
    }

    public void PlayDeath()
    {
        if (isDead) return;

        Play(PlayerState.DEATH, 0);
        isDead = true;   // 이후 다른 모션 재생 안 함
    }

    // 대기·이동처럼 계속 유지되는 상태: 바뀔 때만, 한 번 재생 모션이 끝난 뒤에만
    private void PlayLoop(PlayerState state)
    {
        if (isDead) return;
        if (Time.time < oneShotEndTime) return;
        if (currentState == state) return;

        Play(state, 0);
    }

    // 공격·피격처럼 한 번 재생하는 상태: 클립 길이만큼 대기·이동 전환을 막음
    private void PlayOneShot(PlayerState state, int index)
    {
        var clip = Play(state, index);
        if (clip != null)
            oneShotEndTime = Time.time + clip.length;
    }

    private AnimationClip Play(PlayerState state, int index)
    {
        if (!isReady) return null;

        var clips = GetClips(state);
        if (clips == null || clips.Count == 0)
        {
            Debug.LogWarning("[PlayerAppearance] " + state + " 애니메이션이 없습니다.", this);
            return null;
        }

        if (index < 0 || index >= clips.Count)
        {
            Debug.LogWarning("[PlayerAppearance] " + state + " " + index + "번 애니메이션이 없어 0번을 재생합니다.", this);
            index = 0;
        }

        spum.PlayAnimation(state, index);
        currentState = state;
        return clips[index];
    }

    private System.Collections.Generic.List<AnimationClip> GetClips(PlayerState state) => state switch
    {
        PlayerState.IDLE => spum.IDLE_List,
        PlayerState.MOVE => spum.MOVE_List,
        PlayerState.ATTACK => spum.ATTACK_List,
        PlayerState.DAMAGED => spum.DAMAGED_List,
        PlayerState.DEBUFF => spum.DEBUFF_List,
        PlayerState.DEATH => spum.DEATH_List,
        _ => spum.OTHER_List
    };

    // ===== 테스트 (Inspector ⋮ 메뉴) =====

    [ContextMenu("Test/Idle")] private void TestIdle() => PlayIdle();
    [ContextMenu("Test/Move")] private void TestMove() => PlayMove();
    [ContextMenu("Test/Attack (testAttackIndex)")] private void TestAttack() => PlayOneShot(PlayerState.ATTACK, testAttackIndex);
    [ContextMenu("Test/Damaged")] private void TestDamaged() => PlayDamaged();
    [ContextMenu("Test/Death")] private void TestDeath() => PlayDeath();
    [ContextMenu("Test/Weapon (testItemId)")] private void TestWeapon() => SetWeapon(testItemId);
    [ContextMenu("Test/No Weapon")] private void TestNoWeapon() => SetWeapon(null);
}
