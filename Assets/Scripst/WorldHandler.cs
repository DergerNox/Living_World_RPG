using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldHandler : MonoBehaviour, IWorldStateRepository
{
    public static WorldHandler Instance { get; private set; }

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

    public Player Player => player;
    public IList<Castle> Castles => castles;
    public IList<Town> Towns => towns;
    public IList<Village> Villages => villages;
    public IList<Faction> Factions => factions;

    public Town GetTownAtPosition(Vector3Int position)
    {
        _townByPos.TryGetValue(position, out var town);
        return town;
    }

    public Castle GetCastleAtPosition(Vector3Int position)
    {
        _castleByPos.TryGetValue(position, out var castle);
        return castle;
    }

    public Village GetVillageAtPosition(Vector3Int position)
    {
        _villageByPos.TryGetValue(position, out var village);
        return village;
    }

    public void RebuildSpatialCaches()
    {
        BuildSpatialCaches();
    }

    // Private Variables
    [SerializeField] private Player player;
    private readonly List<Castle> castles = new List<Castle>();
    private readonly List<Town> towns = new List<Town>();
    private readonly List<Village> villages = new List<Village>();
    private readonly List<Faction> factions = new List<Faction>();

	private readonly Dictionary<Vector3Int, Town> _townByPos = new Dictionary<Vector3Int, Town>();
	private readonly Dictionary<Vector3Int, Castle> _castleByPos = new Dictionary<Vector3Int, Castle>();
	private readonly Dictionary<Vector3Int, Village> _villageByPos = new Dictionary<Vector3Int, Village>();

	private void BuildSpatialCaches()
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
