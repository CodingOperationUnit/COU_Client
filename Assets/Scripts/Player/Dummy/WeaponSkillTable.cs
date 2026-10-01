using System.Collections.Generic;

// 임시: 장비 데이터(ItemData)에 시작 스킬<->무기 종류 관련 매핑이 추가되면 교체 후 삭제
public static class WeaponSkillTable
{
    public const int NoSkill = 0;

    // 인벤토리 무기: 아이템 ID → 시작 스킬 ID
    private static readonly Dictionary<long, int> itemToSkill = new()
    {
        { 6001, 3 },   // 낡은 검 → Katana
        { 6002, 2 },   // 연발 권총 → Revolver
        { 6003, 1 },   // 그림자 쿠나이 → Shuriken
    };

    // 무기 종류 ↔ 시작 스킬 ID (더미 무기, 공격 모션용)
    private static readonly Dictionary<WeaponType, int> typeToSkill = new()
    {
        { WeaponType.Sword, 3 },   // Katana
        { WeaponType.Gun, 2 },     // Revolver
        { WeaponType.Throw, 1 },   // Shuriken
    };

    public static int GetSkillByItem(long itemId)
        => itemToSkill.TryGetValue(itemId, out int skillId) ? skillId : NoSkill;

    public static int GetSkillByType(WeaponType weaponType)
        => typeToSkill.TryGetValue(weaponType, out int skillId) ? skillId : NoSkill;

    public static bool TryGetTypeBySkill(int skillId, out WeaponType weaponType)
    {
        foreach (var pair in typeToSkill)
        {
            if (pair.Value == skillId)
            {
                weaponType = pair.Key;
                return true;
            }
        }

        weaponType = default;
        return false;
    }
}