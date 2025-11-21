using System.Collections.Generic;
using UnityEngine;
using LivingWorldRPG.Core;

public class Town : ISettlement
{
    // ISettlement Interface Implementation
    public int PosX => Position.x;  // PosX is the x coordinate of the Position
    public int PosY => Position.y;  // PosY is the y coordinate of the Position
    public int FactionId { get; set; }

    // Private field for position
    public Vector3Int Position { get; private set; }
    public string Name { get; private set; }

    // Additional properties specific to Town
    public int TownLevel { get; set; }
    public int MaxGarrisonSize { get; set; }
    public int CurrentGarrisonSize { get; set; }
    public List<Characters> Visitors { get; set; }
    public Characters Lord { get; set; }
    public List<Party> PartiesInTown { get; set; }
    public List<Buildings> BuildingsInTown { get; set; }
    public Queue<Buildings> BuildingsQueue { get; set; }
    public bool IsTownUnderSiege { get; set; }
    public List<Troop> AvailableUnits { get; set; }
    public Market Market { get; set; } = new Market();
    public int Income { get; set; }
    public int Expenses { get; set; }
    public int Taxes { get; set; }
    public SettlementResourceLedger ResourceLedger { get; }
    public IReadOnlyList<Village> SupplyVillages => supplyVillages;
    public Castle ConnectedCastle { get; internal set; }
    public int Treasury { get; private set; }

    // Constructor
    public Town(int x, int y)
    {
        Position = new Vector3Int(x, y, 0);
        Name = "Town";  // Default name, you can customize it later
        TownLevel = 1;
        MaxGarrisonSize = 300;
        CurrentGarrisonSize = 0;
        Visitors = new List<Characters>();
        AvailableUnits = new List<Troop>();
        PartiesInTown = new List<Party>();
        BuildingsInTown = new List<Buildings>();
        BuildingsQueue = new Queue<Buildings>();
        IsTownUnderSiege = false;
        FactionId = -1;  // Assuming -1 means no faction assigned initially
        ResourceLedger = new SettlementResourceLedger();
        Treasury = 0;

        // Add rank 2 and 3 troops to AvailableUnits
        AddRank2And3Troops();

        // Initialize market
        if (GameFeatureFlags.TradingEnabled)
        {
            Market.AddItem(new ItemData { name = "Sword", type = ItemType.Weapon, value = 100 });
            Market.AddItem(new ItemData { name = "Armor", type = ItemType.Armor, value = 150 });
            Market.AddItem(new ItemData { name = "Health Potion", type = ItemType.Consumable, value = 50 });
        }

        Income = 1000;
        Expenses = 500;
        Taxes = 200;

        // Initialize with basic buildings
        InitializeBuildings();
    }

    // Method to add rank 2 and 3 troops to the town's available units
   private void AddRank2And3Troops()
{
    // Add Rank 2 and Rank 3 troops for each TroopType
    foreach (TroopType troopType in System.Enum.GetValues(typeof(TroopType)))
    {
        // Add Rank 2 troop
        Troop rank2Troop = new Troop(troopType);
        rank2Troop.Level = 2;
        rank2Troop.UpdateRank(); // Ensure the rank is updated based on level
        AvailableUnits.Add(rank2Troop);

        // Add Rank 3 troop
        Troop rank3Troop = new Troop(troopType);
        rank3Troop.Level = 3;
        rank3Troop.UpdateRank(); // Ensure the rank is updated based on level
        AvailableUnits.Add(rank3Troop);
    }
}

    private void InitializeBuildings()
    {
        // Add basic buildings
        BuildingsInTown.Add(new Buildings(BuildingType.Farm) { IsBuilt = true });
        BuildingsInTown.Add(new Buildings(BuildingType.Market) { IsBuilt = true });
    }

    public void BuildBuilding(BuildingType type)
    {
        Buildings newBuilding = new Buildings(type);
        if (BuildingsQueue.Count < 3) // Max 3 buildings in queue
        {
            BuildingsQueue.Enqueue(newBuilding);
            Debug.Log($"Started building {newBuilding.Name} in {Name}. Build time: {newBuilding.BuildTime} days.");
        }
        else
        {
            Debug.Log("Building queue is full!");
        }
    }

