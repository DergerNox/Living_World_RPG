using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstraction for querying and mutating parties without referencing the concrete PartyManager singleton.
/// </summary>
public interface IPartyService
{
    Party PlayerParty { get; }
    IReadOnlyList<Party> AllParties { get; }

    void AddParty(Party party);
    void RemoveParty(Party party);
    List<Party> GetPartiesAtPosition(Vector3Int position);
    void UpdatePartyPositions();
}
