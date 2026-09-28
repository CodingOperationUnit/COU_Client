using System;

[AttributeUsage( AttributeTargets.Class )]
public sealed class SkillIdAttribute : Attribute
{
    public int SkillId { get; }

    public SkillIdAttribute( int skillId )
    {
        SkillId = skillId;
    }
}
