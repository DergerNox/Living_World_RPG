using UnityEngine;
using LivingWorldRPG.World;
using LivingWorldRPG.Interactions;
using LivingWorldRPG.Core;

namespace LivingWorldRPG.Characters
{
    /// <summary>
    /// Keeps an NPC's party position in sync with its transform so party encounters/trade logic can see it.
    /// Also allows traders to lightly interact with town markets on arrival.
    /// </summary>
    public class NPCPartySync : MonoBehaviour
    {
        [SerializeField] private Party party;
        [SerializeField] private NPCRole role;
        [SerializeField] private float syncInterval = 0.5f;

        private float timer;
        private float nodeSize = 1f;
        private IWorldSimulationService worldSimulation;

        public void Initialize(Party party, NPCRole role)
        {
            this.party = party;
            this.role = role;
        }

        private void Start()
        {
            ServiceLocator.TryResolve(out worldSimulation);
            if (PathfindingManager.Instance != null)
            {
                nodeSize = PathfindingManager.Instance.NodeSize;
            }
        }

        private void Update()
        {
            if (party == null)
            {
                return;
            }

            timer += Time.deltaTime;
            if (timer < syncInterval)
            {
                return;
            }
            timer = 0f;

            Vector3Int cell = Vector3Int.FloorToInt(transform.position / nodeSize);
            party.Position = cell;

            if (role == NPCRole.Trader)
            {
                TryTradeAtPosition(cell);
            }
        }

        private void TryTradeAtPosition(Vector3Int cell)
        {
            if (!GameFeatureFlags.TradingEnabled)
            {
                return;
            }

            var repo = worldSimulation?.State;
            if (repo == null)
            {
                return;
            }

            Town town = repo.GetTownAtPosition(cell);
            if (town == null || town.Market == null)
            {
                return;
            }

            // Simple effect: increase supply of a generic “Trade” item, driving prices down slightly.
            var market = town.Market;
            const string genericItem = "TraderGoods";
            if (!market.supply.ContainsKey(genericItem))
            {
                market.supply[genericItem] = 0;
                market.demand[genericItem] = 0;
                market.prices[genericItem] = 10;
            }
            market.supply[genericItem] += 1;
            market.UpdatePrices();
        }
    }
}
