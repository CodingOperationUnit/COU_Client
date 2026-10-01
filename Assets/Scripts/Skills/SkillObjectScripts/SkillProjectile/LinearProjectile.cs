using UnityEngine;

public class LinearProjectile : SkillProjectile
{
    [SerializeField ]protected float lifeTime = 3f;

    protected float elapsed;
    protected float currentSpeed;
    protected bool hasHit;

    protected override void OnLaunch()
    {
        elapsed = 0f;
        hasHit = false;
        currentSpeed = skillData.Speed > 0f ? skillData.Speed : 1.0f;

        // 탄두(up)가 발사 방향을 향하도록 회전
        transform.up = direction;
    }

    private void Update()
    {
        Movement();

        elapsed += Time.deltaTime;

        if(elapsed >= lifeTime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Enemy에만 반응 (플레이어 등 다른 물체와 겹쳐도 사라지지 않음)
        if(!other.TryGetComponent(out Enemy enemy))
        {
            return;
        }

        OnHitEnemy(enemy);
    }

    // 적에게 맞았을 때의 처리. 관통, 튕김 같은 고유 효과는 서브클래스에서 오버라이드
    protected virtual void OnHitEnemy(Enemy enemy)
    {
        // 한 물리 스텝에 적 둘과 겹치면 첫 번째만 처리
        if(hasHit)
        {
            return;
        }

        hasHit = true;
        ApplyDamage(enemy);
        ReturnToPool();
    }

    protected virtual void Movement()
    {
        // 로컬 좌표 기준이라 회전한 탄두 방향(up)으로 전진
        transform.Translate(Vector3.up * currentSpeed * Time.deltaTime);
    }
}
