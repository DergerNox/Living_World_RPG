using System.Collections.Generic;
using UnityEngine;
using LivingWorldRPG.World;

namespace LivingWorldRPG.World
{
    public class FactionMaker : MonoBehaviour
    {
        public static FactionMaker Instance { get; private set; }

        [SerializeField, Min(1)] private int numberOfFactions = 5;

        private readonly List<Faction> factions = new List<Faction>();
        private IWorldSimulationService worldSimulation;

        public IReadOnlyList<Faction> Factions => factions;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (!ServiceLocator.TryResolve(out worldSimulation))
            {
                Debug.LogError("FactionMaker requires an IWorldSimulationService.");
                enabled = false;
            }
        }

        public void AssignFactions(Dictionary<Vector3Int, Castle> castles,
                                    Dictionary<Vector3Int, Town> towns,
                                    Dictionary<Vector3Int, Village> villages)
        {
            if (worldSimulation == null)
            {
                Debug.LogError("FactionMaker cannot assign factions without a world simulation service.");
                return;
            }

            CreateFactions();

            Dictionary<Vector3Int, ISettlement> allSettlements = new Dictionary<Vector3Int, ISettlement>();
            foreach (var c in castles) allSettlements[c.Key] = c.Value;
            foreach (var t in towns) allSettlements[t.Key] = t.Value;
            foreach (var v in villages) allSettlements[v.Key] = v.Value;

            List<Vector3Int> unassigned = new List<Vector3Int>(allSettlements.Keys);

            List<Vector3Int> seeds = new List<Vector3Int>();
            for (int i = 0; i < numberOfFactions && unassigned.Count > 0; i++)
            {
                int index = Random.Range(0, unassigned.Count);
                Vector3Int pos = unassigned[index];
                seeds.Add(pos);
                unassigned.RemoveAt(index);

                ISettlement seedSettlement = allSettlements[pos];
                factions[i].Center = new Vector2(seedSettlement.PosX, seedSettlement.PosY);
            }

            Queue<(Vector3Int, Faction)> frontier = new Queue<(Vector3Int, Faction)>();
            for (int i = 0; i < seeds.Count; i++)
            {
                Vector3Int seed = seeds[i];
                Faction faction = factions[i];
                AssignSettlementToFaction(allSettlements, seed, faction);
                frontier.Enqueue((seed, faction));
            }

            Vector3Int[] directions = new Vector3Int[]
            {
                new Vector3Int(1,0,0), new Vector3Int(-1,0,0),
                new Vector3Int(0,1,0), new Vector3Int(0,-1,0)
            };

            while (unassigned.Count > 0 && frontier.Count > 0)
            {
                var (currentPos, faction) = frontier.Dequeue();

                foreach (var dir in directions)
                {
                    Vector3Int neighbor = currentPos + dir;
                    if (unassigned.Contains(neighbor))
                    {
                        AssignSettlementToFaction(allSettlements, neighbor, faction);
                        unassigned.Remove(neighbor);
                        frontier.Enqueue((neighbor, faction));
                    }
                }
            }

            foreach (var pos in unassigned)
            {
                Faction faction = factions[Random.Range(0, factions.Count)];
                AssignSettlementToFaction(allSettlements, pos, faction);
            }

            Debug.Log("Factions assigned without border gore.");
        }

        private void AssignSettlementToFaction(Dictionary<Vector3Int, ISettlement> all, Vector3Int pos, Faction faction)
        {
            if (!all.ContainsKey(pos)) return;

            ISettlement s = all[pos];
            s.FactionId = faction.Id;

            switch (s)
            {
                case Castle c:
                    faction.Castles.Add(c);
                    break;
                case Town t:
                    faction.Towns.Add(t);
                    break;
                case Village v:
                    faction.Villages.Add(v);
                    break;
            }
        }

        private void CreateFactions()
        {
            factions.Clear();
            worldSimulation?.State.Factions.Clear();

            for (int i = 0; i < numberOfFactions; i++)
            {
                string name = $"Faction {i + 1}";
                Vector2 center = Vector2.zero;
                Faction faction = new Faction(name, center, i);
                factions.Add(faction);
                worldSimulation?.State.Factions.Add(faction);
            }
        }
    {
    }
}
