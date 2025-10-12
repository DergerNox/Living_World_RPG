using System.Collections.Generic;
using UnityEngine;

public class FactionMaker : MonoBehaviour
{
    public static FactionMaker Instance { get; private set; }

    public int numberOfFactions = 5;
    public List<Faction> Factions = new List<Faction>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AssignFactions(Dictionary<Vector3Int, Castle> castles,
                                Dictionary<Vector3Int, Town> towns,
                                Dictionary<Vector3Int, Village> villages)
    {
        CreateFactions();

        // Combine all settlements into one dictionary
        Dictionary<Vector3Int, ISettlement> allSettlements = new Dictionary<Vector3Int, ISettlement>();
        foreach (var c in castles) allSettlements[c.Key] = c.Value;
        foreach (var t in towns) allSettlements[t.Key] = t.Value;
        foreach (var v in villages) allSettlements[v.Key] = v.Value;

        List<Vector3Int> unassigned = new List<Vector3Int>(allSettlements.Keys);

        // Choose seed positions
        List<Vector3Int> seeds = new List<Vector3Int>();
        for (int i = 0; i < numberOfFactions && unassigned.Count > 0; i++)
        {
            int index = Random.Range(0, unassigned.Count);
            Vector3Int pos = unassigned[index];
            seeds.Add(pos);
            unassigned.RemoveAt(index);

            ISettlement seedSettlement = allSettlements[pos];
            Factions[i].Center = new Vector2(seedSettlement.PosX, seedSettlement.PosY);
        }

        Queue<(Vector3Int, Faction)> frontier = new Queue<(Vector3Int, Faction)>();
        for (int i = 0; i < seeds.Count; i++)
        {
            Vector3Int seed = seeds[i];
            Faction faction = Factions[i];
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

        // Assign any leftovers randomly
        foreach (var pos in unassigned)
        {
            Faction faction = Factions[Random.Range(0, Factions.Count)];
            AssignSettlementToFaction(allSettlements, pos, faction);
        }

        Debug.Log("Factions assigned without border gore.");
    }

    void AssignSettlementToFaction(Dictionary<Vector3Int, ISettlement> all, Vector3Int pos, Faction faction)
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

   void CreateFactions()
{
    Factions.Clear();
    WorldHandler.Factions.Clear(); // Clear any previous data

    for (int i = 0; i < numberOfFactions; i++)
    {
        string name = $"Faction {i + 1}";
        Vector2 center = Vector2.zero; // Will be set later
        Faction faction = new Faction(name, center, i);
        Factions.Add(faction);
        WorldHandler.Factions.Add(faction); // Sync with WorldHandler
    }
}
}
