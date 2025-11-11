using UnityEngine;

/// <summary>
/// Mage-specific skill implementation
/// Can add mage-specific behavior here
/// </summary>
public class MageSkill : GenericSkill
{
    public MageSkill(SkillData skillData) : base(skillData)
    {
        if (skillData.skillType != SkillType.Mage)
        {
            Debug.LogWarning($"MageSkill created with non-Mage skill data: {skillData.skillName}");
        }
    }
    
    protected override void OnSkillUsed(ISkillTarget user, ISkillTarget target)
    {
        // Mage-specific behavior after skill use
        // For example: chance to refund some mana
        Debug.Log($"[Mage] {user.Name} channeled arcane energy through {SkillName}!");
    }
}
