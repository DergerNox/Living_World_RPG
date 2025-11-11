using UnityEngine;

/// <summary>
/// Skill effect that heals a target
/// </summary>
public class HealEffect : ISkillEffect
{
    private float baseHealing;
    private float scalingPerLevel;
    private int skillLevel;
    
    public string EffectName => "Heal";
    
    public HealEffect(float baseHealing, float scalingPerLevel, int skillLevel)
    {
        this.baseHealing = baseHealing;
        this.scalingPerLevel = scalingPerLevel;
        this.skillLevel = skillLevel;
    }
    
    public void Apply(ISkillTarget user, ISkillTarget target)
    {
        // If no target specified, heal self
        ISkillTarget healTarget = target ?? user;
        
        int healAmount = CalculateHealing(user);
        healTarget.Heal(healAmount);
        
        Debug.Log($"{user.Name} healed {healTarget.Name} for {healAmount} HP");
    }
    
    public bool Validate(ISkillTarget user, ISkillTarget target)
    {
        ISkillTarget healTarget = target ?? user;
        return healTarget.Health < healTarget.MaxHealth;
    }
    
    private int CalculateHealing(ISkillTarget user)
    {
        // Base healing + level scaling
        float totalHealing = baseHealing + (skillLevel - 1) * scalingPerLevel;
        
        return Mathf.RoundToInt(totalHealing);
    }
}
