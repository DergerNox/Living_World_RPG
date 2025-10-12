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
    //Constructor
    
    //private variables
    private SkillType skillType;
    private int skillLevel;
    private string skillName;
    private string skillDescription;
    private double skillModifier;
    private int skillCost;
    bool isPassive;
}
