using System.Collections.Generic;
using UnityEngine;
using LivingWorldRPG.Characters;
using LivingWorldRPG.World;

namespace LivingWorldRPG.Spawning
{
public class NPCSpawner : MonoBehaviour
{
    [SerializeField] private GameObject npcPrefab; // Assign your NPC prefab here in inspector
    [SerializeField] private int numberOfNPCs = 10;
    [SerializeField] private Transform npcParent;  // Optional: parent transform to keep hierarchy clean

    private IWorldSimulationService worldSimulationService;

    void Start()
    {
        SpawnNPCs();
    }


    void SpawnNPCs()
    {
        if (npcPrefab == null)
        {
            Debug.LogError("NPC Prefab is not assigned!");
            return;
        }

        // Access your settlements and factions from WorldHandler
        var simulation = ResolveWorldSimulation();
        if (simulation == null)
        {
            Debug.LogError("World simulation service unavailable. Cannot spawn NPCs.");
            return;
        }

        List<ISettlement> possibleLocations = new List<ISettlement>(simulation.GetSettlements());

        int spawnedCount = 0;
        while (spawnedCount < numberOfNPCs && possibleLocations.Count > 0)
        {
            // Pick a random settlement
            int index = Random.Range(0, possibleLocations.Count);
            ISettlement chosenSettlement = possibleLocations[index];

            // Convert settlement pos to world pos (assuming you have a way to convert grid coords)
            Vector3 worldPos = new Vector3(chosenSettlement.PosX + 0.5f, chosenSettlement.PosY + 0.5f, 0);

            // Instantiate NPC
            GameObject npcObj = Instantiate(npcPrefab, worldPos, Quaternion.identity, npcParent);
            NPC npc = npcObj.GetComponent<NPC>();
            if (npc != null)
            {
                // Initialize NPC with faction and position info
                npc.Initialize(
                    name: "NPC_" + spawnedCount,
                    factionId: chosenSettlement.FactionId,
                    position: new Vector3Int(chosenSettlement.PosX, chosenSettlement.PosY, 0));
            }

            spawnedCount++;
            // Optionally remove the location to avoid overcrowding same spot:
            possibleLocations.RemoveAt(index);
        }

        Debug.Log($"Spawned {spawnedCount} NPCs.");
    }

    private IWorldSimulationService ResolveWorldSimulation()
    {
        if (worldSimulationService == null)
        {
            if (!ServiceLocator.TryResolve(out worldSimulationService))
            {
                return null;
            }
        }
        return worldSimulationService;
    }
}
}
