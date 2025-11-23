using System.Collections.Generic;
using UnityEngine;
using LivingWorldRPG.World;

namespace LivingWorldRPG.PartySystem
{
    public class PartyManager : MonoBehaviour, IPartyService
    {
        public static PartyManager Instance { get; private set; }

        public Party PlayerParty { get; private set; }
        public List<Party> AllParties { get; private set; } = new List<Party>();
        IReadOnlyList<Party> IPartyService.AllParties => AllParties;
        private IWorldSimulationService worldSimulation;
        private readonly Dictionary<Vector3Int, List<Party>> encounterCache = new Dictionary<Vector3Int, List<Party>>();
        [SerializeField, Min(1)] private int traderSaleValuePerUnit = 5;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            ServiceLocator.TryResolve(out worldSimulation);
        }

        private void Start()
        {
            var player = worldSimulation?.State.Player;
            if (player != null)
            {
                PlayerParty = player.Party;
                if (!AllParties.Contains(PlayerParty))
                {
                    AllParties.Add(PlayerParty);
                }
            }
            else
            {
                Debug.LogWarning("PartyManager could not find a player party. AI party generation will continue without the player.");
            }

            GenerateAIParties();
        }

	void OnEnable()
	{
		TickSystem.OnSecond += OnSecondTick;
	}

	void OnDisable()
	{
		TickSystem.OnSecond -= OnSecondTick;
	}

    private void GenerateAIParties()
    {
        // Generate some AI parties for the world
        for (int i = 0; i < 5; i++)
        {
            Party aiParty = new Party();
            aiParty.Name = "AI Party " + (i + 1);
            aiParty.PartyType = (PartyType)Random.Range(0, System.Enum.GetValues(typeof(PartyType)).Length);
            aiParty.Position = new Vector3Int(Random.Range(0, 100), Random.Range(0, 100), 0);
            AllParties.Add(aiParty);
        }
    }

    public void AddParty(Party party)
    {
        if (!AllParties.Contains(party))
        {
            AllParties.Add(party);
        }
    }

    public void RemoveParty(Party party)
    {
        if (AllParties.Contains(party))
        {
            AllParties.Remove(party);
        }
    }

    public List<Party> GetPartiesAtPosition(Vector3Int position)
    {
        return AllParties.FindAll(p => p.Position == position);
    }

    public void UpdatePartyPositions()
    {
        var snapshot = new List<Party>(AllParties);
        foreach (Party party in snapshot)
        {
            if (party == null || party == PlayerParty)
            {
                continue;
            }

            if (party.PartyType == PartyType.Trader && (party.Destination == Vector3Int.zero || party.Destination == party.Position))
            {
                if (!AssignTraderDestination(party))
                {
                    if (party.SourcePosition != Vector3Int.zero)
                    {
                        party.Destination = party.SourcePosition;
                    }
                }
            }

            if (party.Destination != Vector3Int.zero && party.Position != party.Destination)
            {
                MovePartyTowardsDestination(party);
            }
            else
            {
                Vector3Int newPos = party.Position + new Vector3Int(Random.Range(-1, 2), Random.Range(-1, 2), 0);
                party.MoveTo(newPos);
            }

            if (party.Destination != Vector3Int.zero && party.Position == party.Destination)
            {
                HandlePartyArrival(party);
            }
        }

        ResolvePartyEncounters();
    }

	private void OnSecondTick()
	{
		UpdatePartyPositions();
	}

    public Party SpawnResourceConvoy(Faction faction, Vector3Int origin, Vector3Int destination, int amount, ResourceType? basicResource = null, AdvancedResourcesTypes? advancedResource = null, bool isTrader = false)
    {
        Party convoy = new Party
        {
            Name = $"{(faction?.Name ?? "Neutral")} Convoy",
            PartyType = isTrader ? PartyType.Trader : PartyType.ResourceConvoy,
            Position = origin,
            Destination = destination,
            SourcePosition = origin,
            FactionId = faction?.Id ?? -1,
            CarryWeight = amount * 2
        };

        convoy.TraderCargo.Add(new TraderCargoItem
        {
            BasicResource = basicResource,
            AdvancedResource = advancedResource,
            Amount = amount
        });

        AddParty(convoy);
        Debug.Log($"Spawned convoy '{convoy.Name}' with {(advancedResource.HasValue ? advancedResource.Value.ToString() : basicResource?.ToString() ?? "supplies")} headed to {destination}.");
        return convoy;
    }

