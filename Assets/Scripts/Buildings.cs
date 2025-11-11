using UnityEngine;

public enum BuildingType
{
    Barracks,
    Market,
    Wall,
    Tower,
    Farm,
    Mine
}

public class Buildings
{
    public BuildingType Type { get; set; }
    public string Name { get; set; }
    public int Level { get; set; } = 1;
    public int Cost { get; set; }
    public int BuildTime { get; set; } // in days
    public string Description { get; set; }
    public bool IsBuilt { get; set; } = false;

    // Effects on settlement
    public int IncomeBonus { get; set; }
    public int DefenseBonus { get; set; }
    public int PopulationBonus { get; set; }

    public Buildings(BuildingType type)
    {
        Type = type;
        Name = type.ToString();
        SetBuildingStats();
    }

    private void SetBuildingStats()
    {
        switch (Type)
        {
            case BuildingType.Barracks:
                Cost = 200;
                BuildTime = 3;
                Description = "Increases troop recruitment capacity.";
                DefenseBonus = 10;
                break;
            case BuildingType.Market:
                Cost = 300;
                BuildTime = 5;
                Description = "Increases trade income.";
                IncomeBonus = 50;
                break;
            case BuildingType.Wall:
                Cost = 500;
                BuildTime = 7;
                Description = "Improves settlement defense.";
                DefenseBonus = 50;
                break;
            case BuildingType.Tower:
                Cost = 400;
                BuildTime = 4;
                Description = "Provides ranged defense bonus.";
                DefenseBonus = 30;
                break;
            case BuildingType.Farm:
                Cost = 150;
                BuildTime = 2;
                Description = "Increases population growth.";
                PopulationBonus = 20;
                IncomeBonus = 20;
                break;
            case BuildingType.Mine:
                Cost = 250;
                BuildTime = 4;
                Description = "Provides resource income.";
                IncomeBonus = 40;
                break;
        }
    }

    public void Upgrade()
    {
        Level++;
        Cost = (int)(Cost * 1.5f);
        BuildTime = (int)(BuildTime * 1.2f);
        IncomeBonus = (int)(IncomeBonus * 1.3f);
        DefenseBonus = (int)(DefenseBonus * 1.3f);
        PopulationBonus = (int)(PopulationBonus * 1.3f);
    }
}
