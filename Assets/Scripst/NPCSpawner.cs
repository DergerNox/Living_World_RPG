using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject npcPrefab; // Assign your NPC prefab here in inspector
    public int numberOfNPCs = 10;
    public Transform npcParent;  // Optional: parent transform to keep hierarchy clean
    public

    private IWorldStateRepository worldStateRepository;

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
        var repo = GetWorldStateRepository();
        if (repo == null)
        {
            Debug.LogError("World state repository unavailable. Cannot spawn NPCs.");
            return;
        }

        var factions = repo.Factions;
        var towns = repo.Towns;
        var castles = repo.Castles;
        var villages = repo.Villages;

        // For demo: spawn NPCs mostly in towns or castles
        List<ISettlement> possibleLocations = new List<ISettlement>();
        possibleLocations.AddRange(towns);
        possibleLocations.AddRange(castles);
        possibleLocations.AddRange(villages);

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

    private IWorldStateRepository GetWorldStateRepository()
    {
        if (worldStateRepository == null)
        {
            if (!ServiceLocator.TryResolve(out worldStateRepository) || worldStateRepository == null)
            {
                Debug.LogError("NPCSpawner requires an IWorldStateRepository.");
                return null;
            }
        }
        return worldStateRepository;
    }
}
