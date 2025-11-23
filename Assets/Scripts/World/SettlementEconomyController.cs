using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LivingWorldRPG.Core;

namespace LivingWorldRPG.World
{
    /// <summary>
    /// Drives the settlement economy forward every in-game day and coordinates faction reactions to shortages.
    /// </summary>
    public class SettlementEconomyController : MonoBehaviour
    {
        [SerializeField, Min(1)] private int daysPerTick = 1;
        [SerializeField] private bool runOnceOnStart = true;
        [SerializeField, Min(10)] private int convoyCargoSize = 60;
        [SerializeField, Min(0)] private int convoyCooldownDays = 2;
        [Header("Village Auto Exports")]
        [SerializeField, Min(50)] private int villageAutoExportThreshold = 220;
        [SerializeField, Min(10)] private int villageAutoExportAmount = 80;
        [Header("Town Trade Settings")]
        [SerializeField, Min(30)] private int townAdvancedTradeThreshold = 90;
        [SerializeField, Min(10)] private int townAdvancedTradeAmount = 40;
        [SerializeField, Range(1, 5)] private int townMaxTradeCaravans = 3;
        [Header("Settlement Gold Income")]
        [SerializeField, Min(0)] private int villageDailyGold = 15;
        [SerializeField, Min(0)] private int villageGoldPerLevel = 5;
        [SerializeField, Min(0)] private int townDailyGold = 50;
        [SerializeField, Min(0)] private int castleDailyGold = 30;
        [Header("Party Gold Economics")]
        [SerializeField, Min(0)] private int salaryPerTroop = 2;
        [SerializeField, Min(0)] private int villagerDailyEarnings = 1;
        [SerializeField, Min(0)] private Vector2 traderProfitRange = new Vector2(5, 15);
        [SerializeField, Min(0)] private int traderExtractionThreshold = 10;
        [SerializeField, Min(1)] private int traderExtractionAmount = 4;
        [SerializeField, Min(0)] private Vector2 banditLootRange = new Vector2(3, 12);

        private IWorldSimulationService worldSimulation;
        private ITimeService timeService;
        private IPartyService partyService;

        private readonly Dictionary<int, int> nextFoodConvoyDay = new Dictionary<int, int>();
        private readonly Dictionary<int, int> nextEquipmentConvoyDay = new Dictionary<int, int>();

        private void Awake()
        {
            ServiceLocator.TryResolve(out worldSimulation);
            ServiceLocator.TryResolve(out partyService);
            RegisterTimeService();
        }

        private void Start()
        {
            RegisterTimeService();
            if (worldSimulation != null)
            {
                worldSimulation.WorldGenerated += HandleWorldGenerated;
                if (worldSimulation.IsWorldReady)
                {
                    HandleWorldGenerated();
                }
            }

            if (runOnceOnStart)
            {
                ProcessEconomy(daysPerTick);
            }
        }

        private void OnDestroy()
        {
            if (timeService != null)
            {
                timeService.OnDayChanged -= HandleDayChanged;
            }

            if (worldSimulation != null)
            {
                worldSimulation.WorldGenerated -= HandleWorldGenerated;
            }
        }

        private void RegisterTimeService()
        {
            if (timeService == null && ServiceLocator.TryResolve(out timeService))
            {
                timeService.OnDayChanged += HandleDayChanged;
            }
        }

        private void HandleWorldGenerated()
        {
            ProcessEconomy(daysPerTick);
        }

        private void HandleDayChanged()
        {
            ProcessEconomy(daysPerTick);
        }

        private void ProcessEconomy(int days)
        {
            var state = worldSimulation?.State;
            if (state == null)
            {
                return;
            }

            SettlementEconomySystem.ProgressEconomy(state.Villages, state.Towns, state.Castles, Mathf.Max(1, days));
            DispatchVillageExports(state);
            DispatchTownTrades(state);
            FactionEconomyUtility.RefreshFactionSnapshots(state.Factions);
            ProcessSettlementGold(state);
            ProcessPartyGold();
            ReactToShortages(state);
        }

        private void DispatchVillageExports(IWorldStateRepository state)
        {
            if (state?.Villages == null || partyService == null)
            {
                return;
            }

            foreach (var village in state.Villages)
            {
                TryDispatchVillageConvoy(village, state);
            }
        }

