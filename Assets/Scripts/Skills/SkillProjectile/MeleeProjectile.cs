using UnityEngine;

public sealed class MeleeProjectile : SkillProjectile
{
    [SerializeField] private float _range = 1.5f;
    [SerializeField] private float _effectDuration = 0.3f;

    private float _elapsed;
    private bool _hasDamaged;

    protected override void OnLaunch()
    {
        _elapsed = 0f;
        _hasDamaged = false;
    }

    private void Update()
    {
        if(!_hasDamaged)
        {
            DamageInRange();
            _hasDamaged = true;
        }

        _elapsed += Time.deltaTime;

        if(_elapsed >= _effectDuration)
        {
            ReturnToPool();
        }
    }

    private void DamageInRange()
    {
        // TODO: _range 내 타겟 판정 (Monster 구현 후 작성 예정)
        ApplyDamage();
    }
}
