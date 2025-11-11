using UnityEngine;

/// <summary>
/// Skill effect that applies a buff (stat increase) to a target
/// </summary>
public class BuffEffect : ISkillEffect
{
    private StatType statType;
    private float buffAmount;
    private float duration;
    
    public string EffectName => $"Buff {statType}";
    
    public BuffEffect(StatType statType, float buffAmount, float duration)
    {
        this.statType = statType;
        this.buffAmount = buffAmount;
        this.duration = duration;
    }
    
    public void Apply(ISkillTarget user, ISkillTarget target)
    {
        // If no target specified, buff self
        ISkillTarget buffTarget = target ?? user;
        
        // Create and apply status effect
        var statusEffect = new BuffStatusEffect(statType, buffAmount, duration);
        buffTarget.ApplyStatusEffect(statusEffect);
        
        Debug.Log($"{user.Name} buffed {buffTarget.Name}'s {statType} by {buffAmount} for {duration}s");
    }
    
    public bool Validate(ISkillTarget user, ISkillTarget target)
    {
        return true; // Buffs can always be applied
    }
}

/// <summary>
/// Enum for different stat types that can be buffed/debuffed
/// </summary>
public enum StatType
{
    Attack,
    Defense,
    Speed,
    CriticalChance
}
