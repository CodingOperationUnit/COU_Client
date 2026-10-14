using System.Collections.Generic;
using UnityEngine;

// 탐지 범위 안의 모든 적에게 번개(LightningObject)를 하나씩 소환. 번개가 생기는 순간 대미지가 들어감
// 발동 조건(쿨타임, 범위 안에 적이 있는지)은 CooldownSkillBase를 따름
public sealed class Skill_PowerStrip : CooldownSkillBase
{
    private readonly HashSet<Enemy> _targets = new();

    protected override void Fire()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, skillData.skillRange, LayerMask.GetMask("Enemy"));

        // 번개마다 같은 수치를 쓰므로 한 번만 계산
        SkillStats skillStats = BuildStats();

        _targets.Clear();

        foreach(Collider2D other in colliders)
        {
            // 콜라이더가 여러 개인 적에게 번개가 겹쳐 나가지 않도록 함
            if(!other.TryGetComponent(out Enemy enemy) || enemy.isDead || !_targets.Add(enemy))
            {
                continue;
            }

            Spawn(transform.position, new LinkData(skillStats, transform, enemy.transform));
        }
    }
}
