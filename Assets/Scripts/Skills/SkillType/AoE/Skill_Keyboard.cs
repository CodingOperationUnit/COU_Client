public sealed class Skill_Keyboard : AoeSkillBase
{
    protected override void Fire()
    {
        // 사용자 위치에서 가장 가까운 적 방향으로 휘두름. 범위의 위치/크기는 프리팹 콜라이더가 결정
        Spawn(transform.position, new AoEData(BuildStats(), transform.position, fireDirection));
    }
}
