using System.Collections.Generic;
using UnityEngine;
using LivingWorldRPG.Characters;

namespace LivingWorldRPG.World
{
    public class WorldGenerator : MonoBehaviour, IWorldGenerator
    {
        public static WorldGenerator Instance { get; private set; }

        [SerializeField, Min(10)] private int _worldWidth = 100;
        [SerializeField, Min(10)] private int _worldHeight = 100;
        [SerializeField, Min(1)] private int _numberOfFactions = 5;
        [SerializeField, Min(0)] private int _numberOfCastles = 5;
        [SerializeField, Min(0)] private int _numberOfTowns = 10;
        [SerializeField, Min(0)] private int _numberOfVillages = 20;

        public TerrainType[,] TerrainMap { get; private set; }
        public int WorldWidth => _worldWidth;
        public int WorldHeight => _worldHeight;

        private IWorldStateRepository worldRepository;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ServiceLocator.Register<WorldGenerator>(this);
            ServiceLocator.Register<IWorldGenerator>(this);

            TryResolveWorldRepository();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void GenerateWorld(WorldGenerationSettings settings = null)
        {
            if (!TryResolveWorldRepository())
            {
                Debug.LogError("WorldGenerator cannot generate without a world repository instance.");
                return;
            }

            ApplyOverrides(settings);

            if (!ValidateRepositoryCollections())
            {
                return;
            }

            ResetWorldCollections();
            TerrainMap = new TerrainType[_worldWidth, _worldHeight];

            GenerateTerrain();
            GenerateFactions();
            GenerateSettlements();
            AssignSettlementsToFactions();
            InitializeDiplomacy();
            worldRepository?.RebuildSpatialCaches();
        }

        private bool TryResolveWorldRepository()
        {
            return worldRepository != null ||
                   (ServiceLocator.TryResolve(out worldRepository) && worldRepository != null);
        }

        private void ApplyOverrides(WorldGenerationSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            if (settings.WorldWidth.HasValue) _worldWidth = Mathf.Max(10, settings.WorldWidth.Value);
            if (settings.WorldHeight.HasValue) _worldHeight = Mathf.Max(10, settings.WorldHeight.Value);
            if (settings.NumberOfFactions.HasValue) _numberOfFactions = Mathf.Max(1, settings.NumberOfFactions.Value);
            if (settings.NumberOfCastles.HasValue) _numberOfCastles = Mathf.Max(0, settings.NumberOfCastles.Value);
            if (settings.NumberOfTowns.HasValue) _numberOfTowns = Mathf.Max(0, settings.NumberOfTowns.Value);
            if (settings.NumberOfVillages.HasValue) _numberOfVillages = Mathf.Max(0, settings.NumberOfVillages.Value);
        }

        private void GenerateTerrain()
        {
            for (int x = 0; x < _worldWidth; x++)
            {
                for (int y = 0; y < _worldHeight; y++)
                {
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
            if (worldRepository == null)
            {
                return;
            }

            for (int i = 0; i < _numberOfFactions; i++)
            {
                Vector2 center = GetRandomMapPosition2D();
                Faction faction = new Faction($"Faction {i + 1}", center, i)
                {
                    Color = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.7f, 1f)
                };
                worldRepository.Factions.Add(faction);
            }

            foreach (var faction in worldRepository.Factions)
            {
                faction.Castles ??= new List<Castle>();
                faction.Towns ??= new List<Town>();
                faction.Villages ??= new List<Village>();
            }
        }

        private void GenerateSettlements()
        {
            if (worldRepository == null)
            {
                return;
            }

            for (int i = 0; i < _numberOfCastles; i++)
            {
                Vector3Int position = GetRandomMapPosition3D();
                NPC lord = new NPC($"Lord {i + 1}", 100, 50, 5);
                Castle castle = new Castle(position.x, position.y, lord);
                worldRepository.Castles.Add(castle);
            }

            for (int i = 0; i < _numberOfTowns; i++)
            {
                Vector3Int position = GetRandomMapPosition3D();
                Town town = new Town(position.x, position.y);
                worldRepository.Towns.Add(town);
            }

            for (int i = 0; i < _numberOfVillages; i++)
            {
                Vector3Int position = GetRandomMapPosition3D();
                Village village = new Village(position.x, position.y);
                worldRepository.Villages.Add(village);
            }

            SettlementEconomySystem.InitializeEconomy(worldRepository.Villages, worldRepository.Towns, worldRepository.Castles);
        }

        private void AssignSettlementsToFactions()
        {
            if (worldRepository?.Factions == null || worldRepository.Factions.Count == 0)
            {
                return;
            }

            for (int i = 0; i < worldRepository.Castles.Count; i++)
            {
                int factionId = i % worldRepository.Factions.Count;
                worldRepository.Castles[i].FactionId = factionId;
                worldRepository.Factions[factionId].Castles.Add(worldRepository.Castles[i]);
            }

            foreach (var town in worldRepository.Towns)
            {
                int factionId = Random.Range(0, worldRepository.Factions.Count);
                town.FactionId = factionId;
                worldRepository.Factions[factionId].Towns.Add(town);
            }

            foreach (var village in worldRepository.Villages)
            {
                int factionId = Random.Range(0, worldRepository.Factions.Count);
                village.FactionId = factionId;
                worldRepository.Factions[factionId].Villages.Add(village);
            }
        }

        private void ResetWorldCollections()
        {
            worldRepository?.Factions?.Clear();
            worldRepository?.Castles?.Clear();
            worldRepository?.Towns?.Clear();
            worldRepository?.Villages?.Clear();
        }

        private bool ValidateRepositoryCollections()
        {
            if (worldRepository?.Factions == null ||
                worldRepository?.Castles == null ||
                worldRepository?.Towns == null ||
                worldRepository?.Villages == null)
            {
                Debug.LogError("WorldGenerator requires world repository collections to be initialized before generation.");
                return false;
            }

            return true;
        }

        private static void InitializeDiplomacy()
        {
            if (FactionDiplomacy.Instance != null)
            {
                FactionDiplomacy.Instance.InitializeDiplomacy();
            }
        }

        private Vector2 GetRandomMapPosition2D()
        {
            return new Vector2(Random.Range(0, _worldWidth), Random.Range(0, _worldHeight));
        }

        private Vector3Int GetRandomMapPosition3D()
        {
            return new Vector3Int(Random.Range(0, _worldWidth), Random.Range(0, _worldHeight), 0);
        }
    }
}
