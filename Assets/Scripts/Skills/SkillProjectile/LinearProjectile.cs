using UnityEngine;

public sealed class LinearProjectile : SkillProjectile
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _lifeTime = 3f;

    private float _elapsed;

    protected override void OnLaunch()
    {
        _elapsed = 0f;
    }

    private void Update()
    {
        transform.Translate(dir * _speed * Time.deltaTime);

        _elapsed += Time.deltaTime;

        if(_elapsed >= _lifeTime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // TODO: 타겟 판정 (Monster 구현 후 작성 예정)
        ApplyDamage();
        ReturnToPool();
    }
}
