using System.Collections.Generic;
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
        // Damaged → Die가 순회 중 스포너 목록에서 적을 제거하므로 역순으로 순회
        IReadOnlyList<Enemy> enemies = MonsterSpawner.Instance.SpawnedEnemies;

        for(int i = enemies.Count - 1; i >= 0; i--)
        {
            if(Vector2.Distance(transform.position, enemies[i].transform.position) <= _range)
            {
                ApplyDamage(enemies[i]);
            }
        }
    }
}
