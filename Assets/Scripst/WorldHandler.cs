using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldHandler : MonoBehaviour, IWorldStateRepository
{
    public static WorldHandler Instance { get; private set; }

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    // Start Method
    void Start()
    {
        player = new Player("Player");

        // Generate world if not already done
        if (WorldGenerator.Instance != null)
        {
            WorldGenerator.Instance.GenerateWorld();
        }

        BuildSpatialCaches();

        // Assign player to a faction or make neutral
        if (Factions.Count > 0)
        {
            player.Party.FactionId = 0; // Assign to first faction or handle appropriately
        }

        Debug.Log($"World generated with {Factions.Count} factions, {Castles.Count} castles, {Towns.Count} towns, {Villages.Count} villages.");
    }

    // Update Method
    void Update() { }

    // Get the Town at a specific position
    public static Town GetTownAtPosition(Vector3Int position)
    {
        Town t;
        if (_townByPos.TryGetValue(position, out t)) return t;
        return null;
    }

    // Get the Castle at a specific position
    public static Castle GetCastleAtPosition(Vector3Int position)
    {
        Castle c;
        if (_castleByPos.TryGetValue(position, out c)) return c;
        return null;
    }

    // Get the Village at a specific position
    public static Village GetVillageAtPosition(Vector3Int position)
    {
        Village v;
        if (_villageByPos.TryGetValue(position, out v)) return v;
        return null;
    }

    Player IWorldStateRepository.Player => player;
    IReadOnlyList<Castle> IWorldStateRepository.Castles => castles;
    IReadOnlyList<Town> IWorldStateRepository.Towns => towns;
    IReadOnlyList<Village> IWorldStateRepository.Villages => villages;
    IReadOnlyList<Faction> IWorldStateRepository.Factions => factions;

    public void RebuildSpatialCaches()
    {
        BuildSpatialCaches();
    }

    // Private Variables
    private static Player player;
    private static List<Castle> castles = new List<Castle>();
    private static List<Town> towns = new List<Town>();
    private static List<Village> villages = new List<Village>();
    private static List<Faction> factions = new List<Faction>();

	private static readonly Dictionary<Vector3Int, Town> _townByPos = new Dictionary<Vector3Int, Town>();
	private static readonly Dictionary<Vector3Int, Castle> _castleByPos = new Dictionary<Vector3Int, Castle>();
	private static readonly Dictionary<Vector3Int, Village> _villageByPos = new Dictionary<Vector3Int, Village>();

	private static void BuildSpatialCaches()
	{
		_townByPos.Clear();
		_castleByPos.Clear();
		_villageByPos.Clear();

		foreach (var town in towns)
		{
			if (town == null) continue;
			try { _townByPos[(Vector3Int)town.Position] = town; } catch (Exception) { }
		}
		foreach (var castle in castles)
		{
			if (castle == null) continue;
			try { _castleByPos[(Vector3Int)castle.Position] = castle; } catch (Exception) { }
		}
		foreach (var village in villages)
		{
			if (village == null) continue;
			try { _villageByPos[(Vector3Int)village.Position] = village; } catch (Exception) { }
		}
	}
}
