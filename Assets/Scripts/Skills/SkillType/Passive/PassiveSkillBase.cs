// 발동 없이 장착하는 동안 효과(스탯 보너스 등)만 적용하는 스킬의 베이스
public abstract class PassiveSkillBase : SkillBase
{
    public override void Tick(float deltaTime)
    {
    }

    protected override void OnEquip()
    {
        ApplyEffect();
    }

    protected override void OnUnequip()
    {
        RemoveEffect();
    }

    // 레벨에 따라 효과량이 달라지므로 기존 효과를 제거하고 현재 레벨 기준으로 다시 적용
    protected override void OnLevelChanged()
    {
        RemoveEffect();
        ApplyEffect();
    }

    protected abstract void ApplyEffect();
    protected abstract void RemoveEffect();
}
