using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

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
        IAssetProvider provider;
        if (!ServiceLocator.TryResolve<IAssetProvider>(out provider))
        {
            Object[] fallbackObjects = Resources.LoadAll("Skills");
            SkillData[] fallback = fallbackObjects.OfType<SkillData>().ToArray();
            IndexSkills(fallback);
            Debug.Log($"[SkillFactory] Loaded {skillDatabase.Count} skills from Resources (fallback)");
            return;
        }

        SkillData[] allSkills = provider.LoadAll<SkillData>("Skills");
        
        IndexSkills(allSkills);
        Debug.Log($"[SkillFactory] Loaded {skillDatabase.Count} skills via AssetProvider");
    }

    private void IndexSkills(IEnumerable<SkillData> skills)
    {
        foreach (var skillData in skills)
        {
            if (skillData == null) continue;
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
    }

    public async Task<int> LoadSkillDatabaseAsync()
    {
        skillDatabase = new Dictionary<string, SkillData>();
        IAssetProvider provider;
        if (!ServiceLocator.TryResolve<IAssetProvider>(out provider))
        {
            Object[] fallbackObjects = Resources.LoadAll("Skills");
            SkillData[] fallback = fallbackObjects.OfType<SkillData>().ToArray();
            IndexSkills(fallback);
            return skillDatabase.Count;
        }
        // Resources.LoadAll has no async bulk counterpart; for Addressables, this would change
        SkillData[] allSkills = provider.LoadAll<SkillData>("Skills");
        IndexSkills(allSkills);
        await Task.Yield();
        return skillDatabase.Count;
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
            case SkillType.Mage:
                return new MageSkill(data);

            case SkillType.Warrior:
                return new WarriorSkill(data);

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
