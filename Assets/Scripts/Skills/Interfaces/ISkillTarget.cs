using UnityEngine;

/// <summary>
/// Interface for any entity that can be targeted by skills (characters, enemies, NPCs, etc.)
/// </summary>
public interface ISkillTarget
{
    string Name { get; }
    int Health { get; set; }
    int MaxHealth { get; }
    int Mana { get; set; }
    int MaxMana { get; }
    int Attack { get; set; }
    int Defense { get; set; }
    
    void TakeDamage(int damage);
    void Heal(int amount);
    void ApplyStatusEffect(IStatusEffect effect);
}
