using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject for skill data - allows designers to create skills in Unity Editor
/// </summary>
[CreateAssetMenu(fileName = "NewSkill", menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillId;
    public string skillName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    public SkillType skillType;
    
    [Header("Stats")]
    public int baseManaCost = 10;
    public float baseCooldown = 5f;
    public bool isPassive = false;
    public int maxLevel = 5;
    
    [Header("Effects")]
    public List<SkillEffectData> effects = new List<SkillEffectData>();
    
    [Header("Requirements")]
    public int requiredLevel = 1;
    public List<string> prerequisiteSkillIds = new List<string>();
    
    [Header("Scaling")]
    public float manaCostPerLevel = 5f;
    public float cooldownReductionPerLevel = 0.1f;
}
