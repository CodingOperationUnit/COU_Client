using UnityEngine;

// 플레이어 주위를 도는 행성. 풀에 스스로 반환되지 않고 스킬 해제 때까지 유지됨
// SkillData: Speed = 초당 회전 각도, Range = 회전 반지름, Damage = 접촉 대미지
public sealed class PlanetObject : SkillObject<OrbitData>
{
    private Transform _owner;
    private float _angle;
    private float _damageMultiplier;

    protected override void OnInit(OrbitData data)
    {
        skillData = data.Data;
        _owner = data.Owner;
        _angle = data.StartAngle;
        _damageMultiplier = data.DamageMultiplier;
        OnLaunch();
    }

    protected override void OnLaunch()
    {
        UpdatePosition();
    }

    private void Update()
    {
        if(_owner == null)
        {
            return;
        }

        _angle += skillData.Speed * Time.deltaTime;
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if(_owner == null)
        {
            return;
        }

        float radian = _angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0.0f) * skillData.Range;

        transform.position = _owner.position + offset;
    }

    // 적을 스치는 순간 한 번만 대미지 (닿아 있는 동안 반복 대미지는 주지 않음)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.TryGetComponent(out Enemy enemy))
        {
            return;
        }

        enemy.Damaged(Mathf.RoundToInt(skillData.Damage * _damageMultiplier));
    }
}