    private void MovePartyTowardsDestination(Party party)
    {
        Vector3Int current = party.Position;
        Vector3Int target = party.Destination;
        int dx = Mathf.Clamp(target.x - current.x, -1, 1);
        int dy = Mathf.Clamp(target.y - current.y, -1, 1);
        Vector3Int next = new Vector3Int(current.x + dx, current.y + dy, 0);
        party.MoveTo(next);
    }

    private void HandlePartyArrival(Party party)
    {
        if (party.PartyType != PartyType.ResourceConvoy && party.PartyType != PartyType.Trader)
        {
            return;
        }

        var state = worldSimulation?.State;
        if (state != null)
        {
            ISettlement settlement = GetSettlementAtPosition(state, party.Position);
            if (settlement != null)
            {
                DepositCargo(settlement, party);
                if (party.PartyType == PartyType.Trader)
                {
                    RewardSourceTown(state, party, settlement);
                }
            }
        }

        RemoveParty(party);
    }

    private static ISettlement GetSettlementAtPosition(IWorldStateRepository state, Vector3Int position)
    {
        if (state == null)
        {
            return null;
        }

        var town = state.GetTownAtPosition(position);
        if (town != null) return town;

        var castle = state.GetCastleAtPosition(position);
        if (castle != null) return castle;

        var village = state.GetVillageAtPosition(position);
        return village;
    }

    private static void DepositCargo(ISettlement settlement, Party party)
    {
        if (party == null || party.TraderCargo == null || party.TraderCargo.Count == 0)
        {
            return;
        }

        foreach (var cargo in party.TraderCargo)
        {
            if (cargo.Amount <= 0)
            {
                continue;
            }

            if (cargo.BasicResource.HasValue)
            {
                switch (settlement)
                {
                    case Village village when village.ResourceLedger != null:
                        village.ResourceLedger.AddBasic(cargo.BasicResource.Value, cargo.Amount);
                        break;
                    case Town town when town.ResourceLedger != null:
                        town.ResourceLedger.AddBasic(cargo.BasicResource.Value, cargo.Amount);
                        break;
                    case Castle castle when castle.ResourceLedger != null:
                        castle.ResourceLedger.AddBasic(cargo.BasicResource.Value, cargo.Amount);
                        break;
                }
            }

            if (cargo.AdvancedResource.HasValue)
            {
                switch (settlement)
                {
                    case Town town when town.ResourceLedger != null:
                        town.ResourceLedger.AddAdvanced(cargo.AdvancedResource.Value, cargo.Amount);
                        break;
                    case Castle castle when castle.ResourceLedger != null:
                        castle.ResourceLedger.AddAdvanced(cargo.AdvancedResource.Value, cargo.Amount);
                        break;
                }
            }
        }

        party.TraderCargo.Clear();
    }

    private void RewardSourceTown(IWorldStateRepository state, Party party, ISettlement destinationSettlement)
    {
        if (party.SourcePosition == Vector3Int.zero || destinationSettlement == null)
        {
            return;
        }

        Town sourceTown = state.GetTownAtPosition(party.SourcePosition);
        Vector3Int destinationPos = new Vector3Int(destinationSettlement.PosX, destinationSettlement.PosY, 0);
        if (sourceTown == null || destinationPos == party.SourcePosition)
        {
            return;
        }

        // Simple commission for completing a trade route
        int payout = Mathf.Max(5, traderSaleValuePerUnit * 2);
        sourceTown.AddTreasury(payout);
        sourceTown.Income += payout / 2;
    }

    private bool AssignTraderDestination(Party party)
    {
        var cargo = party.TraderCargo.Find(c => c.AdvancedResource.HasValue);
        if (cargo == null || !cargo.AdvancedResource.HasValue)
        {
            return false;
        }

        var state = worldSimulation?.State;
        if (state?.Towns == null)
        {
            return false;
        }

        Town best = null;
        float bestScore = float.MinValue;
        foreach (var town in state.Towns)
        {
            if (town == null)
            {
                continue;
            }

            float need = EvaluateAdvancedNeed(town, cargo.AdvancedResource.Value);
            if (need <= 0f)
            {
                continue;
            }

            Vector3Int townPos = town.Position;
            float distancePenalty = (townPos - party.Position).sqrMagnitude * 0.1f;
            float factionBonus = town.FactionId == party.FactionId ? 10f : 0f;
            float score = need + factionBonus - distancePenalty;
            if (score > bestScore)
            {
                bestScore = score;
                best = town;
            }
        }

        if (best == null || bestScore <= 0f)
        {
            return false;
        }

        party.Destination = best.Position;
        return true;
    }

