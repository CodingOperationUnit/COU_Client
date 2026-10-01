using UnityEngine;

public readonly struct SkillContext
{
    public readonly Transform Owner;
    public readonly PlayerMovement Movement;
    public readonly PlayerStats Stats;

    public SkillContext(Transform owner, PlayerMovement movement, PlayerStats stats)
    {
        Owner = owner;
        Movement = movement;
        Stats = stats;
    }
}