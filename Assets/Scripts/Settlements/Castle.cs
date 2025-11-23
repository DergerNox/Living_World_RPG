using UnityEngine;
using System.Collections.Generic;

public class Castle : ISettlement
{
    // ISettlement Interface Implementation
    public int PosX => Position.x;  // PosX is the x coordinate of the Position
    public int PosY => Position.y;  // PosY is the y coordinate of the Position
    public int FactionId { get; set; }

    // Private field for position
    public Vector3Int Position { get; private set; }
    public string Name { get; private set; }

    // Additional properties specific to Castle
    public int CastleLevel { get; set; }
    public int MaxGarrisonSize { get; set; }
    public int CurrentGarrisonSize { get; set; }
    public List<Characters> Visitors { get; set; }
    public Characters Lord { get; set; }
    public List<Party> PartiesInCastle { get; set; }
    public List<Buildings> BuildingsInCastle { get; set; }
    public Queue<Buildings> BuildingsQueue { get; set; }
    public bool IsCastleUnderSiege { get; set; }
    public SettlementResourceLedger ResourceLedger { get; }
    public IReadOnlyList<Town> SupplyTowns => supplyTowns;
    public Dictionary<WeaponTypes, int> Armory { get; } = new Dictionary<WeaponTypes, int>();
    public int Supplies { get; private set; }
    public int Treasury { get; private set; }
    // List of available troops for recruitment
    public List<Troop> AvailableUnits { get; set; }

    // Constructor
    public Castle(int x, int y, Characters lord)
    {
        Position = new Vector3Int(x, y, 0);
        Name = "Castle";  // Default name, you can customize it later
        CastleLevel = 1;
        MaxGarrisonSize = 500;  // Adjust max size based on your game design
        CurrentGarrisonSize = 0;
        Visitors = new List<Characters>();
        PartiesInCastle = new List<Party>();
        BuildingsInCastle = new List<Buildings>();
        BuildingsQueue = new Queue<Buildings>();
        IsCastleUnderSiege = false;
        Lord = lord;  // Lord assigned through constructor
        FactionId = -1;  // Assuming -1 means no faction assigned initially
        ResourceLedger = new SettlementResourceLedger();
        Supplies = 0;
        Treasury = 0;

        AvailableUnits = new List<Troop>();
        AddRank4And5Troops();

        // Initialize with basic buildings
        InitializeBuildings();
    }

    // Method to add rank 4 and 5 troops to the castle's available units
    private void AddRank4And5Troops()
    {
        foreach (TroopType troopType in System.Enum.GetValues(typeof(TroopType)))
        {
            // Add Rank 4 troop
            Troop rank4Troop = new Troop(troopType);
            rank4Troop.Level = 4;
            rank4Troop.UpdateRank();
            AvailableUnits.Add(rank4Troop);

            // Add Rank 5 troop
            Troop rank5Troop = new Troop(troopType);
            rank5Troop.Level = 5;
            rank5Troop.UpdateRank();
            AvailableUnits.Add(rank5Troop);
        }
    }

    private void InitializeBuildings()
    {
        // Add basic buildings for castle
        BuildingsInCastle.Add(new Buildings(BuildingType.Wall) { IsBuilt = true });
        BuildingsInCastle.Add(new Buildings(BuildingType.Tower) { IsBuilt = true });
        BuildingsInCastle.Add(new Buildings(BuildingType.Barracks) { IsBuilt = true });
    }

