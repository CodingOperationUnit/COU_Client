using UnityEngine;

public class LinearProjectile : SkillProjectile
{
    [SerializeField ]private float _lifeTime = 3f;

    private float _elapsed;
    private float _currentSpeed;
    private bool _hasHit;

    protected override void OnLaunch()
    {
        _elapsed = 0f;
<<<<<<< Updated upstream:Assets/Scripts/Skills/SkillProjectile/LinearProjectile.cs
        _hasHit = false;
        _currentSpeed = skillData.Speed > 0f ? skillData.Speed : _speed;
=======
        _currentSpeed = skillData.Speed > 0f ? skillData.Speed : 1.0f;
>>>>>>> Stashed changes:Assets/Scripts/Skills/SkillObjectScripts/SkillProjectile/LinearProjectile.cs

        // 탄두(up)가 발사 방향을 향하도록 회전
        transform.up = direction;
    }

    private void Update()
    {
        Movement();

        _elapsed += Time.deltaTime;

        if(_elapsed >= _lifeTime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Enemy에만 반응 (플레이어 등 다른 물체와 겹쳐도 사라지지 않음)
        // 한 물리 스텝에 적 둘과 겹치면 첫 번째만 처리
        if(_hasHit || !other.TryGetComponent(out Enemy enemy))
        {

<<<<<<< Updated upstream:Assets/Scripts/Skills/SkillProjectile/LinearProjectile.cs
        _hasHit = true;
        ApplyDamage(enemy);
        ReturnToPool();
=======
            if (other.gameObject.TryGetComponent<Enemy>(out Enemy enemy)) 
            {
                ApplyDamage(enemy);
                ReturnToPool();
                return;
            }
        }
    }

    protected virtual void Movement()
    {
        // 로컬 좌표 기준이라 회전한 탄두 방향(up)으로 전진
        transform.Translate(Vector3.up * _currentSpeed * Time.deltaTime);
    }

    protected override void ApplyDamage(Enemy enemy)
    {
        float roundDamage = skillData.Damage + 0.5f;
        enemy.Damaged((int)roundDamage);
>>>>>>> Stashed changes:Assets/Scripts/Skills/SkillObjectScripts/SkillProjectile/LinearProjectile.cs
    }
}
