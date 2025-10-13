using System.Collections.Generic;
using UnityEngine;

public enum DiplomacyStatus { Neutral, Allied, AtWar, Hostile }

public class FactionDiplomacy : MonoBehaviour
{
    public static FactionDiplomacy Instance;

    private Dictionary<(int, int), DiplomacyStatus> diplomacyMatrix = new Dictionary<(int, int), DiplomacyStatus>();

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

    private void InitializeDiplomacy()
    {
        // Get all factions from WorldHandler
        var factions = WorldHandler.Factions;
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
}
