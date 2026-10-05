public sealed class Skill_Planet : PersistentSkillBase
{
    protected override void SpawnObjects()
    {
        // 3레벨: 3개, 5레벨: 5개
        int count = level >= 5 ? 5 : level >= 3 ? 3 : 1;

        for(int i = 0; i < count; i++)
        {
            // 행성 사이 간격이 균등하도록 시작 각도를 나눔
            float startAngle = 360.0f / count * i;

            SpawnPersistent(transform.position, new OrbitData(BuildStats(), transform, startAngle, skillData.skillRange));
        }
    }
}
