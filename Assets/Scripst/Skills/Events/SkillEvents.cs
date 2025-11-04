using System;
using UnityEngine;

/// <summary>
/// Static event system for skill-related events
/// Allows decoupling of skill system from UI and other systems
/// </summary>
public static class SkillEvents
{
    public static event Action<ISkillTarget, ISkill> OnSkillUsed;
    public static event Action<ISkillTarget, ISkill> OnSkillLearned;
    public static event Action<ISkill, int> OnSkillLevelUp;
    public static event Action<ISkillTarget, IStatusEffect> OnStatusEffectApplied;
    public static event Action<ISkillTarget, IStatusEffect> OnStatusEffectRemoved;
    
    public static void SkillUsed(ISkillTarget user, ISkill skill)
    {
        OnSkillUsed?.Invoke(user, skill);
        Debug.Log($"[SkillEvents] {user.Name} used {skill.SkillName}");
    }
    
    public static void SkillLearned(ISkillTarget learner, ISkill skill)
    {
        OnSkillLearned?.Invoke(learner, skill);
        Debug.Log($"[SkillEvents] {learner.Name} learned {skill.SkillName}");
    }
    
    public static void SkillLeveledUp(ISkill skill, int newLevel)
    {
        OnSkillLevelUp?.Invoke(skill, newLevel);
        Debug.Log($"[SkillEvents] {skill.SkillName} leveled up to {newLevel}");
    }
    
    public static void StatusEffectApplied(ISkillTarget target, IStatusEffect effect)
    {
        OnStatusEffectApplied?.Invoke(target, effect);
        Debug.Log($"[SkillEvents] {effect.EffectName} applied to {target.Name}");
    }
    
    public static void StatusEffectRemoved(ISkillTarget target, IStatusEffect effect)
    {
        OnStatusEffectRemoved?.Invoke(target, effect);
        Debug.Log($"[SkillEvents] {effect.EffectName} removed from {target.Name}");
    }
}
