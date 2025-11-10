using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Exposes read/write access to the simulated world (player, settlements, factions) without direct static coupling.
/// </summary>
public interface IWorldStateRepository
{
    Player Player { get; }
    IReadOnlyList<Castle> Castles { get; }
    IReadOnlyList<Town> Towns { get; }
    IReadOnlyList<Village> Villages { get; }
    IReadOnlyList<Faction> Factions { get; }

    Town GetTownAtPosition(Vector3Int position);
    Castle GetCastleAtPosition(Vector3Int position);
    Village GetVillageAtPosition(Vector3Int position);

    /// <summary>
    /// Rebuilds spatial caches after bulk world updates (e.g., regeneration, loading a save).
    /// </summary>
    void RebuildSpatialCaches();
}
