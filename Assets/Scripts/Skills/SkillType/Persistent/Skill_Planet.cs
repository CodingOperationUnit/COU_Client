public sealed class Skill_Planet : PersistentSkillBase
{
    protected override void SpawnObjects()
    {
        // 3레벨: 3개, 5레벨: 5개
        int baseCount = level >= 5 ? 5 : level >= 3 ? 3 : 1;
        int count = GetFlatStat(SkillFlatStat.ProjectileCount, baseCount);

        // 행성 사이 간격이 균등하도록 시작 각도를 나눔. 한 번만 계산해서 오브젝트마다 공유
        SkillStats objectStats = BuildStats();

        for(int i = 0; i < count; i++)
        {
            float startAngle = 360.0f / count * i;

            SpawnPersistent(transform.position, new OrbitData(objectStats, transform, startAngle, skillData.skillRange));
        }
    }
}
