using UnityEngine;

/// <summary>
/// Interface for all skills in the game
/// </summary>
public interface ISkill
{
    string SkillId { get; }
    string SkillName { get; }
    string Description { get; }
    SkillType SkillType { get; }
    int Level { get; }
    int ManaCost { get; }
    float Cooldown { get; }
    bool IsPassive { get; }
    bool IsOnCooldown { get; }
    
    bool CanUse(ISkillTarget user);
    void Use(ISkillTarget user, ISkillTarget target = null);
    void LevelUp();
    void UpdateCooldown(float deltaTime);
}
