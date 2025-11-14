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
    [SerializeField] private WalkerGenerator walkerGenerator;

    private IWorldSimulationService worldSimulationService;
    private bool hasSpawned;

    private void OnEnable()
    {
        WorldGenerationState.GenerationStateChanged += HandleGenerationStateChanged;
    }

    private void OnDisable()
    {
        WorldGenerationState.GenerationStateChanged -= HandleGenerationStateChanged;
        if (walkerGenerator != null)
        {
            walkerGenerator.GenerationCompleted -= HandleWorldReady;
        }
    }

    private void Start()
    {
        if (WorldGenerationState.IsGenerating)
        {
            return;
        }

        InitializeAndSpawn();
    }

    private void OnDestroy()
    {
        if (walkerGenerator != null)
        {
            walkerGenerator.GenerationCompleted -= HandleWorldReady;
        }
    }

    private void HandleWorldReady()
    {
        walkerGenerator.GenerationCompleted -= HandleWorldReady;
        SpawnNPCs();
    }

    private void HandleGenerationStateChanged(bool generating)
    {
        if (!generating && walkerGenerator == null)
        {
            InitializeAndSpawn();
        }
    }

    private void InitializeAndSpawn()
    {
        if (walkerGenerator == null)
        {
            walkerGenerator = FindFirstObjectByType<WalkerGenerator>();
        }

        if (walkerGenerator != null)
        {
            if (walkerGenerator.IsGenerationComplete)
            {
                SpawnNPCs();
            }
            else
            {
                walkerGenerator.GenerationCompleted += HandleWorldReady;
            }
        }
        else
        {
            Debug.LogWarning("NPCSpawner could not find a WalkerGenerator. Spawning NPCs immediately.");
            SpawnNPCs();
        }
    }

    private void SpawnNPCs()
    {
        if (hasSpawned)
        {
            return;
        }

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

            NPCView view = npcObj.GetComponent<NPCView>();
            if (view == null)
            {
                view = npcObj.AddComponent<NPCView>();
            }

            var npcData = new NPC($"NPC_{spawnedCount}", 100, 50, Random.Range(1, 5));
            npcData.Initialize(
                name: npcData.Name,
                factionId: chosenSettlement.FactionId,
                position: new Vector3Int(chosenSettlement.PosX, chosenSettlement.PosY, 0));

            view.Initialize(npcData, worldPos);

            spawnedCount++;
            // Optionally remove the location to avoid overcrowding same spot:
            possibleLocations.RemoveAt(index);
        }

        Debug.Log($"Spawned {spawnedCount} NPCs.");
        hasSpawned = true;
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