        private void TryDispatchVillageConvoy(Village village, IWorldStateRepository state)
        {
            if (village?.ResourceLedger == null)
            {
                return;
            }

            var resourceToShip = FindOverflowResource(village);
            if (!resourceToShip.HasValue)
            {
                return;
            }

            ISettlement destination = SelectBestRecipient(village, resourceToShip.Value, state)
                                      ?? village.ConnectedTown
                                      ?? FindClosestSettlement(village.Position, state?.Towns?.Cast<ISettlement>(), state?.Castles?.Cast<ISettlement>());

            if (destination == null)
            {
                return;
            }

            int available = village.ResourceLedger.GetBasic(resourceToShip.Value);
            int payload = Mathf.Clamp(villageAutoExportAmount, 1, available);
            if (payload <= 0)
            {
                return;
            }

            if (!village.ResourceLedger.ConsumeBasic(resourceToShip.Value, payload))
            {
                return;
            }

            var faction = ResolveFaction(state, village.FactionId);
            partyService.SpawnResourceConvoy(faction, village.Position, GetSettlementPosition(destination), payload, resourceToShip.Value);
            Debug.Log($"Village {village.Name} shipped {payload} {resourceToShip} to {destination.GetType().Name} at {destination.PosX},{destination.PosY}.");
        }

        private ResourceType? FindOverflowResource(Village village)
        {
            if (village?.ResourceLedger?.BasicResources == null)
            {
                return null;
            }

            var candidate = village.ResourceLedger.BasicResources
                .Where(kvp => kvp.Value >= villageAutoExportThreshold)
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => (ResourceType?)kvp.Key)
                .FirstOrDefault();

            return candidate;
        }

        private void DispatchTownTrades(IWorldStateRepository state)
        {
            if (state?.Towns == null || partyService == null)
            {
                return;
            }

            foreach (var town in state.Towns)
            {
                TryDispatchTownConvoy(town, state);
            }
        }

        private void TryDispatchTownConvoy(Town town, IWorldStateRepository state)
        {
            if (town?.ResourceLedger == null)
            {
                return;
            }

            var surplus = FindAdvancedSurplus(town);
            if (!surplus.HasValue)
            {
                return;
            }

            int available = town.ResourceLedger.GetAdvanced(surplus.Value);
            int shipments = Mathf.Min(Mathf.Max(1, available / Mathf.Max(1, townAdvancedTradeAmount)), townMaxTradeCaravans);
            var faction = ResolveFaction(state, town.FactionId);

            for (int i = 0; i < shipments; i++)
            {
                int currentStock = town.ResourceLedger.GetAdvanced(surplus.Value);
                if (currentStock < townAdvancedTradeAmount)
                {
                    break;
                }

                bool consumed = town.ResourceLedger.ConsumeAdvanced(surplus.Value, townAdvancedTradeAmount);
                if (!consumed)
                {
                    break;
                }

                Town destination = SelectTownRecipient(town, surplus.Value, state);
                Vector3Int destPos = destination != null ? destination.Position : Vector3Int.zero;
                var convoy = partyService.SpawnResourceConvoy(faction, town.Position, destPos, townAdvancedTradeAmount, null, surplus.Value, true);
                Debug.Log(destination != null
                    ? $"Town {town.Name} dispatched trader caravan {i + 1} selling {townAdvancedTradeAmount} {surplus} to town {destination.Name}."
                    : $"Town {town.Name} sent caravan {i + 1} to seek markets for {surplus}.");
            }
        }

        private AdvancedResourcesTypes? FindAdvancedSurplus(Town town)
        {
            if (town?.ResourceLedger?.AdvancedResources == null)
            {
                return null;
            }

            foreach (var kvp in town.ResourceLedger.AdvancedResources.OrderByDescending(kvp => kvp.Value))
            {
                if (kvp.Value >= townAdvancedTradeThreshold)
                {
                    return kvp.Key;
                }
            }

            return null;
        }

