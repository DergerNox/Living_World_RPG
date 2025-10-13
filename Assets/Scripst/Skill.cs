using UnityEngine;

public class Skill
{
    //public variables
    public SkillType SkillType { get => skillType; set => skillType = value; }
    public int SkillLevel { get => skillLevel; set => skillLevel = value; }
    public string SkillName { get => skillName; set => skillName = value; }
    public string SkillDescription { get => skillDescription; set => skillDescription = value; }
    public double SkillModifier { get => skillModifier; set => skillModifier = value; }
    public int SkillCost { get => skillCost; set => skillCost = value; }
    public bool IsPassive { get => isPassive; set => isPassive = value; }

    // Constructor
    public Skill(SkillType type, string name, string description, double modifier, int cost, bool passive = false)
    {
        skillType = type;
        skillName = name;
        skillDescription = description;
        skillModifier = modifier;
        skillCost = cost;
        isPassive = passive;
        skillLevel = 1;
    }

    // Methods
    public void UseSkill(Characters user)
    {
        if (user.Mana >= skillCost)
        {
            user.Mana -= skillCost;
            ApplyEffect(user);
            Debug.Log($"{user.Name} used skill: {skillName}");
        }
        else
        {
            Debug.Log($"{user.Name} does not have enough mana to use {skillName}");
        }
    }

    private void ApplyEffect(Characters user)
    {
        switch (skillType)
        {
            case SkillType.Warrior:
                // Warrior skills: increase attack
                user.Attack += (int)(skillModifier * skillLevel);
                break;
            case SkillType.Mage:
                // Mage skills: deal magic damage or heal
                if (skillName.Contains("Heal"))
                {
                    user.Health += (int)(skillModifier * skillLevel);
                    if (user.Health > user.MaxHealth) user.Health = user.MaxHealth;
                }
                else
                {
                    // Assume damage to target (simplified)
                    Debug.Log($"Dealt {(int)(skillModifier * skillLevel)} magic damage");
                }
                break;
            case SkillType.Rogue:
                // Rogue skills: increase critical chance or stealth
                // Simplified: increase attack temporarily
                user.Attack += (int)(skillModifier * skillLevel);
                break;
        }
    }

    public void LevelUp()
    {
        skillLevel++;
        skillModifier += 0.1; // Increase modifier per level
    }

    //private variables
    private SkillType skillType;
    private int skillLevel;
    private string skillName;
    private string skillDescription;
    private double skillModifier;
    private int skillCost;
    bool isPassive;
}
