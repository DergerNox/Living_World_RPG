using System;
using System.Collections.Generic;
using UnityEngine;
using LivingWorldRPG.World;

namespace LivingWorldRPG.World
{
    public class WorldHandler : MonoBehaviour, IWorldStateRepository
    {
        public static WorldHandler Instance { get; private set; }

        [SerializeField] private Player player;
        private readonly List<Castle> castles = new List<Castle>();
        private readonly List<Town> towns = new List<Town>();
        private readonly List<Village> villages = new List<Village>();
        private readonly List<Faction> factions = new List<Faction>();

        private readonly Dictionary<Vector3Int, Town> townByPosition = new Dictionary<Vector3Int, Town>();
        private readonly Dictionary<Vector3Int, Castle> castleByPosition = new Dictionary<Vector3Int, Castle>();
        private readonly Dictionary<Vector3Int, Village> villageByPosition = new Dictionary<Vector3Int, Village>();

        private IWorldGenerator worldGenerator;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (!ServiceLocator.TryResolve(out worldGenerator))
            {
                Debug.LogError("WorldHandler requires an IWorldGenerator registered in the ServiceLocator.");
            }
        }

        private void Start()
        {
            player ??= new Player("Player");
            worldGenerator?.GenerateWorld();
            BuildSpatialCaches();

            if (Factions.Count > 0)
            {
                player.Party.FactionId = 0;
            }

            Debug.Log($"World generated with {Factions.Count} factions, {Castles.Count} castles, {Towns.Count} towns, {Villages.Count} villages.");
        }

        public Player Player => player;
        public IList<Castle> Castles => castles;
        public IList<Town> Towns => towns;
        public IList<Village> Villages => villages;
        public IList<Faction> Factions => factions;

        public Town GetTownAtPosition(Vector3Int position)
        {
            townByPosition.TryGetValue(position, out var town);
            return town;
        }

        public Castle GetCastleAtPosition(Vector3Int position)
        {
            castleByPosition.TryGetValue(position, out var castle);
            return castle;
        }

        public Village GetVillageAtPosition(Vector3Int position)
        {
            villageByPosition.TryGetValue(position, out var village);
            return village;
        }

        public void RebuildSpatialCaches()
        {
            BuildSpatialCaches();
        }

        private void BuildSpatialCaches()
        {
            townByPosition.Clear();
            castleByPosition.Clear();
            villageByPosition.Clear();

            foreach (var town in towns)
            {
                if (town == null)
                {
                    continue;
                }

                var key = (Vector3Int)town.Position;
                townByPosition[key] = town;
            }

            foreach (var castle in castles)
            {
                if (castle == null)
                {
                    continue;
                }

                var key = (Vector3Int)castle.Position;
                castleByPosition[key] = castle;
            }

            foreach (var village in villages)
            {
                if (village == null)
                {
                    continue;
                }

                var key = (Vector3Int)village.Position;
                villageByPosition[key] = village;
            }
        }
    }
}