        private Town SelectTownRecipient(Town source, AdvancedResourcesTypes resource, IWorldStateRepository state)
        {
            if (state?.Towns == null)
            {
                return null;
            }

            Town best = null;
            float bestScore = float.MinValue;
            foreach (var town in state.Towns)
            {
                if (town == null || town == source || town.FactionId != source.FactionId)
                {
                    continue;
                }

                float score = GetAdvancedNeedScore(town, resource);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = town;
                }
            }

            return bestScore > 0 ? best : null;
        }

        private ISettlement SelectBestRecipient(Village source, ResourceType resourceType, IWorldStateRepository state)
        {
            if (source == null || state == null)
            {
                return null;
            }

            List<ISettlement> candidates = new List<ISettlement>();
            if (state.Towns != null)
            {
                foreach (var town in state.Towns)
                {
                    if (town != null && town.FactionId == source.FactionId)
                    {
                        candidates.Add(town);
                    }
                }
            }

            if (state.Castles != null)
            {
                foreach (var castle in state.Castles)
                {
                    if (castle != null && castle.FactionId == source.FactionId)
                    {
                        candidates.Add(castle);
                    }
                }
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            ISettlement best = null;
            float bestScore = float.MinValue;
            foreach (var candidate in candidates)
            {
                float score = GetResourceNeedScore(candidate, resourceType);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return bestScore > float.MinValue ? best : null;
        }

        private float GetResourceNeedScore(ISettlement settlement, ResourceType resourceType)
        {
            var ledger = GetLedger(settlement);
            if (ledger == null)
            {
                return float.MinValue;
            }

            int current = ledger.GetBasic(resourceType);
            int desired = GetDesiredStock(settlement, resourceType);
            return desired - current;
        }

        private static SettlementResourceLedger GetLedger(ISettlement settlement)
        {
            return settlement switch
            {
                Village v => v.ResourceLedger,
                Town t => t.ResourceLedger,
                Castle c => c.ResourceLedger,
                _ => null
            };
        }

        private static int GetDesiredStock(ISettlement settlement, ResourceType resourceType)
        {
            switch (settlement)
            {
                case Town _:
                    return resourceType switch
                    {
                        ResourceType.Food => 220,
                        ResourceType.Wood => 180,
                        ResourceType.Iron => 200,
                        ResourceType.Clay => 170,
                        ResourceType.Textile => 160,
                        ResourceType.Herbs => 150,
                        ResourceType.Coal => 170,
                        ResourceType.Salt => 190,
                        _ => 140
                    };
                case Castle _:
                    return resourceType switch
                    {
                        ResourceType.Food => 160,
                        ResourceType.Wood => 200,
                        ResourceType.Iron => 220,
                        ResourceType.Stone => 180,
                        ResourceType.Clay => 180,
                        ResourceType.Coal => 200,
                        ResourceType.Salt => 150,
                        _ => 120
                    };
                default:
                    return 100;
            }
        }

        private float GetAdvancedNeedScore(Town town, AdvancedResourcesTypes resourceType)
        {
            if (town?.ResourceLedger == null)
            {
                return float.MinValue;
            }

            int current = town.ResourceLedger.GetAdvanced(resourceType);
            int desired = GetDesiredAdvancedStock(resourceType);
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

        private static ISettlement FindClosestSettlement(Vector3Int origin, IEnumerable<ISettlement> towns, IEnumerable<ISettlement> castles)
        {
            ISettlement best = null;
            float bestDist = float.MaxValue;

            void Consider(IEnumerable<ISettlement> set)
            {
                if (set == null)
                {
                    return;
                }

                foreach (var settlement in set)
                {
                    if (settlement == null)
                    {
                        continue;
                    }

                    Vector3Int pos = new Vector3Int(settlement.PosX, settlement.PosY, 0);
                    float dist = (pos - origin).sqrMagnitude;
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        best = settlement;
                    }
                }
            }

            Consider(towns);
            Consider(castles);
            return best;
        }

        private static Faction ResolveFaction(IWorldStateRepository state, int factionId)
        {
            if (state?.Factions == null || factionId < 0 || factionId >= state.Factions.Count)
            {
                return null;
            }

            return state.Factions[factionId];
        }

        private void ReactToShortages(IWorldStateRepository repository)
        {
            if (repository?.Factions == null || partyService == null)
            {
                return;
            }

            foreach (var faction in repository.Factions)
            {
                if (faction?.Economy == null)
                {
                    continue;
                }

                if (faction.Economy.HasFoodShortage)
                {
                    TryDispatchFoodConvoy(faction);
                }

                if (faction.Economy.HasEquipmentShortage)
                {
                    TryDispatchEquipmentConvoy(faction);
                }
            }
        }

        private void ProcessSettlementGold(IWorldStateRepository state)
        {
            if (state == null)
            {
                return;
            }

            foreach (var village in state.Villages)
            {
                if (village == null)
                {
                    continue;
                }
                int income = villageDailyGold + Mathf.Max(0, village.VillageLevel - 1) * villageGoldPerLevel;
                village.Treasury = Mathf.Max(0, village.Treasury + income);
            }

            foreach (var town in state.Towns)
            {
                if (town == null)
                {
                    continue;
                }
                int profit = townDailyGold + Mathf.Max(0, town.Income - town.Expenses);
                town.AddTreasury(profit);
            }

            foreach (var castle in state.Castles)
            {
                if (castle == null)
                {
                    continue;
                }
                castle.AddTreasury(castleDailyGold);
            }
        }

        private void ProcessPartyGold()
        {
            if (partyService == null)
            {
                ServiceLocator.TryResolve(out partyService);
                if (partyService == null)
                {
                    return;
                }
            }

            var parties = partyService.AllParties;
            if (parties == null)
            {
                return;
            }

            foreach (var party in parties)
            {
                if (party == null)
                {
                    continue;
                }

                // Pay salaries for all parties based on troop count
                int salary = salaryPerTroop * Mathf.Max(1, party.Troops.Count);
                party.Gold = Mathf.Max(0, party.Gold - salary);

                switch (party.PartyType)
                {
                    case PartyType.Trader:
                        {
                            if (GameFeatureFlags.TradingEnabled)
                            {
                                int profit = TryTraderProfitAtLocation(party);
                                party.Gold += Mathf.Max(0, profit);
                            }
                            break;
                        }
                    case PartyType.Bandits:
                        {
                            int loot = TryBanditLootAtLocation(party);
                            party.Gold += Mathf.Max(0, loot);
                            break;
                        }
                    case PartyType.Villagers:
                    case PartyType.ResourceConvoy:
                        {
                            party.Gold = Mathf.Max(0, party.Gold + villagerDailyEarnings);
                            break;
                        }
                }
            }
        }

        private int TryTraderProfitAtLocation(Party party)
        {
            if (party == null)
            {
                return 0;
            }

            var settlement = GetSettlementAtPosition(party.Position);
            if (!(settlement is Town town) || town.ResourceLedger == null)
            {
                return 0;
            }

            // If carrying cargo, try selling.
            if (party.TraderCargo.Count > 0)
            {
                int profit = 0;
                foreach (var cargo in party.TraderCargo)
                {
                    int pricePer = Random.Range((int)traderProfitRange.x, (int)traderProfitRange.y + 1);
                    int totalPrice = pricePer * cargo.Amount;
                    int paid = town.TakeTreasury(totalPrice);
                    // Add to town stock if paid
                    if (paid > 0)
                    {
                        if (cargo.AdvancedResource.HasValue)
                        {
                            town.ResourceLedger.AddAdvanced(cargo.AdvancedResource.Value, cargo.Amount);
                        }
                        else if (cargo.BasicResource.HasValue)
                        {
                            town.ResourceLedger.AddBasic(cargo.BasicResource.Value, cargo.Amount);
                        }
                        profit += paid;
                    }
                }
                party.TraderCargo.Clear();
                return profit;
            }

            // Otherwise, pick up surplus cargo.
            AdvancedResourcesTypes? target = null;
            int available = 0;
            foreach (var kvp in town.ResourceLedger.AdvancedResources)
            {
                if (kvp.Value > available && kvp.Value > traderExtractionThreshold)
                {
                    available = kvp.Value;
                    target = kvp.Key;
                }
            }

            if (!target.HasValue)
            {
                return 0;
            }

            int extract = Mathf.Min(traderExtractionAmount, available - traderExtractionThreshold);
            if (extract <= 0)
            {
                return 0;
            }

            town.ResourceLedger.ConsumeAdvanced(target.Value, extract);
            party.TraderCargo.Add(new TraderCargoItem { AdvancedResource = target.Value, Amount = extract });
            return 0;
        }

        private int TryBanditLootAtLocation(Party party)
        {
            if (party == null)
            {
                return 0;
            }

            var settlement = GetSettlementAtPosition(party.Position);
            if (settlement == null || settlement.FactionId == party.FactionId)
            {
                return 0;
            }

            int loot = Random.Range((int)banditLootRange.x, (int)banditLootRange.y + 1);
            switch (settlement)
            {
                case Town town:
                    loot = Mathf.Min(loot, town.TakeTreasury(loot));
                    break;
                case Castle castle:
                    loot = Mathf.Min(loot, castle.TakeTreasury(loot));
                    break;
                case Village village:
                    int take = Mathf.Min(loot, village.Treasury);
                    village.Treasury -= take;
                    loot = take;
                    break;
            }

            return Mathf.Max(0, loot);
        }

        private ISettlement GetSettlementAtPosition(Vector3Int position)
        {
            var repo = worldSimulation?.State;
            if (repo == null)
            {
                return null;
            }

            var town = repo.GetTownAtPosition(position);
            if (town != null) return town;
            var castle = repo.GetCastleAtPosition(position);
            if (castle != null) return castle;
            var village = repo.GetVillageAtPosition(position);
            return village;
        }

        private bool TryDispatchFoodConvoy(Faction faction)
        {
            if (!CanSendConvoy(faction.Id, nextFoodConvoyDay))
            {
                return false;
            }

            var donor = faction.Villages?.FirstOrDefault(v => v?.ResourceLedger?.GetBasic(ResourceType.Food) >= convoyCargoSize)
                        ?? faction.Villages?.FirstOrDefault();
            ISettlement receiver = faction.Towns?.FirstOrDefault() ?? (ISettlement)faction.Castles?.FirstOrDefault();
            if (donor == null || receiver == null)
            {
                return false;
            }

            partyService.SpawnResourceConvoy(faction, donor.Position, GetSettlementPosition(receiver), convoyCargoSize, ResourceType.Food);
            MarkConvoySent(faction.Id, nextFoodConvoyDay);
            return true;
        }

        private bool TryDispatchEquipmentConvoy(Faction faction)
        {
            if (!CanSendConvoy(faction.Id, nextEquipmentConvoyDay))
            {
                return false;
            }

            var donor = faction.Towns?.FirstOrDefault(t => t?.ResourceLedger?.GetAdvanced(AdvancedResourcesTypes.Steel) >= convoyCargoSize / 2)
                        ?? faction.Towns?.FirstOrDefault();
            ISettlement receiver = faction.Castles?.FirstOrDefault();
            if (donor == null || receiver == null)
            {
                return false;
            }

            int payload = Mathf.Max(10, convoyCargoSize / 2);
            partyService.SpawnResourceConvoy(faction, donor.Position, GetSettlementPosition(receiver), payload, null, AdvancedResourcesTypes.Steel);
            MarkConvoySent(faction.Id, nextEquipmentConvoyDay);
            return true;
        }

        private bool CanSendConvoy(int factionId, Dictionary<int, int> cooldownMap)
        {
            if (timeService == null || convoyCooldownDays <= 0)
            {
                return true;
            }

            int absoluteDay = GetAbsoluteDay();
            return !cooldownMap.TryGetValue(factionId, out int readyDay) || absoluteDay >= readyDay;
        }

        private void MarkConvoySent(int factionId, Dictionary<int, int> cooldownMap)
        {
            if (timeService == null || convoyCooldownDays <= 0)
            {
                return;
            }

            cooldownMap[factionId] = GetAbsoluteDay() + convoyCooldownDays;
        }

        private int GetAbsoluteDay()
        {
            if (timeService == null)
            {
                return 0;
            }

            return timeService.Year * 512 + timeService.Month * 32 + timeService.Day;
        }

        private static Vector3Int GetSettlementPosition(ISettlement settlement)
        {
            return new Vector3Int(settlement.PosX, settlement.PosY, 0);
        }
    }
}
