using System;
using System.Collections.Generic;

namespace LivingWorldRPG.World
{
    /// <summary>
    /// Facade around world generation/state repositories so callers interact with a consistent module boundary.
    /// </summary>
    public interface IWorldSimulationService
    {
        /// <summary>
        /// Raised after the world has been generated and spatial caches rebuilt.
        /// </summary>
        event Action WorldGenerated;

        /// <summary>
        /// True when the world has been generated during this session.
        /// </summary>
        bool IsWorldReady { get; }

        /// <summary>
        /// Underlying world-state repository exposed for legacy consumers (read-only preferred).
        /// </summary>
        IWorldStateRepository State { get; }

        /// <summary>
        /// Generates (or regenerates) the world using the configured generator and repositories.
        /// Safe to call multiple times; generation only runs once unless <paramref name="force"/> is true.
        /// </summary>
        void InitializeWorld(WorldGenerationSettings settings = null, bool force = false);

        /// <summary>
        /// Enumerates settlements matching an optional predicate for filtering.
        /// </summary>
        IEnumerable<ISettlement> GetSettlements(Func<ISettlement, bool> predicate = null);

        IEnumerable<Castle> GetCastlesByFaction(int factionId);
        IEnumerable<Town> GetTownsByFaction(int factionId);
        IEnumerable<Village> GetVillagesByFaction(int factionId);

        Faction GetFaction(int factionId);
        IEnumerable<Faction> GetFactions(Func<Faction, bool> predicate = null);
    }
}
