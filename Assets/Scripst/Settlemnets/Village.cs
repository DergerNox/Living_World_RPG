using UnityEngine;
using System.Collections.Generic;

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
    public int TownLevel { get; set; }
    public int MaxMilitiaSize { get; set; }
    public int CurrentMilitiaSize { get; set; }
    public List<Party> VisitingParties { get; set; }
    public List<Buildings> Buildings { get; set; }
    public List<Characters> VisitingCharacters { get; set; }
    public Characters Leader { get; set; }
    public List<Outpost> OutpostConnected { get; set; }

    // Recruitment pool for troops
    public List<Troop> RecruitmentPool { get; set; }

    // Constructor
    public Village(int x, int y)
    {
        Position = new Vector3Int(x, y, 0);
        Name = "Village";  // Default name, you can customize it later
        Population = 100;
        Resources = 100;
        TownLevel = 1;
        MaxMilitiaSize = 100;
        CurrentMilitiaSize = 0;
        VisitingParties = new List<Party>();
        Buildings = new List<Buildings>();
        VisitingCharacters = new List<Characters>();
        OutpostConnected = new List<Outpost>();
        FactionId = -1;  // Assuming -1 means no faction assigned initially

        RecruitmentPool = new List<Troop>();
        AddRank1And2Troops();
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
}
