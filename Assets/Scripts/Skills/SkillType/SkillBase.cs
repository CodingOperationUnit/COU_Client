using System;
using UnityEngine;

public abstract class SkillBase
{
    public const int MaxLevel = 5;

    // 수정 예정
    // ==============================================================================================================
    // 레벨당 배율 (레벨 1 = 1.0 기준으로 누적)
    private const float CooldownMultiplierPerLevel = 0.758f;
    private const float DamageMultiplierPerLevel = 1.2f;
    // ==============================================================================================================

    protected SkillData skillData;
    protected int level = 0;

    protected GameObject prefab;
    protected Transform transform;
    protected PlayerMovement movement;
    protected PlayerStats stats;
    protected SkillModifiers modifiers;

    public int Level => level;
    public bool IsEquipped { get; private set; }

    public float CooldownMultiplier => Mathf.Pow(CooldownMultiplierPerLevel, Mathf.Max(level - 1, 0));
    public float DamageMultiplier => Mathf.Pow(DamageMultiplierPerLevel, Mathf.Max(level - 1, 0));

    public int SkillId => skillData != null ? skillData.skillId : -1;
    public float Range => skillData != null ? skillData.skillRange : 0.0f;

    public virtual void Initialize(SkillData data)
    {
        skillData = data;
        level = 0;
    }

    public void SetContext(SkillContext context)
    {
        transform = context.Owner;
        movement = context.Movement;
        stats = context.Stats;
        modifiers = context.Modifiers;
    }

    // 프리팹이 필요 없는 스킬(패시브 등)은 null이 들어올 수 있음
    public void SetPrefab(GameObject prefab)
    {
        this.prefab = prefab;
    }

    // 매 프레임 호출. 발동 방식은 스킬 유형마다 다르므로 서브클래스가 구현
    public abstract void Tick(float deltaTime);

    public void Equip()
    {
        IsEquipped = true;
        OnEquip();
    }

    public void Unequip()
    {
        IsEquipped = false;
        OnUnequip();
    }

    public virtual void Levelup()
    {
        level = Math.Clamp(level + 1, 1, MaxLevel);
        NotifyLevelChanged();
    }

    public virtual void LevelDown()
    {
        level = Math.Clamp(level - 1, 1, MaxLevel);
        NotifyLevelChanged();
    }

    // 장착된 상태에서 레벨이 바뀔 때만 훅 호출 (장착 전 최초 레벨 설정은 OnEquip에서 처리)
    private void NotifyLevelChanged()
    {
        if(IsEquipped)
        {
            OnLevelChanged();
        }
    }

    protected virtual void OnEquip()
    {
    }

    protected virtual void OnUnequip()
    {
    }

    protected virtual void OnLevelChanged()
    {
    }

    // 레벨로 정해진 기본값에 패시브 보너스를 적용한 최종값. 보너스가 없으면 기본값 그대로
    protected float GetPercentStat(SkillPercentStat stat, float baseValue)
    {
        return modifiers != null ? modifiers.ApplyPercent(stat, baseValue) : baseValue;
    }

    // 개수처럼 고정값으로 늘어나는 스탯용. 0 미만은 0으로 맞춤
    protected int GetFlatStat(SkillFlatStat stat, int baseValue)
    {
        int value = modifiers != null ? modifiers.ApplyFlat(stat, baseValue) : baseValue;

        return Mathf.Max(value, 0);
    }

    // 스킬 오브젝트에 넘길 최종 수치 계산 (레벨 배율, 패시브 보너스 반영)
    protected SkillStats BuildStats()
    {
        int damage = Mathf.RoundToInt(GetPercentStat(SkillPercentStat.Damage, skillData.skillDamage * DamageMultiplier));
        float speed = GetPercentStat(SkillPercentStat.Speed, skillData.skillSpeed);
        float scale = GetPercentStat(SkillPercentStat.Area, 1.0f);

        return new SkillStats(damage, speed, scale);
    }

    // 풀에서 스킬 오브젝트를 꺼내 초기화. 초기화 인자 구조체 타입(TInitData)은 호출부에서 추론됨
    protected GameObject Spawn<TInitData>(Vector3 position, TInitData initData)
    {
        if(prefab == null)
        {
            Debug.LogWarning($"[SkillBase] 프리팹이 없습니다: {SkillId}");
            return null;
        }

        return SkillObjectPool.Instance.Get(prefab, position, Quaternion.identity, instance =>
        {
            if(instance.TryGetComponent(out SkillObject<TInitData> skillObject))
            {
                skillObject.Init(initData);
            }
        });
    }
}
