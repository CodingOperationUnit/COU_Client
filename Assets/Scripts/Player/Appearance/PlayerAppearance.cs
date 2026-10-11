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

    // ==== 애니메이션 ====
    public void PlayIdle() => PlayLoop(PlayerState.IDLE);
    public void PlayMove() => PlayLoop(PlayerState.MOVE);
    public void PlayAttack() => Play(PlayerState.ATTACK, AttackIndex);
    public void PlayDamaged() => Play(PlayerState.DAMAGED, 0);
    public void PlayDeath() => Play(PlayerState.DEATH, 0);


    private void PlayLoop(PlayerState state)
    {
        if (currentState == state) return;
        Play(state, 0);
    }

    private void Play(PlayerState state, int index)
    {
        if (!isReady) return;

        var clips = GetClips(state);
        if(clips == null || clips.Count == 0)
        {
            Debug.LogWarning("[PlayerAppearance] " + state + " 애니메이션이 없습니다.", this);
            return;
        }

        if (index < 0 || index >= clips.Count)
        {
            Debug.LogWarning("[PlayerAppearance] " + state + " " + index + "번 애니메이션이 없어 0번을 재생합니다.", this);
            index = 0;
        }

        spum.PlayAnimation(state, index);
        currentState = state;
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
    [ContextMenu("Test/Attack (testAttackIndex)")] private void TestAttack() => Play(PlayerState.ATTACK, testAttackIndex);
    [ContextMenu("Test/Damaged")] private void TestDamaged() => PlayDamaged();
    [ContextMenu("Test/Death")] private void TestDeath() => PlayDeath();
    [ContextMenu("Test/Weapon (testItemId)")] private void TestWeapon() => SetWeapon(testItemId);
    [ContextMenu("Test/No Weapon")] private void TestNoWeapon() => SetWeapon(null);
}
