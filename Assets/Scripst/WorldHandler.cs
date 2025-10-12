using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldHandler : MonoBehaviour
{
    // Public Variables
    public static Player Player { get { return player; } set { player = value; } }
    public static List<Castle> Castles { get { return castles; } set { castles = value; } }
    public static List<Town> Towns { get { return towns; } set { towns = value; } }
    public static List<Village> Villages { get { return villages; } set { villages = value; } }
    public static List<Faction> Factions { get { return factions; } set { factions = value; } }

    // Constructor
    public WorldHandler()
    {
    }

    // Start Method
    void Start()
    {
        player = new Player("Player");
        
        Debug.Log(Factions.Count);
    }

    // Update Method
    void Update()
    {
    }

    // Get the Town at a specific position
    public static Town GetTownAtPosition(Vector3Int position)
    {
        foreach (var town in towns)
        {
            if (town == null)
            {
                Debug.LogWarning("Null town found in towns list.");
                continue;
            }
            Vector3Int townPos;
            try
            {
                townPos = (Vector3Int)town.Position;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error accessing Town.Position: {e.Message}");
                continue;
            }
            if (townPos == position)
            {
                return town;
            }
        }
        return null;
    }

    // Get the Castle at a specific position
    public static Castle GetCastleAtPosition(Vector3Int position)
    {
        foreach (var castle in castles)
        {
            if (castle == null)
            {
                Debug.LogWarning("Null castle found in castles list.");
                continue;
            }
            Vector3Int castlePos;
            try
            {
                castlePos = (Vector3Int)castle.Position;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error accessing Castle.Position: {e.Message}");
                continue;
            }
            if (castlePos == position)
            {
                return castle;
            }
        }
        return null;
    }

    // Get the Village at a specific position
    public static Village GetVillageAtPosition(Vector3Int position)
    {
        foreach (var village in villages)
        {
            if (village == null)
            {
                Debug.LogWarning("Null village found in villages list.");
                continue;
            }
            Vector3Int villagePos;
            try
            {
                villagePos = (Vector3Int)village.Position;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error accessing Village.Position: {e.Message}");
                continue;
            }
            if (villagePos == position)
            {
                return village;
            }
        }
        return null;
    }

    // Private Variables
    private static Player player;
    private static List<Castle> castles = new List<Castle>();
    private static List<Town> towns = new List<Town>();
    private static List<Village> villages = new List<Village>();
    private static List<Faction> factions = new List<Faction>();
}
