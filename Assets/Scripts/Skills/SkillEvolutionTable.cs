using System;
using System.Collections.Generic;

// 액티브 스킬이 진화하려면 5레벨이 된 뒤 여기에 적힌 짝 패시브를 하나라도 가지고 있어야 함
// 짝 패시브가 여럿이면 배열에 모두 적으면 되고, 그중 하나만 있어도 진화함. 나중에 데이터 테이블로 옮기기 쉽도록 한 곳에 모아 둠
public static class SkillEvolutionTable
{
    private static readonly Dictionary<int, int[]> _passiveIdsByActiveId = new()
    {
        { 20010, new[] { 21010 } }, // 텀블러 -> 에너지드링크
        { 20020, new[] { 21020 } }, // 안경 -> 안약
        { 20030, new[] { 21030 } }, // 모니터 -> 보조 모니터
        { 20040, new[] { 21040 } }, // 스피커 -> 이어폰
        { 20050, new[] { 21050 } }, // 마우스 -> 마우스 패드
        { 20060, new[] { 21060 } }, // 키보드 -> KeyboardSwitch
        { 20070, new[] { 21070 } }, // CPU -> 오버클럭
        { 20080, new[] { 21080 } }, // 멀티탭 -> 정전기
        { 20090, new[] { 21090 } }, // CtrlZ -> Shift
    };

    // 짝 패시브 ID 목록. 진화 조합이 없는 스킬은 빈 배열
    public static IReadOnlyList<int> GetPassiveIds(int activeSkillId)
    {
        return _passiveIdsByActiveId.TryGetValue(activeSkillId, out int[] passiveIds) ? passiveIds : Array.Empty<int>();
    }
}
