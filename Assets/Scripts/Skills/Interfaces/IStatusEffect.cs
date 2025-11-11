using UnityEngine;

/// <summary>
/// Interface for status effects (buffs, debuffs, DoTs, etc.)
/// </summary>
public interface IStatusEffect
{
    string EffectName { get; }
    float Duration { get; }
    bool IsExpired { get; }
    
    void Apply(ISkillTarget target);
    void Update(float deltaTime);
    void Remove(ISkillTarget target);
}
