using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton manager for the skill system
/// Handles skill cooldown updates and character registration
/// </summary>
public class SkillManager : MonoBehaviour
{
    private static SkillManager instance;
    public static SkillManager Instance => instance;
    
    private SkillFactory skillFactory;
    private List<Characters> registeredCharacters;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Initialize()
    {
        skillFactory = new SkillFactory();
        registeredCharacters = new List<Characters>();
        
        Debug.Log("[SkillManager] Initialized");
    }
    
    private void Update()
    {
        float deltaTime = Time.deltaTime;
        
        // Update all character skill cooldowns and status effects
        foreach (var character in registeredCharacters)
        {
            if (character != null)
            {
                character.UpdateSkillCooldowns(deltaTime);
                character.UpdateStatusEffects(deltaTime);
            }
        }
    }
    
    /// <summary>
    /// Register a character to receive automatic cooldown and status effect updates
    /// </summary>
    public void RegisterCharacter(Characters character)
    {
        if (character == null)
        {
            Debug.LogWarning("[SkillManager] Attempted to register null character");
            return;
        }
        
        if (!registeredCharacters.Contains(character))
        {
            registeredCharacters.Add(character);
            Debug.Log($"[SkillManager] Registered character: {character.Name}");
        }
    }
    
    /// <summary>
    /// Unregister a character (e.g., when character dies or is removed)
    /// </summary>
    public void UnregisterCharacter(Characters character)
    {
        if (registeredCharacters.Contains(character))
        {
            registeredCharacters.Remove(character);
            Debug.Log($"[SkillManager] Unregistered character: {character.Name}");
        }
    }
    
    /// <summary>
    /// Create a skill instance from skill ID
    /// </summary>
    public BaseSkill CreateSkill(string skillId)
    {
        return skillFactory.CreateSkill(skillId);
    }
    
    /// <summary>
    /// Teach a skill to a character
    /// </summary>
    public bool TeachSkill(Characters character, string skillId)
    {
        if (character == null)
        {
            Debug.LogWarning("[SkillManager] Cannot teach skill to null character");
            return false;
        }
        
        BaseSkill skill = CreateSkill(skillId);
        if (skill != null)
        {
            character.LearnSkill(skill);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Get skill data without creating an instance
    /// </summary>
    public SkillData GetSkillData(string skillId)
    {
        return skillFactory.GetSkillData(skillId);
    }
    
    /// <summary>
    /// Get all available skill IDs
    /// </summary>
    public List<string> GetAllSkillIds()
    {
        return skillFactory.GetAllSkillIds();
    }
    
    /// <summary>
    /// Get all skills of a specific type
    /// </summary>
    public List<SkillData> GetSkillsByType(SkillType type)
    {
        return skillFactory.GetSkillsByType(type);
    }
}
