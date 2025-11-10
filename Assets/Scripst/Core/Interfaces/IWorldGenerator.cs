using UnityEngine;

/// <summary>
/// Abstraction for any system that can generate terrain, factions, and settlement data.
/// Enables swapping implementations (procedural vs. handcrafted) and unit testing.
/// </summary>
public interface IWorldGenerator
{
    /// <summary>
    /// Latest generated terrain map. Implementations should keep this synchronized with generated data.
    /// </summary>
    TerrainType[,] TerrainMap { get; }

    /// <summary>
    /// Executes the full generation pipeline using the provided configuration.
    /// Partial overrides (null settings) should fall back to implementation defaults.
    /// </summary>
    /// <param name="settings">Optional overrides for world size, faction counts, etc.</param>
    void GenerateWorld(WorldGenerationSettings settings = null);
}

/// <summary>
/// Optional overrides for procedural generation without hard-coding MonoBehaviour fields.
/// </summary>
public class WorldGenerationSettings
{
    public int? WorldWidth { get; set; }
    public int? WorldHeight { get; set; }
    public int? NumberOfFactions { get; set; }
    public int? NumberOfCastles { get; set; }
    public int? NumberOfTowns { get; set; }
    public int? NumberOfVillages { get; set; }
}
