using UnityEngine;

/// <summary>
/// Skill effect that deals damage to a target
/// </summary>
public class DamageEffect : ISkillEffect
{
    private float baseDamage;
    private float scalingPerLevel;
    private int skillLevel;
    
    public string EffectName => "Damage";
    
    public DamageEffect(float baseDamage, float scalingPerLevel, int skillLevel)
    {
        this.baseDamage = baseDamage;
        this.scalingPerLevel = scalingPerLevel;
        this.skillLevel = skillLevel;
    }
    
    public void Apply(ISkillTarget user, ISkillTarget target)
    {
        if (target == null)
        {
            Debug.LogWarning("DamageEffect: No target specified");
            return;
        }
        
        int damage = CalculateDamage(user);
        target.TakeDamage(damage);
        
        Debug.Log($"{user.Name} dealt {damage} damage to {target.Name}");
    }
    
    public bool Validate(ISkillTarget user, ISkillTarget target)
    {
        return target != null && target.Health > 0;
    }
    
    private int CalculateDamage(ISkillTarget user)
    {
        // Base damage + level scaling + user's attack stat scaling
        float totalDamage = baseDamage + (skillLevel - 1) * scalingPerLevel;
        
        // Add 10% of user's attack stat to damage
        totalDamage += user.Attack * 0.1f;
        
        return Mathf.RoundToInt(totalDamage);
    }
}
