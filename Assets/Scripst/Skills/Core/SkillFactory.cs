using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Factory for creating skills from SkillData ScriptableObjects
/// Centralizes skill instantiation and manages skill database
/// </summary>
public class SkillFactory
{
    private Dictionary<string, SkillData> skillDatabase;
    
    public SkillFactory()
    {
        LoadSkillDatabase();
    }
    
    /// <summary>
    /// Load all SkillData ScriptableObjects from Resources folder
    /// </summary>
    private void LoadSkillDatabase()
    {
        skillDatabase = new Dictionary<string, SkillData>();
        
        // Load all SkillData ScriptableObjects from Resources/Skills
        SkillData[] allSkills = Resources.LoadAll<SkillData>("Skills");
        
        foreach (var skillData in allSkills)
        {
            if (string.IsNullOrEmpty(skillData.skillId))
            {
                Debug.LogWarning($"SkillData {skillData.name} has no skillId set!");
                continue;
            }
            
            if (skillDatabase.ContainsKey(skillData.skillId))
            {
                Debug.LogWarning($"Duplicate skill ID found: {skillData.skillId}");
                continue;
            }
            
            skillDatabase[skillData.skillId] = skillData;
        }
        
        Debug.Log($"[SkillFactory] Loaded {skillDatabase.Count} skills from Resources");
    }
    
    /// <summary>
    /// Create a skill instance from skill ID
    /// </summary>
    public BaseSkill CreateSkill(string skillId)
    {
        if (!skillDatabase.ContainsKey(skillId))
        {
            Debug.LogError($"Skill with ID '{skillId}' not found in database");
            return null;
        }
        
        SkillData data = skillDatabase[skillId];
        
        // Create appropriate skill type based on data
        switch (data.skillType)
        {
            case SkillType.Warrior:
                return new WarriorSkill(data);
                
            case SkillType.Mage:
                return new MageSkill(data);
                
            case SkillType.Rogue:
                return new RogueSkill(data);
                
            default:
                return new GenericSkill(data);
        }
    }
    
    /// <summary>
    /// Get skill data without creating a skill instance
    /// </summary>
    public SkillData GetSkillData(string skillId)
    {
        return skillDatabase.ContainsKey(skillId) ? skillDatabase[skillId] : null;
    }
    
    /// <summary>
    /// Get all skill IDs in the database
    /// </summary>
    public List<string> GetAllSkillIds()
    {
        return new List<string>(skillDatabase.Keys);
    }
    
    /// <summary>
    /// Get all skills of a specific type
    /// </summary>
    public List<SkillData> GetSkillsByType(SkillType type)
    {
        List<SkillData> result = new List<SkillData>();
        
        foreach (var skillData in skillDatabase.Values)
        {
            if (skillData.skillType == type)
            {
                result.Add(skillData);
            }
        }
        
        return result;
    }
}
