using System.Collections.Generic;
using UnityEngine;

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

        // Add rank 2 and 3 troops to AvailableUnits
        AddRank2And3Troops();

        // Initialize market
        Market.AddItem(new Item { name = "Sword", type = ItemType.Weapon, value = 100 });
        Market.AddItem(new Item { name = "Armor", type = ItemType.Armor, value = 150 });
        Market.AddItem(new Item { name = "Health Potion", type = ItemType.Consumable, value = 50 });

        Income = 1000;
        Expenses = 500;
        Taxes = 200;
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
}
