using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LivingWorldRPG.World
{
    /// <summary>
    /// Default implementation for <see cref="IWorldSimulationService"/>.
    /// Provides a boundary for world generation and high-level queries.
    /// </summary>
    public sealed class WorldSimulationService : IWorldSimulationService
    {
        private readonly IWorldStateRepository worldState;
        private readonly IWorldGenerator worldGenerator;
        private bool worldReady;

        public WorldSimulationService(IWorldStateRepository worldState, IWorldGenerator worldGenerator)
        {
            this.worldState = worldState;
            this.worldGenerator = worldGenerator;
        }

        public event Action WorldGenerated;

        public bool IsWorldReady => worldReady;

        public IWorldStateRepository State => worldState;

        public void InitializeWorld(WorldGenerationSettings settings = null, bool force = false)
        {
            if (worldReady && !force)
            {
                return;
            }

            if (worldGenerator == null)
            {
                Debug.LogError("WorldSimulationService: No IWorldGenerator registered; cannot initialize world.");
                return;
            }

            worldGenerator.GenerateWorld(settings);
            worldState.RebuildSpatialCaches();

            if (worldState.Player != null && worldState.Player.Party != null && worldState.Factions.Count > 0)
            {
                // Default player faction assignment if one has not been set yet.
                if (worldState.Player.Party.FactionId < 0 || worldState.Player.Party.FactionId >= worldState.Factions.Count)
                {
                    worldState.Player.Party.FactionId = 0;
                }
            }

            worldReady = true;

            Debug.Log($"WorldSimulationService: world ready with {worldState.Factions.Count} factions, {worldState.Castles.Count} castles, {worldState.Towns.Count} towns, {worldState.Villages.Count} villages.");

            WorldGenerated?.Invoke();
        }

        public IEnumerable<ISettlement> GetSettlements(Func<ISettlement, bool> predicate = null)
        {
            IEnumerable<ISettlement> all = worldState.Castles
                .Cast<ISettlement>()
                .Concat(worldState.Towns.Cast<ISettlement>())
                .Concat(worldState.Villages.Cast<ISettlement>());

            return predicate == null ? all : all.Where(predicate);
        }

        public IEnumerable<Castle> GetCastlesByFaction(int factionId)
        {
            return worldState.Castles.Where(c => c != null && c.FactionId == factionId);
        }

        public IEnumerable<Town> GetTownsByFaction(int factionId)
        {
            return worldState.Towns.Where(t => t != null && t.FactionId == factionId);
        }

        public IEnumerable<Village> GetVillagesByFaction(int factionId)
        {
            return worldState.Villages.Where(v => v != null && v.FactionId == factionId);
        }

        public Faction GetFaction(int factionId)
        {
            if (factionId < 0 || factionId >= worldState.Factions.Count)
            {
                return null;
            }
            return worldState.Factions[factionId];
        }

        public IEnumerable<Faction> GetFactions(Func<Faction, bool> predicate = null)
        {
            return predicate == null ? worldState.Factions : worldState.Factions.Where(predicate);
        }
    }
}
