using System;
using UnityEngine;

/// <summary>
/// Data structure for skill effects
/// </summary>
[Serializable]
public class SkillEffectData
{
    public SkillEffectType effectType;
    public float baseValue;
    public float scalingPerLevel;
    public TargetType targetType;
    public string statusEffectId; // For status effects
}

public enum SkillEffectType
{
    Damage,
    Heal,
    BuffAttack,
    BuffDefense,
    DebuffAttack,
    DebuffDefense,
    ApplyStatusEffect,
    RestoreMana,
    DrainMana,
    Teleport
}

public enum TargetType
{
    Self,
    SingleEnemy,
    SingleAlly,
    AllEnemies,
    AllAllies,
    Area
}
