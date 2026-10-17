public sealed class Skill_Speaker : PersistentSkillBase
{
    protected override void SpawnObjects()
    {
        // 3레벨: 3개, 진화(5레벨 + 이어폰): 5개. 개수 증가 패시브 보너스는 여기에 더해짐
        int baseCount = IsEvolved ? 5 : level >= 3 ? 3 : 1;
        int count = GetFlatStat(SkillFlatStat.ProjectileCount, baseCount);

        // 스피커 사이 간격이 균등하도록 시작 각도를 나눔. 한 번만 계산해서 오브젝트마다 공유
        SkillStats objectStats = BuildStats();

        // 회전 반지름은 사거리 증가 보너스까지 반영한 Range를 씀
        float radius = Range;

        for(int i = 0; i < count; i++)
        {
            float startAngle = 360.0f / count * i;

            SpawnPersistent(transform.position, new OrbitData(objectStats, transform, startAngle, radius));
        }
    }
}
