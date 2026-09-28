using UnityEngine;

[CreateAssetMenu( fileName = "SkillData", menuName = "Skill/SkillData" )]
public class SkillData : ScriptableObject
{
    public int SkillId;
    public string SkillName;
    public float Cooldown;
    public float Damage;
    public float Range;
}
