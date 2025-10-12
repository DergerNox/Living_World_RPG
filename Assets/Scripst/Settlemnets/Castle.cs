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

        AvailableUnits = new List<Troop>();
        AddRank4And5Troops();
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
}
