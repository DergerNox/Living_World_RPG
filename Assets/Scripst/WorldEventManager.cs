using System.Collections.Generic;
using UnityEngine;

public enum WorldEventType { War, Quest, Siege, Growth }

public class WorldEvent
{
    public WorldEventType Type { get; set; }
    public string Description { get; set; }
    public int Duration { get; set; } // in days
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
    public static WorldEventManager Instance;

    private List<WorldEvent> activeEvents = new List<WorldEvent>();
    private IQuestService questService;
    private ITimeService timeService;
    private IWorldStateRepository worldStateRepository;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ResolveQuestService();
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
        var repo = ResolveWorldStateRepository();
        if (repo == null || repo.Factions.Count == 0) return;

        WorldEventType type = (WorldEventType)Random.Range(0, System.Enum.GetValues(typeof(WorldEventType)).Length);
        int factionId = Random.Range(0, repo.Factions.Count);
        Vector3Int location = GetRandomSettlementLocation(factionId);

        string description = "";
        int duration = Random.Range(5, 15);

        switch (type)
        {
            case WorldEventType.War:
                description = $"Faction {factionId} declares war!";
                if (FactionDiplomacy.Instance != null && repo.Factions.Count > 1)
                {
                    int targetFaction = Random.Range(0, repo.Factions.Count);
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
        var repo = ResolveWorldStateRepository();
        if (repo == null) return Vector3Int.zero;

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
        var repo = ResolveWorldStateRepository();
        var player = repo?.Player;
        if (player == null) return;

        Quest quest = QuestGenerator.GenerateQuest(player.Level);
        var service = ResolveQuestService();
        service?.StartQuest(quest);
    }

    private void TriggerSiegeEvent(Vector3Int location)
    {
        // Find settlement at location and set siege flag
        var repo = ResolveWorldStateRepository();
        if (repo == null) return;

        var castle = repo.GetCastleAtPosition(location);
        if (castle != null)
        {
            castle.IsCastleUnderSiege = true;
        }
        var town = repo.GetTownAtPosition(location);
        if (town != null)
        {
            town.IsTownUnderSiege = true;
        }
        var village = repo.GetVillageAtPosition(location);
        if (village != null)
        {
            // Villages don't have siege flag, but we can simulate damage
            village.ProcessSiegeEvent();
        }
    }

    private void TriggerGrowthEvent(int factionId)
    {
        var repo = ResolveWorldStateRepository();
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
                var repo = ResolveWorldStateRepository();
                var castle = repo?.GetCastleAtPosition(worldEvent.Location);
                if (castle != null) castle.IsCastleUnderSiege = false;
                var town = repo?.GetTownAtPosition(worldEvent.Location);
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

    private IWorldStateRepository ResolveWorldStateRepository()
    {
        if (worldStateRepository == null)
        {
            if (!ServiceLocator.TryResolve(out worldStateRepository) || worldStateRepository == null)
            {
                Debug.LogError("WorldEventManager requires an IWorldStateRepository.");
                return null;
            }
        }
        return worldStateRepository;
    }
}