    private float EvaluateAdvancedNeed(Town town, AdvancedResourcesTypes resource)
    {
        if (town?.ResourceLedger == null)
        {
            return float.MinValue;
        }

        int desired = GetDesiredAdvancedStock(resource);
        int current = town.ResourceLedger.GetAdvanced(resource);
        return desired - current;
    }

    private int GetDesiredAdvancedStock(AdvancedResourcesTypes resourceType)
    {
        switch (resourceType)
        {
            case AdvancedResourcesTypes.Steel:
                return 120;
            case AdvancedResourcesTypes.ReinforcedWood:
                return 100;
            case AdvancedResourcesTypes.StoneBriks:
                return 110;
            case AdvancedResourcesTypes.Food:
                return 140;
            default:
                return 90;
        }
    }

    private void ResolvePartyEncounters()
    {
        if (AllParties.Count < 2)
        {
            return;
        }

        foreach (var party in AllParties)
        {
            if (party == null)
            {
                continue;
            }

            if (!encounterCache.TryGetValue(party.Position, out var list))
            {
                list = new List<Party>();
                encounterCache[party.Position] = list;
            }
            list.Add(party);
        }

        foreach (var kvp in encounterCache)
        {
            var parties = kvp.Value;
            if (parties.Count > 1)
            {
                HandleEncounterGroup(parties);
            }
            parties.Clear();
        }

        encounterCache.Clear();
    }

    private void HandleEncounterGroup(List<Party> parties)
    {
        for (int i = 0; i < parties.Count; i++)
        {
            var a = parties[i];
            if (!IsPartyActive(a))
            {
                continue;
            }

            for (int j = i + 1; j < parties.Count; j++)
            {
                var b = parties[j];
                if (!IsPartyActive(b))
                {
                    continue;
                }

                var action = EvaluateInteraction(a, b);
                switch (action)
                {
                    case PartyInteractionOutcome.Escort:
                        ResolveEscort(a, b);
                        break;
                    case PartyInteractionOutcome.Trade:
                        ResolveTrade(a, b);
                        break;
                    case PartyInteractionOutcome.Attack:
                        ResolveAttack(a, b);
                        break;
                }
            }
        }
    }

    private bool IsPartyActive(Party party) =>
        party != null && AllParties.Contains(party);

    private enum PartyInteractionOutcome
    {
        Ignore,
        Escort,
        Trade,
        Attack
    }

    private PartyInteractionOutcome EvaluateInteraction(Party a, Party b)
    {
        if (a == null || b == null)
        {
            return PartyInteractionOutcome.Ignore;
        }

        bool sameFaction = a.FactionId >= 0 && a.FactionId == b.FactionId;
        if (sameFaction)
        {
            if ((IsCombatant(a) && IsCivilian(b)) || (IsCombatant(b) && IsCivilian(a)))
            {
                return PartyInteractionOutcome.Escort;
            }

            return PartyInteractionOutcome.Ignore;
        }

        if (IsHostile(a) || IsHostile(b))
        {
            return PartyInteractionOutcome.Attack;
        }

        if (IsTrader(a) && IsTrader(b))
        {
            return PartyInteractionOutcome.Trade;
        }

        if ((IsTrader(a) && b.PartyType == PartyType.ResourceConvoy) ||
            (IsTrader(b) && a.PartyType == PartyType.ResourceConvoy))
        {
            return PartyInteractionOutcome.Trade;
        }

        return PartyInteractionOutcome.Ignore;
    }

    private static bool IsCombatant(Party party) =>
        party != null && (party.PartyType == PartyType.Military || party.PartyType == PartyType.Mercenaries || party.PartyType == PartyType.Bandits);

    private static bool IsHostile(Party party) => IsCombatant(party);

    private static bool IsTrader(Party party) =>
        party != null && party.PartyType == PartyType.Trader;

