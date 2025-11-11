using UnityEngine;

/// <summary>
/// Status effect that temporarily increases a stat
/// </summary>
public class BuffStatusEffect : IStatusEffect
{
    private StatType statType;
    private float buffAmount;
    private float duration;
    private float remainingTime;
    private bool applied;
    
    public string EffectName => $"{statType} Buff";
    public float Duration => duration;
    public bool IsExpired => remainingTime <= 0;
    
    public BuffStatusEffect(StatType statType, float buffAmount, float duration)
    {
        this.statType = statType;
        this.buffAmount = buffAmount;
        this.duration = duration;
        this.remainingTime = duration;
        this.applied = false;
    }
    
    public void Apply(ISkillTarget target)
    {
        if (applied) return;
        
        // Apply the buff
        switch (statType)
        {
            case StatType.Attack:
                target.Attack += Mathf.RoundToInt(buffAmount);
                break;
            case StatType.Defense:
                target.Defense += Mathf.RoundToInt(buffAmount);
                break;
            // Add other stat types as needed
        }
        
        applied = true;
        SkillEvents.StatusEffectApplied(target, this);
    }
    
    public void Update(float deltaTime)
    {
        if (remainingTime > 0)
        {
            remainingTime -= deltaTime;
        }
    }
    
    public void Remove(ISkillTarget target)
    {
        if (!applied) return;
        
        // Remove the buff
        switch (statType)
        {
            case StatType.Attack:
                target.Attack -= Mathf.RoundToInt(buffAmount);
                break;
            case StatType.Defense:
                target.Defense -= Mathf.RoundToInt(buffAmount);
                break;
            // Add other stat types as needed
        }
        
        applied = false;
        SkillEvents.StatusEffectRemoved(target, this);
    }
}
