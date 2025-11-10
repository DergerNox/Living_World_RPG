using System.Collections.Generic;
using UnityEngine;

public enum DiplomacyStatus { Neutral, Allied, AtWar, Hostile }

public class FactionDiplomacy : MonoBehaviour
{
    public static FactionDiplomacy Instance;

    private Dictionary<(int, int), DiplomacyStatus> diplomacyMatrix = new Dictionary<(int, int), DiplomacyStatus>();
    private IWorldStateRepository worldStateRepository;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeDiplomacy();
    }

    public void InitializeDiplomacy()
    {
        diplomacyMatrix.Clear();
        // Get all factions from WorldHandler
        var factions = GetFactions();
        if (factions == null) return;
        foreach (var factionA in factions)
        {
            foreach (var factionB in factions)
            {
                if (factionA.Id != factionB.Id)
                {
                    // Default to Neutral
                    diplomacyMatrix[(factionA.Id, factionB.Id)] = DiplomacyStatus.Neutral;
                    diplomacyMatrix[(factionB.Id, factionA.Id)] = DiplomacyStatus.Neutral;
                }
            }
        }

        // Randomly set some alliances and hostilities
        for (int i = 0; i < factions.Count; i++)
        {
            for (int j = i + 1; j < factions.Count; j++)
            {
                float rand = Random.value;
                if (rand < 0.2f) // 20% chance for alliance
                {
                    FormAlliance(factions[i].Id, factions[j].Id);
                }
                else if (rand < 0.3f) // 10% chance for hostility
                {
                    SetDiplomacyStatus(factions[i].Id, factions[j].Id, DiplomacyStatus.Hostile);
                }
            }
        }
    }

    public DiplomacyStatus GetDiplomacyStatus(int factionAId, int factionBId)
    {
        if (diplomacyMatrix.TryGetValue((factionAId, factionBId), out DiplomacyStatus status))
        {
            return status;
        }
        return DiplomacyStatus.Neutral;
    }

    public void SetDiplomacyStatus(int factionAId, int factionBId, DiplomacyStatus status)
    {
        diplomacyMatrix[(factionAId, factionBId)] = status;
        diplomacyMatrix[(factionBId, factionAId)] = status; // Symmetric

        Debug.Log($"Diplomacy between Faction {factionAId} and Faction {factionBId} set to {status}");
    }

    public void DeclareWar(int factionAId, int factionBId)
    {
        SetDiplomacyStatus(factionAId, factionBId, DiplomacyStatus.AtWar);
    }

    public void FormAlliance(int factionAId, int factionBId)
    {
        SetDiplomacyStatus(factionAId, factionBId, DiplomacyStatus.Allied);
    }

    public bool AreAllied(int factionAId, int factionBId)
    {
        return GetDiplomacyStatus(factionAId, factionBId) == DiplomacyStatus.Allied;
    }

    public bool AreAtWar(int factionAId, int factionBId)
    {
        return GetDiplomacyStatus(factionAId, factionBId) == DiplomacyStatus.AtWar;
    }

    private IReadOnlyList<Faction> GetFactions()
    {
        if (worldStateRepository == null)
        {
            if (!ServiceLocator.TryResolve(out worldStateRepository))
            {
                worldStateRepository = WorldHandler.Instance;
            }
        }
        return worldStateRepository?.Factions ?? WorldHandler.Factions;
    }
}