    private static bool IsCivilian(Party party) =>
        party != null && (party.PartyType == PartyType.Trader || party.PartyType == PartyType.ResourceConvoy || party.PartyType == PartyType.Villagers);

    private void ResolveEscort(Party a, Party b)
    {
        Party escort = IsCombatant(a) ? a : b;
        Party escorted = escort == a ? b : a;
        if (escort == null || escorted == null)
        {
            return;
        }

        if (escort.Destination == Vector3Int.zero && escorted.Destination != Vector3Int.zero)
        {
            escort.Destination = escorted.Destination;
        }

        if (escorted.Destination == Vector3Int.zero && escort.Destination != Vector3Int.zero)
        {
            escorted.Destination = escort.Destination;
        }

        Debug.Log($"{escort.Name} is escorting {escorted.Name} for faction {escort.FactionId}.");
    }

    private void ResolveTrade(Party a, Party b)
    {
        if (a == null || b == null)
        {
            return;
        }

        int cargoA = 0;
        int cargoB = 0;
        if (a.TraderCargo != null)
        {
            foreach (var c in a.TraderCargo) cargoA += Mathf.Max(0, c.Amount);
        }
        if (b.TraderCargo != null)
        {
            foreach (var c in b.TraderCargo) cargoB += Mathf.Max(0, c.Amount);
        }

        int tradeValue = Mathf.Max(5, Mathf.Min(Mathf.Max(1, cargoA), Mathf.Max(1, cargoB)));
        a.Gold += tradeValue;
        b.Gold += tradeValue;
        Debug.Log($"{a.Name} and {b.Name} traded goods valued at {tradeValue} gold.");
    }

        private void ResolveAttack(Party a, Party b)
        {
            if (a == null || b == null)
            {
                return;
            }

        bool aCombat = IsCombatant(a);
        bool bCombat = IsCombatant(b);

        if (!aCombat && !bCombat)
        {
            return;
        }

        if (aCombat && !bCombat)
        {
            RemoveParty(b);
            Debug.Log($"{a.Name} dispersed {b.Name} at {a.Position}.");
            return;
        }

        if (!aCombat && bCombat)
        {
            RemoveParty(a);
            Debug.Log($"{b.Name} dispersed {a.Name} at {b.Position}.");
            return;
        }

        int strengthA = GetPartyCombatPower(a);
        int strengthB = GetPartyCombatPower(b);
        if (strengthA == strengthB)
        {
            strengthA += Random.Range(-2, 3);
            strengthB += Random.Range(-2, 3);
        }

        if (strengthA >= strengthB)
        {
            TransferSpoils(a, b);
            RemoveParty(b);
            Debug.Log($"{a.Name} defeated {b.Name} (power {strengthA} vs {strengthB}).");
        }
        else
        {
            TransferSpoils(b, a);
            RemoveParty(a);
            Debug.Log($"{b.Name} defeated {a.Name} (power {strengthB} vs {strengthA}).");
        }
    }

    private void TransferSpoils(Party winner, Party loser)
    {
        if (winner == null || loser == null)
        {
            return;
        }

        int loot = Mathf.Max(0, loser.Gold);
        if (loot <= 0)
        {
            return;
        }

        // Only transfer a fraction to avoid wiping economies.
        int stolen = Mathf.Max(1, Mathf.RoundToInt(loot * 0.5f));
        winner.Gold += stolen;
        loser.Gold -= stolen;
    }

    private int GetPartyCombatPower(Party party)
    {
        if (party == null)
        {
            return 0;
        }

        int troopPower = party.Troops != null ? party.Troops.Count * 5 : 0;
        int memberPower = party.Members != null ? party.Members.Count * 2 : 0;
        int basePower = Mathf.Max(1, party.Size);
        int typeBonus = party.PartyType switch
        {
            PartyType.Military => 20,
            PartyType.Mercenaries => 15,
            PartyType.Bandits => 10,
            _ => 0
        };

        return basePower + troopPower + memberPower + typeBonus;
    }

        private IWorldSimulationService ResolveWorldSimulation()
        {
            if (worldSimulation == null && !ServiceLocator.TryResolve(out worldSimulation))
            {
                Debug.LogError("PartyManager requires an IWorldSimulationService.");
            }

            return worldSimulation;
        }
    }
}
