using System;
using System.Collections.Generic;

// 퍼센트로 증감하는 스탯. 값은 비율로 넣음 (0.1 = +10%, -0.2 = -20%)
public enum SkillPercentStat
{
    Damage,
    Cooldown,
    Area,   // 크기 (기본 1.0배)
    Speed
}

// 고정 수치로 증감하는 스탯
public enum SkillFlatStat
{
    ProjectileCount, // 투사체/지속형 오브젝트 개수
    Pierce
}

// 패시브가 등록하고, 액티브 스킬이 발동할 때 읽는 보너스 모음
// 보너스를 등록한 주체(source)별로 보관해서 해제할 때 수치를 되돌릴 필요 없이 통째로 제거함
public class SkillModifiers
{
    private static readonly int PercentStatCount = Enum.GetValues(typeof(SkillPercentStat)).Length;
    private static readonly int FlatStatCount = Enum.GetValues(typeof(SkillFlatStat)).Length;

    private sealed class SourceEntry
    {
        public readonly float[] Percents = new float[PercentStatCount];
        public readonly int[] Flats = new int[FlatStatCount];
    }

    private readonly Dictionary<SkillBase, SourceEntry> _entriesBySource = new();

    // 종류별 합계 캐시 (Apply가 발사할 때마다 불리므로 변경 시점에만 다시 계산)
    private readonly float[] _percentTotals = new float[PercentStatCount];
    private readonly int[] _flatTotals = new int[FlatStatCount];

    // 보너스가 바뀔 때 호출 (지속형 스킬이 오브젝트를 다시 만들 때 사용)
    public event Action OnChanged;

    // source가 등록한 같은 종류의 보너스를 새 값으로 교체
    public void SetPercent(SkillBase source, SkillPercentStat stat, float value)
    {
        GetEntry(source).Percents[(int)stat] = value;
        Recalculate();
    }

    public void SetFlat(SkillBase source, SkillFlatStat stat, int value)
    {
        GetEntry(source).Flats[(int)stat] = value;
        Recalculate();
    }

    public void RemoveSource(SkillBase source)
    {
        if(_entriesBySource.Remove(source))
        {
            Recalculate();
        }
    }

    // 기본값 * (1 + 퍼센트 보너스 합). 보너스가 없으면 기본값 그대로
    public float ApplyPercent(SkillPercentStat stat, float baseValue)
    {
        return baseValue * (1.0f + _percentTotals[(int)stat]);
    }

    // 기본값 + 고정 보너스 합. 보너스가 없으면 기본값 그대로
    public int ApplyFlat(SkillFlatStat stat, int baseValue)
    {
        return baseValue + _flatTotals[(int)stat];
    }

    private SourceEntry GetEntry(SkillBase source)
    {
        if(!_entriesBySource.TryGetValue(source, out SourceEntry entry))
        {
            entry = new SourceEntry();
            _entriesBySource[source] = entry;
        }

        return entry;
    }

    private void Recalculate()
    {
        Array.Clear(_percentTotals, 0, _percentTotals.Length);
        Array.Clear(_flatTotals, 0, _flatTotals.Length);

        foreach(SourceEntry entry in _entriesBySource.Values)
        {
            for(int i = 0; i < PercentStatCount; i++)
            {
                _percentTotals[i] += entry.Percents[i];
            }

            for(int i = 0; i < FlatStatCount; i++)
            {
                _flatTotals[i] += entry.Flats[i];
            }
        }

        OnChanged?.Invoke();
    }
}
