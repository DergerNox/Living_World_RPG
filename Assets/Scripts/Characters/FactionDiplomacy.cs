using System.Collections.Generic;
using UnityEngine;
using LivingWorldRPG.World;

namespace LivingWorldRPG.Characters
{
    public enum DiplomacyStatus { Neutral, Allied, AtWar, Hostile }

    public class FactionDiplomacy : MonoBehaviour
    {
        public static FactionDiplomacy Instance { get; private set; }

        private readonly Dictionary<(int, int), DiplomacyStatus> diplomacyMatrix = new Dictionary<(int, int), DiplomacyStatus>();
        private IWorldSimulationService worldSimulation;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ServiceLocator.TryResolve(out worldSimulation);
        }

        private void Start()
        {
            InitializeDiplomacy();
        }

        public void InitializeDiplomacy()
        {
            diplomacyMatrix.Clear();
            var factions = GetFactions();
            if (factions == null)
            {
                return;
            }

            foreach (var factionA in factions)
            {
                foreach (var factionB in factions)
                {
                    if (factionA.Id != factionB.Id)
                    {
                        diplomacyMatrix[(factionA.Id, factionB.Id)] = DiplomacyStatus.Neutral;
                        diplomacyMatrix[(factionB.Id, factionA.Id)] = DiplomacyStatus.Neutral;
                    }
                }
            }

            for (int i = 0; i < factions.Count; i++)
            {
                for (int j = i + 1; j < factions.Count; j++)
                {
                    float rand = Random.value;
                    if (rand < 0.2f)
                    {
                        FormAlliance(factions[i].Id, factions[j].Id);
                    }
                    else if (rand < 0.3f)
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

        private IList<Faction> GetFactions()
        {
            if (worldSimulation == null)
            {
                ServiceLocator.TryResolve(out worldSimulation);
            }

            if (worldSimulation == null)
            {
                Debug.LogError("FactionDiplomacy requires an IWorldSimulationService.");
                return null;
            }

            return worldSimulation.State.Factions;
        }
    }
}
