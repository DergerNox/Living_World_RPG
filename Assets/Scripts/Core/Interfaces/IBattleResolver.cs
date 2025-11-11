using System.Collections.Generic;

/// <summary>
/// Resolves combat between parties or individual troops, enabling alternative combat models.
/// </summary>
public interface IBattleResolver
{
    /// <summary>
    /// Resolves a duel between two troops, returning true if the attacker survives.
    /// </summary>
    bool ResolveDuel(Troop attacker, Troop defender, TerrainType terrain);

    /// <summary>
    /// Resolves combat between attacker and defender parties, mutating their troop collections.
    /// </summary>
    void ResolveParties(IList<Party> attackers, IList<Party> defenders, TerrainType terrain);
}
