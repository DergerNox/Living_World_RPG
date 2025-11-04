using UnityEngine;

/// <summary>
/// Interface for skill effects that can be applied to targets
/// </summary>
public interface ISkillEffect
{
    string EffectName { get; }
    void Apply(ISkillTarget user, ISkillTarget target);
    bool Validate(ISkillTarget user, ISkillTarget target);
}
