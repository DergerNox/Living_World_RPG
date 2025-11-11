using System.Runtime.CompilerServices;
using UnityEngine;
using Random=System.Random;

public class Weapons
{
    //public Variables
    public string Name { get => name; set => name = value; }
    public WeaponTypes WeaponType { get => weaponType; set => weaponType = value; }
    public int Damage { get => damage; set => damage = value; }
    public int Price { get => price; set => price = value; }
    public int WeaponLevel { get => weaponLevel; set => weaponLevel = value; }
    public Skill SkillRequirement { get => skillRequirement; set => skillRequirement = value; }
    public int SkillLevelRequirement { get => skillLevelRequirement; set => skillLevelRequirement = value; }
    public Resources RequredResourceA { get => requredResourceA; set => requredResourceA = value; }
    public int MinInputResourceA { get => minInputResourceA; set => minInputResourceA = value; }
    public int MaxInputResourceA { get => maxInputResourceA; set => maxInputResourceA = value; }
    public int CurrentAmountOfInputResourceA { get => currentAmountOfInputResourceA; set => currentAmountOfInputResourceA = value; }
    public Resources RequredResourceB { get => requredResourceB; set => requredResourceB = value; }
    public int MinInputResourceB { get => minInputResourceB; set => minInputResourceB = value; }
    public int MaxInputResourceB { get => maxInputResourceB; set => maxInputResourceB = value; }
    public int CurrentAmountOfInputResourceB { get => currentAmountOfInputResourceB; set => currentAmountOfInputResourceB = value; }
    //Public Methods
    //Constructor
    public Weapons(WeaponTypes i_weaponType)
    {
        NameGenerator nameGenerator = new NameGenerator();
        name=nameGenerator.GenerateName()+" "+i_weaponType.ToString();
        weaponType = i_weaponType;
    }
    //private Variables
    private string name;
    private WeaponTypes weaponType;
    private int damage;
    private int price;
    private int weaponLevel;
    private Skill skillRequirement;
    private int skillLevelRequirement;
    private Resources requredResourceA;
    private int minInputResourceA;
    private int maxInputResourceA;
    private int currentAmountOfInputResourceA;
    private Resources requredResourceB;
    private int minInputResourceB;
    private int maxInputResourceB;
    private int currentAmountOfInputResourceB;
    
}
