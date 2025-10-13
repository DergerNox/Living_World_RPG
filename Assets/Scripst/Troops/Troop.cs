using UnityEngine;

public class Troop
{
    // Public Properties
    public string Name { get => name; set => name = value; }
    public int Damage { get => damage; set => damage = value; }
    public int Health { get => health; set => health = value; }
    public int Armor { get => armor; set => armor = value; }
    public int Speed { get => speed; set => speed = value; }
    public int Experience { get => experience; set => experience = value; }
    public int Level { get => level; set => level = value; }
    public bool IsAlive { get => isAlive; set => isAlive = value; }
    public TroopType TroopType { get => troopType; set => troopType = value; }
    public TroopRank Rank { get => rank; set => rank = value; }
    public int Cost { get => cost; set => cost = value; }
    public Sprite UnitImage { get => unitImage; private set => unitImage = value; }
    public SkillBook SkillBook { get; set; } = new SkillBook();
    public int Attack { get; set; } = 10;
    public int Defense { get; set; } = 5;

    // Special abilities
    public List<SpecialAbility> SpecialAbilities { get; set; } = new List<SpecialAbility>();

    // Private Fields
    private string name;
    private TroopType troopType;
    private TroopRank rank;
    private int damage;
    private int health;
    private int armor;
    private int speed;
    private int experience;
    private int level;
    private bool isAlive;
    private int cost;
    private Sprite unitImage; // To store the troop's image

    // Constructor
    public Troop(TroopType type)
    {
        troopType = type;
        level = 1;
        experience = 0;
        isAlive = true;
        SetBaseStatsByType(type);
        NameGenerator nameGenerator = new NameGenerator();
        name = nameGenerator.GenerateName() + " " + troopType.ToString();
        UpdateRank(); // Set initial rank based on level

        // Assign the correct image based on type and rank
        UnitImage = GetTroopImage(troopType, rank);

        // Initialize special abilities based on type
        InitializeSpecialAbilities();
    }

    // Methods
    public void GainExperience(int amount)
    {
        experience += amount;

        // Example simple level-up system
        if (experience >= level * 100)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        switch (troopType)
        {
            case TroopType.Infantry:
                health += 10;
                armor += 2;
                damage += 3;
                break;
            case TroopType.Archer:
                damage += 5;
                speed += 1;
                break;
            case TroopType.Cavalry:
                health += 5;
                speed += 2;
                damage += 4;
                break;
            case TroopType.Engineer:
                damage += 2;
                armor += 1;
                break;
        }

        UpdateRank();
    }

    public void UpdateRank()
    {
        switch (troopType)
        {
            case TroopType.Infantry:
                rank = level switch
                {
                    1 => TroopRank.Recruit,
                    2 => TroopRank.Footman,
                    3 => TroopRank.Veteran,
                    4 => TroopRank.Sergeant,
                    _ => TroopRank.Champion
                };
                break;

            case TroopType.Cavalry:
                rank = level switch
                {
                    1 => TroopRank.Rider,
                    2 => TroopRank.Horseman,
                    3 => TroopRank.Lancer,
                    4 => TroopRank.Knight,
                    _ => TroopRank.Cavalier
                };
                break;

            case TroopType.Archer:
                rank = level switch
                {
                    1 => TroopRank.Bowman,
                    2 => TroopRank.Archer,
                    3 => TroopRank.Sharpshooter,
                    4 => TroopRank.Ranger,
                    _ => TroopRank.Marksman
                };
                break;

            case TroopType.Engineer:
                rank = level switch
                {
                    1 => TroopRank.Tinkerer,
                    2 => TroopRank.Mechanic,
                    3 => TroopRank.Engineer,
                    4 => TroopRank.SiegeMaster,
                    _ => TroopRank.GrandEngineer
                };
                break;
        }

        // Reassign the image after rank update
        UnitImage = GetTroopImage(troopType, rank);
    }

    private void SetBaseStatsByType(TroopType type)
    {
        switch (type)
        {
            case TroopType.Infantry:
                damage = 10;
                health = 50;
                armor = 5;
                speed = 2;
                cost = 50;
                break;
            case TroopType.Archer:
                damage = 15;
                health = 30;
                armor = 2;
                speed = 3;
                cost = 60;
                break;
            case TroopType.Cavalry:
                damage = 12;
                health = 40;
                armor = 4;
                speed = 4;
                cost = 100;
                break;
            case TroopType.Engineer:
                damage = 8;
                health = 35;
                armor = 3;
                speed = 2;
                cost = 80;
                break;
        }
    }

    // Method to get the image based on type and rank
    public Sprite GetTroopImage(TroopType type, TroopRank rank)
{
    // Format the image name based on troop type and rank (both converted to strings)
    string imageName = $"{type.ToString()}_{rank.ToString()}";

    // Debug log to check the image name
    Debug.Log($"Looking for image: {imageName}");

    // Load the sprite (assuming the images are stored in a Resources folder)
    Sprite sprite = UnityEngine.Resources.Load<Sprite>($"Sprites/TroopSprites/{imageName}");

    if (sprite == null)
    {
        Debug.LogError($"Image not found for {imageName}");
    }

    return sprite;
}

    private void InitializeSpecialAbilities()
    {
        switch (troopType)
        {
            case TroopType.Infantry:
                SpecialAbilities.Add(new SpecialAbility("Shield Wall", "Increases defense for all nearby infantry", AbilityType.Defensive, 3f, (user, target) =>
                {
                    // Implement shield wall logic - increase defense temporarily
                    user.Defense += 5;
                    Debug.Log($"{user.Name} uses Shield Wall!");
                }));
                break;
            case TroopType.Archer:
                SpecialAbilities.Add(new SpecialAbility("Volley Fire", "Fires multiple arrows at once", AbilityType.Offensive, 4f, (user, target) =>
                {
                    // Implement volley fire - increased damage
                    int extraDamage = user.Damage;
                    target.Health -= extraDamage;
                    Debug.Log($"{user.Name} uses Volley Fire on {target.Name}!");
                }));
                break;
            case TroopType.Cavalry:
                SpecialAbilities.Add(new SpecialAbility("Charge", "Powerful charge attack with bonus damage", AbilityType.Offensive, 5f, (user, target) =>
                {
                    // Implement charge - bonus damage
                    int chargeDamage = user.Damage * 2;
                    target.Health -= chargeDamage;
                    Debug.Log($"{user.Name} charges at {target.Name}!");
                }));
                break;
            case TroopType.Engineer:
                SpecialAbilities.Add(new SpecialAbility("Repair", "Repairs nearby structures or heals allies", AbilityType.Support, 6f, (user, target) =>
                {
                    // Implement repair - heal target
                    target.Health += 20;
                    if (target.Health > 100) target.Health = 100; // Assuming max health
                    Debug.Log($"{user.Name} repairs {target.Name}!");
                }));
                break;
        }
    }

}
