using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour, IWorldGenerator
{
    public static WorldGenerator Instance;

    public int worldWidth = 100;
    public int worldHeight = 100;
    public int numberOfFactions = 5;
    public int numberOfCastles = 5;
    public int numberOfTowns = 10;
    public int numberOfVillages = 20;

    public TerrainType[,] TerrainMap { get; private set; }
    public int WorldWidth => worldWidth;
    public int WorldHeight => worldHeight;

    private HashSet<Vector2Int> settlementPositions = new HashSet<Vector2Int>();
    private IWorldStateRepository _worldRepo;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (!ServiceLocator.TryResolve(out _worldRepo) || _worldRepo == null)
        {
            Debug.LogError("WorldGenerator requires an IWorldStateRepository registered in the ServiceLocator.");
            enabled = false;
        }
    }

    public void GenerateWorld(WorldGenerationSettings settings = null)
    {
        if (_worldRepo == null)
        {
            Debug.LogError("WorldGenerator cannot generate without a world repository instance.");
            return;
        }
        ApplyOverrides(settings);
        TerrainMap = new TerrainType[worldWidth, worldHeight];
        settlementPositions.Clear();

        GenerateTerrain();
        GenerateFactions();
        GenerateSettlements();
        AssignSettlementsToFactions();
        InitializeDiplomacy();
    }

    private void ApplyOverrides(WorldGenerationSettings settings)
    {
        if (settings == null) return;

        if (settings.WorldWidth.HasValue) worldWidth = Mathf.Max(10, settings.WorldWidth.Value);
        if (settings.WorldHeight.HasValue) worldHeight = Mathf.Max(10, settings.WorldHeight.Value);
        if (settings.NumberOfFactions.HasValue) numberOfFactions = Mathf.Max(1, settings.NumberOfFactions.Value);
        if (settings.NumberOfCastles.HasValue) numberOfCastles = Mathf.Max(0, settings.NumberOfCastles.Value);
        if (settings.NumberOfTowns.HasValue) numberOfTowns = Mathf.Max(0, settings.NumberOfTowns.Value);
        if (settings.NumberOfVillages.HasValue) numberOfVillages = Mathf.Max(0, settings.NumberOfVillages.Value);
    }

    private void GenerateTerrain()
    {
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                // Simple noise-based terrain generation
                float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                if (noise < 0.3f)
                {
                    TerrainMap[x, y] = TerrainType.Water;
                }
                else if (noise < 0.5f)
                {
                    TerrainMap[x, y] = TerrainType.Forest;
                }
                else if (noise < 0.7f)
                {
                    TerrainMap[x, y] = TerrainType.Grass;
                }
                else if (noise < 0.9f)
                {
                    TerrainMap[x, y] = TerrainType.Hills;
                }
                else
                {
                    TerrainMap[x, y] = TerrainType.Mountains;
                }
            }
        }
    }

    private void GenerateFactions()
    {
        if (_worldRepo == null) return;

        _worldRepo.Factions.Clear();
        for (int i = 0; i < numberOfFactions; i++)
        {
            Vector2 center = new Vector2(Random.Range(0, worldWidth), Random.Range(0, worldHeight));
            Faction faction = new Faction($"Faction {i + 1}", center, i);
            _worldRepo.Factions.Add(faction);
        }

        // Ensure factions have lists initialized
        foreach (var faction in _worldRepo.Factions)
        {
            if (faction.Castles == null) faction.Castles = new List<Castle>();
            if (faction.Towns == null) faction.Towns = new List<Town>();
            if (faction.Villages == null) faction.Villages = new List<Village>();
        }
    }

    private void GenerateSettlements()
    {
        if (_worldRepo == null) return;

        _worldRepo.Castles.Clear();
        _worldRepo.Towns.Clear();
        _worldRepo.Villages.Clear();

        // Generate Castles
        for (int i = 0; i < numberOfCastles; i++)
        {
            Vector3Int position = new Vector3Int(Random.Range(0, worldWidth), Random.Range(0, worldHeight), 0);
            Characters lord = new NPC($"Lord {i + 1}", 100, 50, 5); // Assuming NPC constructor
            Castle castle = new Castle(position.x, position.y, lord);
            _worldRepo.Castles.Add(castle);
        }

        // Generate Towns
        for (int i = 0; i < numberOfTowns; i++)
        {
            Vector3Int position = new Vector3Int(Random.Range(0, worldWidth), Random.Range(0, worldHeight), 0);
            Town town = new Town(position.x, position.y);
            _worldRepo.Towns.Add(town);
        }

        // Generate Villages
        for (int i = 0; i < numberOfVillages; i++)
        {
            Vector3Int position = new Vector3Int(Random.Range(0, worldWidth), Random.Range(0, worldHeight), 0);
            Village village = new Village(position.x, position.y);
            _worldRepo.Villages.Add(village);
        }
    }

    private void AssignSettlementsToFactions()
    {
        // Assign castles to factions
        for (int i = 0; i < _worldRepo.Castles.Count; i++)
        {
            int factionId = i % _worldRepo.Factions.Count;
            _worldRepo.Castles[i].FactionId = factionId;
            _worldRepo.Factions[factionId].Castles.Add(_worldRepo.Castles[i]);
        }

        // Assign towns and villages randomly to factions
        foreach (var town in _worldRepo.Towns)
        {
            int factionId = Random.Range(0, _worldRepo.Factions.Count);
            town.FactionId = factionId;
            _worldRepo.Factions[factionId].Towns.Add(town);
        }

        foreach (var village in _worldRepo.Villages)
        {
            int factionId = Random.Range(0, _worldRepo.Factions.Count);
            village.FactionId = factionId;
            _worldRepo.Factions[factionId].Villages.Add(village);
        }
    }

    private void InitializeDiplomacy()
    {
        if (FactionDiplomacy.Instance != null)
        {
            FactionDiplomacy.Instance.InitializeDiplomacy();
        }
    }
}
