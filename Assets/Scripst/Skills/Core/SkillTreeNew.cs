using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages skill progression trees and learning requirements
/// No longer initializes skills - that's handled by SkillFactory
/// </summary>
public class SkillTreeNew
{
    private Dictionary<SkillType, List<SkillData>> skillTrees;
    private SkillFactory skillFactory;
    
    public SkillTreeNew()
    {
        skillFactory = new SkillFactory();
        LoadSkillTrees();
    }
    
    /// <summary>
    /// Load skill trees from SkillFactory (which loads from ScriptableObjects)
    /// </summary>
    private void LoadSkillTrees()
    {
        skillTrees = new Dictionary<SkillType, List<SkillData>>();
        
        // Initialize empty lists for each skill type
        skillTrees[SkillType.Warrior] = new List<SkillData>();
        skillTrees[SkillType.Mage] = new List<SkillData>();
        skillTrees[SkillType.Rogue] = new List<SkillData>();
        
        // Get all skills and organize by type
        foreach (SkillType type in System.Enum.GetValues(typeof(SkillType)))
        {
            List<SkillData> skillsOfType = skillFactory.GetSkillsByType(type);
            
            // Sort by required level
            skillsOfType.Sort((a, b) => a.requiredLevel.CompareTo(b.requiredLevel));
            
            skillTrees[type] = skillsOfType;
        }
        
        Debug.Log($"[SkillTree] Loaded skill trees for {skillTrees.Count} classes");
    }
    
    /// <summary>
    /// Get all skills in a skill tree for a specific class
    /// </summary>
    public List<SkillData> GetSkillTree(SkillType type)
    {
        return skillTrees.ContainsKey(type) ? new List<SkillData>(skillTrees[type]) : new List<SkillData>();
    }
    
    /// <summary>
    /// Check if a character can learn a specific skill
    /// </summary>
    public bool CanLearnSkill(SkillData skillData, Characters character)
    {
        if (skillData == null || character == null)
            return false;
        
        // Check if already learned
        if (character.HasSkill(skillData.skillId))
        {
            Debug.Log($"{character.Name} already knows {skillData.skillName}");
            return false;
        }
        
        // Check level requirement
        if (character.Level < skillData.requiredLevel)
        {
            Debug.Log($"{character.Name} needs to be level {skillData.requiredLevel} to learn {skillData.skillName} (current: {character.Level})");
            return false;
        }
        
        // Check prerequisites
        foreach (string prereqId in skillData.prerequisiteSkillIds)
        {
            if (!character.HasSkill(prereqId))
            {
                SkillData prereqData = skillFactory.GetSkillData(prereqId);
                string prereqName = prereqData != null ? prereqData.skillName : prereqId;
                Debug.Log($"{character.Name} must learn {prereqName} before learning {skillData.skillName}");
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Attempt to teach a skill to a character
    /// </summary>
    public bool LearnSkill(string skillId, Characters character)
    {
        SkillData skillData = skillFactory.GetSkillData(skillId);
        if (skillData == null)
        {
            Debug.LogError($"Skill with ID '{skillId}' not found");
            return false;
        }
        
        if (!CanLearnSkill(skillData, character))
        {
            return false;
        }
        
        BaseSkill skill = skillFactory.CreateSkill(skillId);
        if (skill != null)
        {
            character.LearnSkill(skill);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Get all skills a character can currently learn
    /// </summary>
    public List<SkillData> GetAvailableSkills(Characters character, SkillType? filterType = null)
    {
        List<SkillData> available = new List<SkillData>();
        
        IEnumerable<SkillType> typesToCheck = filterType.HasValue 
            ? new[] { filterType.Value } 
            : System.Enum.GetValues(typeof(SkillType)) as SkillType[];
        
        foreach (SkillType type in typesToCheck)
        {
            if (skillTrees.ContainsKey(type))
            {
                foreach (SkillData skillData in skillTrees[type])
                {
                    if (CanLearnSkill(skillData, character))
                    {
                        available.Add(skillData);
                    }
                }
            }
        }
        
        return available;
    }
}
