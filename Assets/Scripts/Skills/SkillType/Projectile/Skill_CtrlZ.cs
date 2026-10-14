using UnityEngine;

// 가장 가까운 적에게 부메랑(BoomerangProjectile)을 던짐. 부메랑이 돌아올 때까지 다음 발사를 막고,
// 돌아온 시점부터 쿨타임을 다시 셈 (시트 쿨타임이 0에 가까우면 돌아오자마자 다시 발사)
public sealed class Skill_CtrlZ : ProjectileSkillBase
{
    private GameObject _thrown;

    protected override bool CanActivate()
    {
        // 던진 부메랑이 아직 돌아오지 않았으면 장전되지 않은 상태
        if(_thrown != null && _thrown.activeInHierarchy)
        {
            return false;
        }

        return base.CanActivate();
    }

    protected override void Fire()
    {
        GameObject instance = Spawn(transform.position, new ProjectileData(BuildStats(), fireDirection, fireTarget, 1, transform));

        if(instance == null || !instance.TryGetComponent(out BoomerangProjectile boomerang))
        {
            return;
        }

        _thrown = instance;
        boomerang.OnBeforeReturn += HandleBoomerangReturned;
    }

    private void HandleBoomerangReturned(GameObject instance)
    {
        // 같은 오브젝트가 풀에서 다시 나올 때 중복 구독되지 않도록 해제
        if(instance.TryGetComponent(out BoomerangProjectile boomerang))
        {
            boomerang.OnBeforeReturn -= HandleBoomerangReturned;
        }

        _thrown = null;
        ResetCooldown();
    }
}
