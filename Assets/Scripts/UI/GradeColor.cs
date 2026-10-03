using UnityEngine;

public static class GradeColor
{
    private static readonly Color[] colors =
    {
        new Color32(0xA0, 0xA7, 0xB4, 0xFF),
        new Color32(0x8F, 0xE6, 0x05, 0xFF),
        new Color32(0x0A, 0x90, 0xFF, 0xFF)
    };

    public static Color Get(ItemGrade grade)
        => colors[(int)grade];
}
