using UnityEngine;

/// <summary>
/// Warrior-specific skill implementation
/// Can add warrior-specific behavior here
/// </summary>
public class WarriorSkill : GenericSkill
{
    public WarriorSkill(SkillData skillData) : base(skillData)
    {
        if (skillData.skillType != SkillType.Warrior)
        {
            Debug.LogWarning($"WarriorSkill created with non-Warrior skill data: {skillData.skillName}");
        }
    }
    
    protected override void OnSkillUsed(ISkillTarget user, ISkillTarget target)
    {
        // Warrior-specific behavior after skill use
        // For example: gain temporary defense boost
        Debug.Log($"[Warrior] {user.Name} used {SkillName} with warrior prowess!");
    }
}
