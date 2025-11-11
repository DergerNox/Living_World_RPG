using System.Collections.Generic;
using UnityEngine;

namespace LivingWorldRPG.World
{
public enum WorldEventType { War, Quest, Siege, Growth }

public class WorldEvent
{
    public WorldEventType Type { get; set; }
    public string Description { get; set; }
    public int Duration { get; set; }
    public int FactionId { get; set; }
    public Vector3Int Location { get; set; }

    public WorldEvent(WorldEventType type, string description, int duration, int factionId, Vector3Int location)
    {
        Type = type;
        Description = description;
        Duration = duration;
        FactionId = factionId;
        Location = location;
    }
}

public class WorldEventManager : MonoBehaviour
{
    public static WorldEventManager Instance { get; private set; }

    private readonly List<WorldEvent> activeEvents = new List<WorldEvent>();
    private IQuestService questService;
    private ITimeService timeService;
    private IWorldSimulationService worldSimulation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResolveQuestService();
        ServiceLocator.TryResolve(out worldSimulation);
    }

    private void Start()
    {
        var time = ResolveTimeService();
        if (time != null)
        {
            time.OnDayChanged += ProcessDailyEvents;
        }
    }

    private void OnDestroy()
    {
        if (timeService != null)
        {
            timeService.OnDayChanged -= ProcessDailyEvents;
        }
    }

    private void ProcessDailyEvents()
    {
        // Process active events
        for (int i = activeEvents.Count - 1; i >= 0; i--)
        {
            activeEvents[i].Duration--;
            if (activeEvents[i].Duration <= 0)
            {
                ResolveEvent(activeEvents[i]);
                activeEvents.RemoveAt(i);
            }
        }

        // Chance to trigger new events
        if (Random.value < 0.1f) // 10% chance per day
        {
            TriggerRandomEvent();
        }
    }

    private void TriggerRandomEvent()
    {
        var simulation = ResolveWorldSimulation();
        if (simulation == null || simulation.State.Factions.Count == 0)
        {
            return;
        }

        WorldEventType type = (WorldEventType)Random.Range(0, System.Enum.GetValues(typeof(WorldEventType)).Length);
        int factionId = Random.Range(0, simulation.State.Factions.Count);
        Vector3Int location = GetRandomSettlementLocation(factionId);

        string description = "";
        int duration = Random.Range(5, 15);

        switch (type)
        {
            case WorldEventType.War:
                description = $"Faction {factionId} declares war!";
                if (FactionDiplomacy.Instance != null && simulation.State.Factions.Count > 1)
                {
                    int targetFaction = Random.Range(0, simulation.State.Factions.Count);
                    if (targetFaction != factionId)
                    {
                        FactionDiplomacy.Instance.DeclareWar(factionId, targetFaction);
                    }
                }
                break;
            case WorldEventType.Quest:
                description = $"A new quest is available in Faction {factionId}.";
                GenerateQuestEvent(factionId);
                break;
            case WorldEventType.Siege:
                description = $"A settlement in Faction {factionId} is under siege!";
                TriggerSiegeEvent(location);
                break;
            case WorldEventType.Growth:
                description = $"Faction {factionId} experiences growth.";
                TriggerGrowthEvent(factionId);
                break;
        }

        WorldEvent newEvent = new WorldEvent(type, description, duration, factionId, location);
        activeEvents.Add(newEvent);
        Debug.Log($"World Event: {description}");
    }

    private Vector3Int GetRandomSettlementLocation(int factionId)
    {
        var simulation = ResolveWorldSimulation();
        var repo = simulation?.State;
        if (repo == null)
        {
            return Vector3Int.zero;
        }

        if (factionId < 0 || factionId >= repo.Factions.Count) return Vector3Int.zero;
        var faction = repo.Factions[factionId];
        var settlements = new List<ISettlement>();
        settlements.AddRange(faction.Castles);
        settlements.AddRange(faction.Towns);
        settlements.AddRange(faction.Villages);

        if (settlements.Count > 0)
        {
            var settlement = settlements[Random.Range(0, settlements.Count)];
            return new Vector3Int(settlement.PosX, settlement.PosY, 0);
        }
        return Vector3Int.zero;
    }

    private void GenerateQuestEvent(int factionId)
    {
        var player = ResolveWorldSimulation()?.State.Player;
        if (player == null) return;

        Quest quest = QuestGenerator.GenerateQuest(player.Level);
        var service = ResolveQuestService();
        service?.StartQuest(quest);
    }

    private void TriggerSiegeEvent(Vector3Int location)
    {
        // Find settlement at location and set siege flag
        var state = ResolveWorldSimulation()?.State;
        if (state == null) return;

        var castle = state.GetCastleAtPosition(location);
        if (castle != null)
        {
            castle.IsCastleUnderSiege = true;
        }
        var town = state.GetTownAtPosition(location);
        if (town != null)
        {
            town.IsTownUnderSiege = true;
        }
        var village = state.GetVillageAtPosition(location);
        if (village != null)
        {
            // Villages don't have siege flag, but we can simulate damage
            village.ProcessSiegeEvent();
        }
    }

    private void TriggerGrowthEvent(int factionId)
    {
        var repo = ResolveWorldSimulation()?.State;
        if (repo == null || factionId < 0 || factionId >= repo.Factions.Count) return;

        var faction = repo.Factions[factionId];
        foreach (var village in faction.Villages)
        {
            village.ProcessGrowthEvent();
        }
    }

    private void ResolveEvent(WorldEvent worldEvent)
    {
        switch (worldEvent.Type)
        {
            case WorldEventType.War:
                // War resolution logic
                Debug.Log($"War in Faction {worldEvent.FactionId} has ended.");
                break;
            case WorldEventType.Siege:
                // End siege
                var state = ResolveWorldSimulation()?.State;
                var castle = state?.GetCastleAtPosition(worldEvent.Location);
                if (castle != null) castle.IsCastleUnderSiege = false;
                var town = state?.GetTownAtPosition(worldEvent.Location);
                if (town != null) town.IsTownUnderSiege = false;
                Debug.Log($"Siege at {worldEvent.Location} has ended.");
                break;
            default:
                Debug.Log($"Event {worldEvent.Type} resolved.");
                break;
        }
    }

    private IQuestService ResolveQuestService()
    {
        if (questService == null)
        {
            if (!ServiceLocator.TryResolve(out questService) || questService == null)
            {
                Debug.LogError("WorldEventManager requires an IQuestService.");
                return null;
            }
        }

        return questService;
    }

    private ITimeService ResolveTimeService()
    {
        if (timeService == null)
        {
            if (!ServiceLocator.TryResolve(out timeService) || timeService == null)
            {
                Debug.LogError("WorldEventManager requires an ITimeService.");
                return null;
            }
        }
        return timeService;
    }

    private IWorldSimulationService ResolveWorldSimulation()
    {
        if (worldSimulation == null && !ServiceLocator.TryResolve(out worldSimulation))
        {
            Debug.LogError("WorldEventManager requires an IWorldSimulationService.");
        }

        return worldSimulation;
    }
}
}
