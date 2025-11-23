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
    [SerializeField] private bool useDynamicCounts = true;
    [SerializeField, Min(0f)] private float npcsPerVillage = 0.5f;
    [SerializeField, Min(0f)] private float npcsPerTown = 1.5f;
    [SerializeField, Min(0f)] private float npcsPerCastle = 2f;
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

        int targetCount = DetermineTargetNpcCount(simulation);
        int spawnedCount = 0;
        while (spawnedCount < targetCount && possibleLocations.Count > 0)
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

            NPCRole role = ChooseRole(chosenSettlement);
            var npcData = new NPC($"NPC_{role}_{spawnedCount}", 100, 50, Random.Range(1, 5));
            npcData.Initialize(
                name: npcData.Name,
                factionId: chosenSettlement.FactionId,
                position: new Vector3Int(chosenSettlement.PosX, chosenSettlement.PosY, 0),
                role: role);
            SeedTroops(npcData);

            view.Initialize(npcData, worldPos);

            SetupAI(npcObj, npcData, simulation);

            spawnedCount++;
            // Optionally remove the location to avoid overcrowding same spot:
            possibleLocations.RemoveAt(index);
        }

        Debug.Log($"Spawned {spawnedCount} NPCs.");
        hasSpawned = true;
    }

    private int DetermineTargetNpcCount(IWorldSimulationService simulation)
    {
        if (!useDynamicCounts)
        {
            return Mathf.Max(1, numberOfNPCs);
        }

        var state = simulation.State;
        int villages = state?.Villages?.Count ?? 0;
        int towns = state?.Towns?.Count ?? 0;
        int castles = state?.Castles?.Count ?? 0;

        float total = villages * npcsPerVillage + towns * npcsPerTown + castles * npcsPerCastle;
        return Mathf.Max(1, Mathf.CeilToInt(total));
    }

    private void SetupAI(GameObject npcObj, NPC npcData, IWorldSimulationService simulation)
    {
        if (npcObj == null || simulation == null || npcData == null)
        {
            return;
        }

        var stateMachine = npcObj.GetComponent<AIStateMachine>() ?? npcObj.AddComponent<AIStateMachine>();
        var decisionMaker = npcObj.GetComponent<NPCDecisionMaker>() ?? npcObj.AddComponent<NPCDecisionMaker>();
        var controller = npcObj.GetComponent<NPC2DController>() ?? npcObj.AddComponent<NPC2DController>();

        // Ensure required movement components exist
        var rb2d = npcObj.GetComponent<Rigidbody2D>() ?? npcObj.AddComponent<Rigidbody2D>();
        rb2d.gravityScale = 0f;
        rb2d.freezeRotation = true;

        var col2d = npcObj.GetComponent<Collider2D>() ?? npcObj.AddComponent<CircleCollider2D>();
        col2d.isTrigger = false;

        if (controller == null)
        {
            Debug.LogWarning($"NPCSpawner: Failed to get NPC2DController on {npcObj.name}.");
            return;
        }

        // Build a lightweight waypoint list based on same-faction settlements.
        var waypoints = BuildWaypointsForRole(simulation, npcData);
        if (waypoints == null || waypoints.Count == 0)
        {
            var wp = new GameObject($"WP_{npcData.Name}_home").transform;
            wp.SetParent(npcParent != null ? npcParent : transform);
            wp.position = npcObj.transform.position;
            waypoints = new List<Transform> { wp };
        }

        controller.waypoints = waypoints.ToArray();
        controller.enabled = true;
        if (stateMachine != null)
        {
            stateMachine.SetState(AIState.Patrolling);
        }

        // Register this NPC as a moving party for encounters.
        RegisterPartyForNPC(npcData, npcObj.transform);
    }

    private List<Transform> BuildWaypointsForRole(IWorldSimulationService simulation, NPC npcData)
    {
        var list = new List<Transform>();

        if (simulation == null || npcData == null)
        {
            return list;
        }

        var settlements = new List<ISettlement>(simulation.GetSettlements());

        List<ISettlement> candidates = new List<ISettlement>();
        if (npcData.Role == NPCRole.Trader)
        {
            candidates.AddRange(simulation.GetSettlements(s => s is Town && s.FactionId == npcData.FactionId));
        }
        else if (npcData.Role == NPCRole.Guard)
        {
            candidates.AddRange(simulation.GetSettlements(s => s is Castle && s.FactionId == npcData.FactionId));
            candidates.AddRange(simulation.GetSettlements(s => s is Village && s.FactionId == npcData.FactionId));
        }
        else if (npcData.Role == NPCRole.Bandit)
        {
            candidates.AddRange(settlements.FindAll(s => s.FactionId != npcData.FactionId));
        }
        else
        {
            candidates.AddRange(simulation.GetSettlements(s => s.FactionId == npcData.FactionId));
        }

        if (candidates.Count == 0)
        {
            candidates.AddRange(settlements);
        }

        int waypointCount = Mathf.Clamp(candidates.Count, 1, 4);
        Transform parentForWps = npcParent != null ? npcParent : transform;
        for (int i = 0; i < waypointCount; i++)
        {
            var s = candidates[Random.Range(0, candidates.Count)];
            var wp = new GameObject($"WP_{npcData.Name}_{i}").transform;
            wp.SetParent(parentForWps);
            wp.position = new Vector3(s.PosX + 0.5f, s.PosY + 0.5f, 0f);
            list.Add(wp);
        }

        return list;
    }

    private void RegisterPartyForNPC(NPC npc, Transform actorTransform)
    {
        if (npc == null)
        {
            return;
        }

        if (npc.Party == null)
        {
            npc.Party = new Party { Leader = npc, FactionId = npc.FactionId };
        }

        var party = npc.Party;
        party.PartyType = MapRoleToPartyType(npc.Role);
        party.Position = Vector3Int.FloorToInt(actorTransform.position);

        if (ServiceLocator.TryResolve(out IPartyService partyService) && partyService != null)
        {
            partyService.AddParty(party);
        }

        // Keep the party position in sync with the NPC transform for encounter logic.
        var sync = actorTransform.GetComponent<NPCPartySync>() ?? actorTransform.gameObject.AddComponent<NPCPartySync>();
        sync.Initialize(party, npc.Role);
    }

    private PartyType MapRoleToPartyType(NPCRole role)
    {
        switch (role)
        {
            case NPCRole.Trader:
                return PartyType.Trader;
            case NPCRole.Guard:
                return PartyType.Military;
            case NPCRole.Bandit:
                return PartyType.Bandits;
            case NPCRole.Scout:
                return PartyType.ResourceConvoy;
            default:
                return PartyType.Villagers;
        }
    }

    private void SeedTroops(NPC npc)
    {
        if (npc == null)
        {
            return;
        }

        if (npc.Party == null)
        {
            npc.Party = new Party { Leader = npc, FactionId = npc.FactionId };
        }

        var party = npc.Party;
        int troopCount = Random.Range(1, 4);
        for (int i = 0; i < troopCount; i++)
        {
            TroopType type = (TroopType)Random.Range(0, System.Enum.GetValues(typeof(TroopType)).Length);
            Troop t = new Troop(type);
            t.Level = Random.Range(1, 3);
            t.UpdateRank();
            party.Troops.Add(t);
        }
    }

    private NPCRole ChooseRole(ISettlement settlement)
    {
        if (settlement == null)
        {
            return NPCRole.Villager;
        }
        if (settlement is Castle)
        {
            return NPCRole.Guard;
        }

        if (settlement is Town)
        {
            return Random.value < 0.4f ? NPCRole.Trader : NPCRole.Villager;
        }

        // Villages: mostly villagers, occasional scout/bandit.
        float roll = Random.value;
        if (roll < 0.1f) return NPCRole.Scout;
        if (roll < 0.2f) return NPCRole.Bandit;
        return NPCRole.Villager;
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
