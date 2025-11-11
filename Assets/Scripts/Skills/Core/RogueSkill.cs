using UnityEngine;

/// <summary>
/// Rogue-specific skill implementation
/// Can add rogue-specific behavior here
/// </summary>
public class RogueSkill : GenericSkill
{
    public RogueSkill(SkillData skillData) : base(skillData)
    {
        if (skillData.skillType != SkillType.Rogue)
        {
            Debug.LogWarning($"RogueSkill created with non-Rogue skill data: {skillData.skillName}");
        }
    }
    
    protected override void OnSkillUsed(ISkillTarget user, ISkillTarget target)
    {
        // Rogue-specific behavior after skill use
        // For example: chance for critical hit or bonus damage
        Debug.Log($"[Rogue] {user.Name} executed {SkillName} with precision!");
    }
}