    public void ProcessBuildingQueue()
    {
        if (BuildingsQueue.Count > 0)
        {
            Buildings building = BuildingsQueue.Peek();
            building.BuildTime--;
            if (building.BuildTime <= 0)
            {
                BuildingsQueue.Dequeue();
                building.IsBuilt = true;
                BuildingsInTown.Add(building);
                ApplyBuildingEffects(building);
                Debug.Log($"{building.Name} completed in {Name}!");
            }
        }
    }

    private void ApplyBuildingEffects(Buildings building)
    {
        Income += building.IncomeBonus;
        // Other effects can be applied here
    }

    public int GetTotalDefenseBonus()
    {
        int totalBonus = 0;
        foreach (var building in BuildingsInTown)
        {
            if (building.IsBuilt)
            {
                totalBonus += building.DefenseBonus;
            }
        }
        return totalBonus;
    }

    public void CollectTaxes()
    {
        int collectedTaxes = Taxes;
        Income += collectedTaxes;
        Debug.Log($"Collected {collectedTaxes} gold in taxes from {Name}.");
    }

    public void ProcessSiegeEvent()
    {
        if (IsTownUnderSiege)
        {
            // Reduce population or resources during siege
            // This could trigger battles or other events
            Debug.Log($"{Name} is under siege!");
        }
    }

    public int ProcessEconomyTick(int days = 1)
    {
        days = Mathf.Max(1, days);
        int profit = 0;
        int processingCapacity = Mathf.Max(1, TownLevel) * days * 3;

        foreach (var recipe in refinementRecipes)
        {
            if (processingCapacity <= 0)
            {
                break;
            }

            int availableA = ResourceLedger.GetBasic(recipe.InputA);
            int availableB = ResourceLedger.GetBasic(recipe.InputB);
            int batches = Mathf.Min(processingCapacity, availableA / recipe.CostA, availableB / recipe.CostB);
            if (batches <= 0)
            {
                continue;
            }

            int consumedA = batches * recipe.CostA;
            int consumedB = batches * recipe.CostB;
            ResourceLedger.ConsumeBasic(recipe.InputA, consumedA);
            ResourceLedger.ConsumeBasic(recipe.InputB, consumedB);
            ResourceLedger.AddAdvanced(recipe.Output, batches);
            profit += recipe.ProfitPerBatch * batches;
            processingCapacity -= batches;
        }

        if (profit > 0)
        {
            Treasury += profit;
            Income += profit;
        }

        return profit;
    }

    public void AddTreasury(int amount)
    {
        Treasury = Mathf.Max(0, Treasury + amount);
    }

    internal void RegisterVillageSupply(Village village)
    {
        if (village == null || supplyVillages.Contains(village))
        {
            return;
        }

        supplyVillages.Add(village);
    }

    internal void ResetSupplyNetwork()
    {
        supplyVillages.Clear();
        ConnectedCastle = null;
    }

    private readonly List<Village> supplyVillages = new List<Village>();

    public readonly struct RefinementRecipe
    {
        public RefinementRecipe(ResourceType inputA, ResourceType inputB, AdvancedResourcesTypes output, int costA, int costB, int profitPerBatch)
        {
            InputA = inputA;
            InputB = inputB;
            Output = output;
            CostA = Mathf.Max(1, costA);
            CostB = Mathf.Max(1, costB);
            ProfitPerBatch = Mathf.Max(0, profitPerBatch);
        }

        public ResourceType InputA { get; }
        public ResourceType InputB { get; }
        public AdvancedResourcesTypes Output { get; }
        public int CostA { get; }
        public int CostB { get; }
        public int ProfitPerBatch { get; }
    }

    private static readonly RefinementRecipe[] refinementRecipes =
    {
        new RefinementRecipe(ResourceType.Food, ResourceType.Salt, AdvancedResourcesTypes.Food, 5, 2, 12),
        new RefinementRecipe(ResourceType.Wood, ResourceType.Iron, AdvancedResourcesTypes.ReinforcedWood, 6, 2, 15),
        new RefinementRecipe(ResourceType.Stone, ResourceType.Clay, AdvancedResourcesTypes.StoneBriks, 5, 3, 14),
        new RefinementRecipe(ResourceType.Iron, ResourceType.Coal, AdvancedResourcesTypes.Steel, 4, 3, 20),
    };
    public static IReadOnlyList<RefinementRecipe> RefinementRecipes => refinementRecipes;
}
