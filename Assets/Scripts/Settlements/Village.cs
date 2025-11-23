using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class Village : ISettlement
{
    // Implementing ISettlement interface
    public int PosX => Position.x;  // PosX is the x coordinate of the Position
    public int PosY => Position.y;  // PosY is the y coordinate of the Position
    public int FactionId { get; set; }

    // Private field for position
    public Vector3Int Position { get; private set; }
    public string Name { get; private set; }

    // Additional properties specific to Village
    public int Population { get; set; }
    public int Resources { get; set; }
    public int MaxMilitiaSize { get; set; }
    public int CurrentMilitiaSize { get; set; }
    public List<Party> VisitingParties { get; set; }
    public List<Buildings> Buildings { get; set; }
    public List<Characters> VisitingCharacters { get; set; }
    public Characters Leader { get; set; }
    public List<Outpost> OutpostConnected { get; set; }
    public SettlementResourceLedger ResourceLedger { get; }
    public Town ConnectedTown { get; internal set; }
    public int VillageLevel { get; private set; } = 1;
    public IReadOnlyList<ResourceType> ProductionTypes => productionTypes;
    public int Treasury { get; set; }
    public int GetDailyProduction(ResourceType type)
    {
        if (!productionTypes.Contains(type))
        {
            return 0;
        }
        int idx = Mathf.Clamp(VillageLevel - 1, 0, productionPerLevel.Length - 1);
        return productionPerLevel[idx];
    }

    [Obsolete("Use VillageLevel instead.")]
    public int TownLevel
    {
        get => VillageLevel;
        set => SetVillageLevel(value);
    }

    // Recruitment pool for troops
    public List<Troop> RecruitmentPool { get; set; }

    // Constructor
    public Village(int x, int y)
    {
        Position = new Vector3Int(x, y, 0);
        Name = "Village";  // Default name, you can customize it later
        Population = 100;
        Resources = 100;
        MaxMilitiaSize = 100;
        CurrentMilitiaSize = 0;
        Treasury = 0;
        VisitingParties = new List<Party>();
        Buildings = new List<Buildings>();
        VisitingCharacters = new List<Characters>();
        OutpostConnected = new List<Outpost>();
        FactionId = -1;  // Assuming -1 means no faction assigned initially
        VillageLevel = 1;
        ResourceLedger = new SettlementResourceLedger();
        RefreshProductionSelection();
        RecruitmentPool = new List<Troop>();
        AddRank1And2Troops();

        // Initialize with basic buildings
        InitializeBuildings();
    }

    // Method to add rank 1 and 2 troops to the village's recruitment pool
    private void AddRank1And2Troops()
    {
        // Add Rank 1 and Rank 2 troops for each TroopType
        foreach (TroopType troopType in System.Enum.GetValues(typeof(TroopType)))
        {
            // Add Rank 1 troop
            Troop rank1Troop = new Troop(troopType);
            rank1Troop.Level = 1;
            rank1Troop.UpdateRank(); // Ensure the rank is updated based on level
            RecruitmentPool.Add(rank1Troop);

            // Add Rank 2 troop
            Troop rank2Troop = new Troop(troopType);
            rank2Troop.Level = 2;
            rank2Troop.UpdateRank(); // Ensure the rank is updated based on level
            RecruitmentPool.Add(rank2Troop);
        }
    }

    private void InitializeBuildings()
    {
        // Add basic buildings for village
        Buildings.Add(new Buildings(BuildingType.Farm) { IsBuilt = true });
    }

    public void BuildBuilding(BuildingType type)
    {
        Buildings newBuilding = new Buildings(type);
        // Villages have limited building capacity
        if (Buildings.Count < 3)
        {
            Buildings.Add(newBuilding);
            ApplyBuildingEffects(newBuilding);
            Debug.Log($"Built {newBuilding.Name} in {Name}.");
        }
        else
        {
            Debug.Log("Village cannot support more buildings!");
        }
    }

    private void ApplyBuildingEffects(Buildings building)
    {
        Population += building.PopulationBonus;
        Resources += building.IncomeBonus;
    }

    public void ProcessGrowthEvent()
    {
        // Simulate population growth
        Population += UnityEngine.Random.Range(1, 5);
        Resources += Population / 10; // Resources based on population
        Debug.Log($"{Name} grew! Population: {Population}, Resources: {Resources}");
    }

    public void ProcessSiegeEvent()
    {
        // Villages are vulnerable during sieges
        if (UnityEngine.Random.value < 0.3f) // 30% chance of being attacked
        {
            Population -= UnityEngine.Random.Range(10, 30);
            if (Population < 0) Population = 0;
            Debug.Log($"{Name} suffered during siege! Population now: {Population}");
        }
    }

    public void SetVillageLevel(int level)
    {
        VillageLevel = Mathf.Clamp(level, 1, MaxVillageLevel);
        RefreshProductionSelection();
    }

    public bool TryUpgradeVillageLevel()
    {
        if (VillageLevel >= MaxVillageLevel)
        {
            return false;
        }

        VillageLevel++;
        RefreshProductionSelection();
        return true;
    }

    public int ProduceResources(int days = 1)
    {
        days = Mathf.Max(1, days);
        int produced = 0;
        int perResource = productionPerLevel[Mathf.Clamp(VillageLevel - 1, 0, productionPerLevel.Length - 1)];
        foreach (var type in productionTypes)
        {
            int amount = perResource * days;
            ResourceLedger.AddBasic(type, amount);
            produced += amount;
        }

        Resources += produced;
        return produced;
    }

    private const int MaxVillageLevel = 3;
    private static readonly int[] productionPerLevel = { 6, 12, 20 };
    private static readonly ResourceType[] resourcePool =
    {
        ResourceType.Food,
        ResourceType.Wood,
        ResourceType.Stone,
        ResourceType.Iron,
        ResourceType.Coal,
        ResourceType.Clay,
        ResourceType.Textile,
        ResourceType.Herbs,
        ResourceType.Salt
    };
    private readonly List<ResourceType> productionTypes = new List<ResourceType>();

    private void RefreshProductionSelection()
    {
        int desired = Mathf.Clamp(VillageLevel, 1, MaxVillageLevel);
        var ordered = resourcePool
            .OrderBy(type => GetResourceScore(type))
            .ToList();

        productionTypes.Clear();
        for (int i = 0; i < desired && i < ordered.Count; i++)
        {
            productionTypes.Add(ordered[i]);
        }
    }

    private int GetResourceScore(ResourceType type)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + Position.x;
            hash = hash * 31 + Position.y;
            hash = hash * 31 + (int)type * 397;
            return hash;
        }
    }
}