    public void BuildBuilding(BuildingType type)
    {
        Buildings newBuilding = new Buildings(type);
        if (BuildingsQueue.Count < 5) // Castles can have more in queue
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
                BuildingsInCastle.Add(building);
                ApplyBuildingEffects(building);
                Debug.Log($"{building.Name} completed in {Name}!");
            }
        }
    }

    private void ApplyBuildingEffects(Buildings building)
    {
        // Apply effects specific to castle
        MaxGarrisonSize += building.DefenseBonus; // Example: walls increase garrison capacity
    }

    public int GetTotalDefenseBonus()
    {
        int totalBonus = 0;
        foreach (var building in BuildingsInCastle)
        {
            if (building.IsBuilt)
            {
                totalBonus += building.DefenseBonus;
            }
        }
        return totalBonus;
    }

    public void ProcessSiegeEvent()
    {
        if (IsCastleUnderSiege)
        {
            // Handle siege mechanics for castle
            Debug.Log($"{Name} is under siege! Defense bonus: {GetTotalDefenseBonus()}");
        }
    }

    public int ForgeEquipment(int days = 1)
    {
        days = Mathf.Max(1, days);
        int produced = 0;
        int forgingCapacity = Mathf.Max(1, CastleLevel) * days * 2;

        foreach (var recipe in equipmentRecipes)
        {
            if (forgingCapacity <= 0)
            {
                break;
            }

            int availableA = ResourceLedger.GetAdvanced(recipe.RequiredAdvanced);
            int availableB = ResourceLedger.GetBasic(recipe.RequiredBasic);
            int craftable = Mathf.Min(forgingCapacity, availableA / recipe.CostAdvanced, availableB / recipe.CostBasic);
            if (craftable <= 0)
            {
                continue;
            }

            ResourceLedger.ConsumeAdvanced(recipe.RequiredAdvanced, craftable * recipe.CostAdvanced);
            ResourceLedger.ConsumeBasic(recipe.RequiredBasic, craftable * recipe.CostBasic);
            int output = craftable * recipe.OutputCount;

            if (Armory.TryGetValue(recipe.WeaponType, out int existing))
            {
                Armory[recipe.WeaponType] = existing + output;
            }
            else
            {
                Armory[recipe.WeaponType] = output;
            }

            produced += output;
            forgingCapacity -= craftable;
        }

        ProcessSupplies();
        return produced;
    }

    internal void RegisterSupplyingTown(Town town)
    {
        if (town == null || supplyTowns.Contains(town))
        {
            return;
        }

        supplyTowns.Add(town);
    }

    internal void ResetSupplyNetwork()
    {
        supplyTowns.Clear();
    }

    private void ProcessSupplies()
    {
        int availableFood = ResourceLedger.GetAdvanced(AdvancedResourcesTypes.Food);
        if (availableFood < FoodPerSupplyBatch)
        {
            return;
        }

        int batches = availableFood / FoodPerSupplyBatch;
        int consumed = batches * FoodPerSupplyBatch;
        ResourceLedger.ConsumeAdvanced(AdvancedResourcesTypes.Food, consumed);
        Supplies += batches * SupplyValuePerBatch;
    }

    public void AddTreasury(int amount)
    {
        Treasury = Mathf.Max(0, Treasury + amount);
    }

    public int TakeTreasury(int amount)
    {
        int toTake = Mathf.Max(0, amount);
        toTake = Mathf.Min(toTake, Treasury);
        Treasury -= toTake;
        return toTake;
    }

    private readonly List<Town> supplyTowns = new List<Town>();

    public readonly struct EquipmentRecipe
    {
        public EquipmentRecipe(AdvancedResourcesTypes advanced, ResourceType basic, WeaponTypes weaponType, int costAdvanced, int costBasic, int outputCount)
        {
            RequiredAdvanced = advanced;
            RequiredBasic = basic;
            WeaponType = weaponType;
            CostAdvanced = Mathf.Max(1, costAdvanced);
            CostBasic = Mathf.Max(1, costBasic);
            OutputCount = Mathf.Max(1, outputCount);
        }

        public AdvancedResourcesTypes RequiredAdvanced { get; }
        public ResourceType RequiredBasic { get; }
        public WeaponTypes WeaponType { get; }
        public int CostAdvanced { get; }
        public int CostBasic { get; }
        public int OutputCount { get; }
    }

    private static readonly EquipmentRecipe[] equipmentRecipes =
    {
        new EquipmentRecipe(AdvancedResourcesTypes.Steel, ResourceType.Food, WeaponTypes.Sword, 3, 2, 1),
        new EquipmentRecipe(AdvancedResourcesTypes.ReinforcedWood, ResourceType.Textile, WeaponTypes.Bow, 2, 1, 1),
        new EquipmentRecipe(AdvancedResourcesTypes.StoneBriks, ResourceType.Salt, WeaponTypes.Shield, 2, 1, 1)
    };
    public static IReadOnlyList<EquipmentRecipe> EquipmentRecipes => equipmentRecipes;

    private const int FoodPerSupplyBatch = 3;
    private const int SupplyValuePerBatch = 15;
}
