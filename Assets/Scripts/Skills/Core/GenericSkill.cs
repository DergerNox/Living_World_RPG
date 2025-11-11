using UnityEngine;

/// <summary>
/// Generic skill implementation that creates effects based on SkillData
/// </summary>
public class GenericSkill : BaseSkill
{
    public GenericSkill(SkillData skillData) : base(skillData)
    {
    }
    
    protected override void InitializeEffects()
    {
        effects.Clear();
        
        // Create effects based on skill data
        foreach (var effectData in data.effects)
        {
            ISkillEffect effect = CreateEffect(effectData);
            if (effect != null)
            {
                effects.Add(effect);
            }
        }
    }
    
    private ISkillEffect CreateEffect(SkillEffectData effectData)
    {
        switch (effectData.effectType)
        {
            case SkillEffectType.Damage:
                return new DamageEffect(effectData.baseValue, effectData.scalingPerLevel, currentLevel);
                
            case SkillEffectType.Heal:
                return new HealEffect(effectData.baseValue, effectData.scalingPerLevel, currentLevel);
                
            case SkillEffectType.BuffAttack:
                return new BuffEffect(StatType.Attack, effectData.baseValue, effectData.scalingPerLevel);
                
            case SkillEffectType.BuffDefense:
                return new BuffEffect(StatType.Defense, effectData.baseValue, effectData.scalingPerLevel);
                
            default:
                Debug.LogWarning($"Effect type {effectData.effectType} not implemented yet");
                return null;
        }
    }
    
    protected override void OnLevelUp()
    {
        // Reinitialize effects with new level
        InitializeEffects();
    }
}
