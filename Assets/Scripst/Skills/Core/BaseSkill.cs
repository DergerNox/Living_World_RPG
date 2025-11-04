using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base abstract class for all skills
/// Implements common skill functionality and delegates specific behavior to derived classes
/// </summary>
public abstract class BaseSkill : ISkill
{
    protected SkillData data;
    protected int currentLevel;
    protected float currentCooldown;
    protected List<ISkillEffect> effects;
    
    public string SkillId => data.skillId;
    public string SkillName => data.skillName;
    public string Description => data.description;
    public SkillType SkillType => data.skillType;
    public int Level => currentLevel;
    public int ManaCost => CalculateManaCost();
    public float Cooldown => CalculateCooldown();
    public bool IsPassive => data.isPassive;
    public bool IsOnCooldown => currentCooldown > 0;
    
    protected BaseSkill(SkillData skillData)
    {
        data = skillData;
        currentLevel = 1;
        currentCooldown = 0;
        effects = new List<ISkillEffect>();
        InitializeEffects();
    }
    
    /// <summary>
    /// Initialize skill effects based on skill data
    /// Override in derived classes for custom effect initialization
    /// </summary>
    protected abstract void InitializeEffects();
    
    public virtual bool CanUse(ISkillTarget user)
    {
        if (IsPassive)
        {
            Debug.Log($"{SkillName} is a passive skill and cannot be used actively");
            return false;
        }
        
        if (IsOnCooldown)
        {
            Debug.Log($"{SkillName} is on cooldown ({currentCooldown:F1}s remaining)");
            return false;
        }
        
        if (user.Mana < ManaCost)
        {
            Debug.Log($"{user.Name} does not have enough mana to use {SkillName} (Need: {ManaCost}, Have: {user.Mana})");
            return false;
        }
        
        return true;
    }
    
    public virtual void Use(ISkillTarget user, ISkillTarget target = null)
    {
        if (!CanUse(user)) return;
        
        // Consume mana
        user.Mana -= ManaCost;
        
        // Start cooldown
        currentCooldown = Cooldown;
        
        // Apply all effects
        ApplyEffects(user, target);
        
        // Trigger event
        SkillEvents.SkillUsed(user, this);
        
        // Call hook for derived classes
        OnSkillUsed(user, target);
    }
    
    protected virtual void ApplyEffects(ISkillTarget user, ISkillTarget target)
    {
        foreach (var effect in effects)
        {
            if (effect.Validate(user, target))
            {
                effect.Apply(user, target);
            }
        }
    }
    
    public virtual void LevelUp()
    {
        if (currentLevel < data.maxLevel)
        {
            currentLevel++;
            SkillEvents.SkillLeveledUp(this, currentLevel);
            OnLevelUp();
        }
        else
        {
            Debug.Log($"{SkillName} is already at max level ({data.maxLevel})");
        }
    }
    
    public void UpdateCooldown(float deltaTime)
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= deltaTime;
            if (currentCooldown < 0) currentCooldown = 0;
        }
    }
    
    /// <summary>
    /// Hook called after skill is used - override for custom behavior
    /// </summary>
    protected virtual void OnSkillUsed(ISkillTarget user, ISkillTarget target) { }
    
    /// <summary>
    /// Hook called after skill levels up - override for custom behavior
    /// </summary>
    protected virtual void OnLevelUp() { }
    
    protected int CalculateManaCost()
    {
        return Mathf.RoundToInt(data.baseManaCost + (currentLevel - 1) * data.manaCostPerLevel);
    }
    
    protected float CalculateCooldown()
    {
        return Mathf.Max(0.5f, data.baseCooldown - (currentLevel - 1) * data.cooldownReductionPerLevel);
    }
}
