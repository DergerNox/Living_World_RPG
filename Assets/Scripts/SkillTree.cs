using System.Collections.Generic;
using UnityEngine;

public class SkillTree
{
    private Dictionary<SkillType, List<Skill>> skillTrees;
    private readonly Dictionary<Characters, List<Skill>> learnedLegacySkills = new Dictionary<Characters, List<Skill>>();

    public SkillTree()
    {
        skillTrees = new Dictionary<SkillType, List<Skill>>();
        InitializeSkillTrees();
    }

    public List<Skill> GetSkillTree(SkillType type)
    {
        if (skillTrees.ContainsKey(type))
        {
            return skillTrees[type];
        }
        return new List<Skill>();
    }

    public bool CanLearnSkill(Skill skill, Characters character)
    {
        // Simple check: level requirement
        return character.Level >= skill.SkillLevel;
    }

    public void LearnSkill(Skill skill, Characters character)
    {
        if (!CanLearnSkill(skill, character))
        {
            Debug.Log($"{character.Name} cannot learn {skill.SkillName} yet");
            return;
        }

        var list = GetOrCreateLegacySkillList(character);
        if (!list.Contains(skill))
        {
            list.Add(skill);
        }
        Debug.Log($"{character.Name} learned legacy skill {skill.SkillName}");
    }

    public IReadOnlyList<Skill> GetLegacySkills(Characters character)
    {
        return learnedLegacySkills.TryGetValue(character, out var list)
            ? list.AsReadOnly()
            : new List<Skill>().AsReadOnly();
    }

    private List<Skill> GetOrCreateLegacySkillList(Characters character)
    {
        if (!learnedLegacySkills.TryGetValue(character, out var list))
        {
            list = new List<Skill>();
            learnedLegacySkills[character] = list;
        }
        return list;
    }

    private void InitializeSkillTrees()
    {
        // Warrior Skill Tree
        List<Skill> warriorTree = new List<Skill>
        {
            new Skill(SkillType.Warrior, "Power Strike", "A powerful melee attack", 1.5, 10),
            new Skill(SkillType.Warrior, "Shield Bash", "Bash with shield to stun", 1.2, 15),
            new Skill(SkillType.Warrior, "Berserker Rage", "Increase attack but reduce defense", 2.0, 20, true),
            new Skill(SkillType.Warrior, "Charge", "Rush towards enemy", 1.8, 25),
            new Skill(SkillType.Warrior, "Fortress", "Greatly increase defense", 0.5, 30, true)
        };
        skillTrees[SkillType.Warrior] = warriorTree;

        // Mage Skill Tree
        List<Skill> mageTree = new List<Skill>
        {
            new Skill(SkillType.Mage, "Fireball", "Launch a fireball", 2.0, 25),
            new Skill(SkillType.Mage, "Heal", "Restore health", 1.0, 20),
            new Skill(SkillType.Mage, "Mana Shield", "Absorb damage with mana", 1.5, 30, true),
            new Skill(SkillType.Mage, "Lightning Bolt", "Strike with lightning", 2.5, 35),
            new Skill(SkillType.Mage, "Teleport", "Instantly move to a location", 1.0, 40)
        };
        skillTrees[SkillType.Mage] = mageTree;

        // Rogue Skill Tree
        List<Skill> rogueTree = new List<Skill>
        {
            new Skill(SkillType.Rogue, "Backstab", "Attack from behind for bonus damage", 2.5, 15),
            new Skill(SkillType.Rogue, "Stealth", "Become invisible temporarily", 1.0, 10),
            new Skill(SkillType.Rogue, "Pickpocket", "Steal gold from enemies", 1.0, 5, true),
            new Skill(SkillType.Rogue, "Poison Dagger", "Apply poison to weapon", 1.3, 20),
            new Skill(SkillType.Rogue, "Shadow Step", "Teleport behind enemy", 1.0, 25)
        };
        skillTrees[SkillType.Rogue] = rogueTree;
    }
}
