using UnityEngine;

[SkillId( 2 )]
public sealed class Skill_Revolver : SkillBase
{
    public override void Activate()
    {
        // TODO: 자동 타겟팅 및 투사체 발사 로직 (Player/Monster 구현 후 작성 예정)
        FindNearestTarget();
        FireProjectile();

        ResetCooldown();
    }

    private Transform FindNearestTarget()
    {
        // TODO: 자동 타겟팅 및 투사체 발사 로직 (Player/Monster 구현 후 작성 예정)
        return null;
    }

    private void FireProjectile()
    {
        // TODO: 자동 타겟팅 및 투사체 발사 로직 (Player/Monster 구현 후 작성 예정)
    }
}
