using System.Collections.Generic;
using UnityEngine;

public class SkillBook
{
    // Constructor
    public SkillBook()
    {
        skills = new List<Skill>();
        activeSkills = new List<Skill>();
        passiveSkills = new List<Skill>();
        warriorSkills = new List<Skill>();
        mageSkills = new List<Skill>();
        rogueSkills = new List<Skill>();
        InitializeSkills();
    }

    // Methods
    public void AddSkill(Skill skill)
    {
        skills.Add(skill);
        if (skill.IsPassive)
        {
            passiveSkills.Add(skill);
        }
        else
        {
            activeSkills.Add(skill);
        }

        // Add to class-specific list
        switch (skill.SkillType)
        {
            case SkillType.Warrior:
                warriorSkills.Add(skill);
                break;
            case SkillType.Mage:
                mageSkills.Add(skill);
                break;
            case SkillType.Rogue:
                rogueSkills.Add(skill);
                break;
        }
    }

    public void RemoveSkill(Skill skill)
    {
        skills.Remove(skill);
        if (skill.IsPassive)
        {
            passiveSkills.Remove(skill);
        }
        else
        {
            activeSkills.Remove(skill);
        }

        // Remove from class-specific list
        switch (skill.SkillType)
        {
            case SkillType.Warrior:
                warriorSkills.Remove(skill);
                break;
            case SkillType.Mage:
                mageSkills.Remove(skill);
                break;
            case SkillType.Rogue:
                rogueSkills.Remove(skill);
                break;
        }
    }

    public List<Skill> GetSkillsByType(SkillType type)
    {
        switch (type)
        {
            case SkillType.Warrior:
                return warriorSkills;
            case SkillType.Mage:
                return mageSkills;
            case SkillType.Rogue:
                return rogueSkills;
            default:
                return skills;
        }
    }

    public List<Skill> GetActiveSkills()
    {
        return activeSkills;
    }

    public List<Skill> GetPassiveSkills()
    {
        return passiveSkills;
    }

    private void InitializeSkills()
    {
        // Warrior Skills
        AddSkill(new Skill(SkillType.Warrior, "Power Strike", "A powerful melee attack", 1.5, 10));
        AddSkill(new Skill(SkillType.Warrior, "Shield Bash", "Bash with shield to stun", 1.2, 15));
        AddSkill(new Skill(SkillType.Warrior, "Berserker Rage", "Increase attack but reduce defense", 2.0, 20, true)); // Passive

        // Mage Skills
        AddSkill(new Skill(SkillType.Mage, "Fireball", "Launch a fireball", 2.0, 25));
        AddSkill(new Skill(SkillType.Mage, "Heal", "Restore health", 1.0, 20));
        AddSkill(new Skill(SkillType.Mage, "Mana Shield", "Absorb damage with mana", 1.5, 30, true)); // Passive

        // Rogue Skills
        AddSkill(new Skill(SkillType.Rogue, "Backstab", "Attack from behind for bonus damage", 2.5, 15));
        AddSkill(new Skill(SkillType.Rogue, "Stealth", "Become invisible temporarily", 1.0, 10));
        AddSkill(new Skill(SkillType.Rogue, "Pickpocket", "Steal gold from enemies", 1.0, 5, true)); // Passive
    }

    //private variables
    private List<Skill> skills;
    private List<Skill> activeSkills;
    private List<Skill> passiveSkills;
    private List<Skill> warriorSkills;
    private List<Skill> mageSkills;
    private List<Skill> rogueSkills;
}
