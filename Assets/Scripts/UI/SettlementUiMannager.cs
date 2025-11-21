using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using LivingWorldRPG.World;
using LivingWorldRPG.Core;

namespace LivingWorldRPG.UI
{
    public class SettlementUIManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject uiPanel;
        [SerializeField] private GameObject recruitmentPanel;
        [SerializeField] private GameObject troopSelectionPanel;

        [Header("Scripts")]
        [SerializeField] private TroopSelectionPanel troopSelectionPanelScript;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI settlementNameText;
        [SerializeField] private TextMeshProUGUI factionNameText;
        [SerializeField] private Button recruitButton;
        [SerializeField] private Button tradeButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private TextMeshProUGUI economySummaryText;
        [SerializeField] private TextMeshProUGUI factionNeedText;
        [SerializeField] private TextMeshProUGUI productionDetailsText;

        [Header("Troop List")]
        [SerializeField] private Transform troopContainer;
        [SerializeField] private GameObject troopCardPrefab;

        private ISettlement currentSettlement;
        private IAudioService audioService;
        private IWorldSimulationService worldSimulationService;

        private IAudioService AudioService
        {
            get
            {
                if (audioService == null && !ServiceLocator.TryResolve(out audioService))
                {
                    Debug.LogError("SettlementUIManager requires an IAudioService.");
                }

                return audioService;
            }
        }

        private Player CurrentPlayer => ResolveWorldSimulation()?.State.Player;

        private void Start()
        {
            ResolveWorldSimulation();

            if (recruitmentPanel != null)
            {
                recruitmentPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleEscapeKey();
            }
        }

        private void HandleEscapeKey()
        {
            if (recruitmentPanel != null && recruitmentPanel.activeSelf)
            {
                OnCloseRecruitment();
                return;
            }
            if (troopSelectionPanel != null && troopSelectionPanel.activeSelf)
            {
                troopSelectionPanel.SetActive(false);
                if (uiPanel != null) uiPanel.SetActive(true);
                return;
            }
            else if (uiPanel != null && uiPanel.activeSelf)
            {
                CloseUI();
            }
        }

        public void OpenSettlementUI(ISettlement settlement)
        {
            if (uiPanel == null || recruitButton == null || tradeButton == null || settlementNameText == null)
            {
                Debug.LogError("SettlementUIManager is missing UI references.");
                return;
            }

            currentSettlement = settlement;

            settlementNameText.text = GetSettlementName(settlement);
            uiPanel.SetActive(true);

            AudioService?.PlayUIMenuOpen();

            bool canTrade = GameFeatureFlags.TradingEnabled && !(settlement is Castle);
            if (recruitButton != null)
            {
                recruitButton.gameObject.SetActive(true);
                recruitButton.interactable = true;
            }
            if (tradeButton != null)
            {
                tradeButton.gameObject.SetActive(canTrade);
                tradeButton.interactable = canTrade;
            }
            if (exitButton != null)
            {
                exitButton.gameObject.SetActive(true);
                exitButton.interactable = true;
            }
            UpdateEconomyWidgets(settlement);
        }

        public void CloseUI()
        {
            if (uiPanel != null)
            {
                uiPanel.SetActive(false);
            }

            currentSettlement = null;
            AudioService?.PlayUIMenuClose();
        }

    public void OnRecruitButton()
    {
        // Play UI click sound
        AudioService?.PlayUIClick();

        Debug.Log($"Opening recruitment at {GetSettlementName(currentSettlement)}");

            if (uiPanel != null)
            {
                uiPanel.SetActive(false);
            }

        if (currentSettlement is Village village)
        {
            recruitmentPanel?.SetActive(true);
            LoadTroopCards(village.RecruitmentPool);  // Show only rank 1 and 2 troops
        }
        else if (currentSettlement is Town town)
        {
            recruitmentPanel?.SetActive(true);
            LoadTroopCards(town.AvailableUnits);  // Show all troops (no change)
        }
        else if (currentSettlement is Castle castle)
        {
            recruitmentPanel?.SetActive(true);
            LoadTroopCards(castle.AvailableUnits);  // Show all troops for castle like town and village
        }
        else
        {
            recruitmentPanel.SetActive(false); // No recruitment for other settlement types
        }
    }

    public void OnCloseRecruitment()
    {
            if (recruitmentPanel != null)
            {
                recruitmentPanel.SetActive(false);
            }

            if (uiPanel != null)
            {
                uiPanel.SetActive(true);
            }
        if (currentSettlement != null)
        {
            settlementNameText.text = GetSettlementName(currentSettlement);
            UpdateEconomyWidgets(currentSettlement);
        }                  // Return to settlement UI
    }

    public void OnTradeButton()
    {
        if (!GameFeatureFlags.TradingEnabled)
        {
            Debug.LogWarning("Trading is disabled.");
            return;
        }
        // Play UI click sound
        AudioService?.PlayUIClick();

        Debug.Log($"Opening trade at {GetSettlementName(currentSettlement)}");
        // TODO: Implement trade UI
    }

    public void OnExitButton()
    {
        AudioService?.PlayUIClick();
        CloseUI();
    }

    public void OnStatsButton()
    {
        // Play UI click sound
        AudioService?.PlayUIClick();

        // Open character stats UI for player
        var player = CurrentPlayer;
        if (CharacterStatsUI.Instance != null && player != null)
        {
            CharacterStatsUI.Instance.OpenStatsUI(player);
        }
    }

    public void OnInventoryButton()
    {
        // Play UI click sound
        AudioService?.PlayUIClick();

        // Open inventory UI for player
        var player = CurrentPlayer;
        if (InventoryUI.Instance != null && player != null)
        {
            InventoryUI.Instance.OpenInventory(player);
        }
    }

    public void OnWorldMapButton()
    {
        // Play UI click sound
        AudioService?.PlayUIClick();

        // Open world map UI
            if (WorldMapUI.Instance != null)
            {
                WorldMapUI.Instance.OpenWorldMap();
            }
    }

    private string GetSettlementName(ISettlement settlement)
    {
        if (settlement is Town town) return town.Name;
        if (settlement is Village village) return village.Name;
        if (settlement is Castle castle) return castle.Name;
        return "Unknown Settlement";
    }

    // Load the available units into the recruitment UI
    private void LoadTroopCards(List<Troop> availableUnits)
    {
            if (availableUnits == null || troopContainer == null || troopCardPrefab == null)
            {
                return;
            }

            foreach (Transform child in troopContainer)
            {
                Destroy(child.gameObject);
            }

        foreach (var troop in availableUnits)
        {
                GameObject troopCard = Instantiate(troopCardPrefab, troopContainer);
            TroopCard troopCardScript = troopCard.GetComponent<TroopCard>();

            if (troopCardScript == null)
            {
                Debug.LogError("TroopCard script not found on the prefab!");
                continue;
            }

            troopCardScript.SetUpTroopCard(troop);

            UnityEngine.UI.Button cardButton = troopCard.GetComponent<UnityEngine.UI.Button>();
            if (cardButton != null && troopSelectionPanelScript != null)
            {
                cardButton.onClick.AddListener(() =>
                {
                    AudioService?.PlayUIClick();
                    troopSelectionPanelScript.Open(troop);
                });
            }
            else
            {
                Debug.LogWarning("Missing Button or TroopSelectionPanel reference!");
            }
        }
    }

        private IWorldSimulationService ResolveWorldSimulation()
        {
            if (worldSimulationService == null && !ServiceLocator.TryResolve(out worldSimulationService))
            {
                Debug.LogError("SettlementUIManager requires an IWorldSimulationService.");
            }

            return worldSimulationService;
        }


    // Recruit the selected troop
        private void RecruitTroop(Troop selectedTroop)
        {
            Debug.Log($"Recruited {selectedTroop.Name}");
        }

        private void UpdateEconomyWidgets(ISettlement settlement)
        {
            if (economySummaryText != null)
            {
                economySummaryText.text = BuildEconomySummary(settlement);
            }

            if (factionNameText != null)
            {
                var faction = GetFactionForSettlement(settlement);
                factionNameText.text = faction != null ? $"Faction: {faction.Name}" : "Faction: None";
            }

            if (factionNeedText != null)
            {
                var faction = GetFactionForSettlement(settlement);
                factionNeedText.text = faction?.Economy?.BuildStatusLine() ?? "No faction data";
            }

            if (productionDetailsText != null)
            {
                productionDetailsText.text = BuildProductionDetails(settlement);
            }
        }

        private string BuildEconomySummary(ISettlement settlement)
        {
            if (settlement == null)
            {
                return "No settlement selected";
            }

            switch (settlement)
            {
                case Village village:
                    return $"Level {village.VillageLevel} village\nStock: {FormatBasicLedger(village.ResourceLedger)}";
                case Town town:
                    return $"Treasury {town.Treasury} | Income {town.Income}\nRaw: {FormatBasicLedger(town.ResourceLedger)}\nRefined: {FormatAdvancedLedger(town.ResourceLedger)}";
                case Castle castle:
                    return $"Garrison {castle.CurrentGarrisonSize}/{castle.MaxGarrisonSize}\nArmory: {FormatArmory(castle.Armory)}";
                default:
                    return "No economy data";
            }
        }

        private string BuildProductionDetails(ISettlement settlement)
        {
            if (settlement == null)
            {
                return "No production data";
            }

            var sb = new StringBuilder();
            switch (settlement)
            {
                case Village village:
                    sb.AppendLine($"Produces ({village.VillageLevel} slots):");
                    foreach (var type in village.ProductionTypes)
                    {
                        int perDay = village.GetDailyProduction(type);
                        int stock = village.ResourceLedger.GetBasic(type);
                        sb.AppendLine($"• {type}: {perDay}/day (stock {stock})");
                    }
                    break;
                case Town town:
                    sb.AppendLine("Refinement Chains:");
                    foreach (var recipe in Town.RefinementRecipes)
                    {
                        int stockA = town.ResourceLedger.GetBasic(recipe.InputA);
                        int stockB = town.ResourceLedger.GetBasic(recipe.InputB);
                        int refined = town.ResourceLedger.GetAdvanced(recipe.Output);
                        sb.AppendLine($"• {recipe.Output}: {recipe.InputA} x{recipe.CostA} + {recipe.InputB} x{recipe.CostB} → 1 | stock {refined} (inputs {stockA}/{stockB})");
                    }
                    break;
                case Castle castle:
                    sb.AppendLine("Equipment Forging:");
                    foreach (var recipe in Castle.EquipmentRecipes)
                    {
                        int advStock = castle.ResourceLedger.GetAdvanced(recipe.RequiredAdvanced);
                        int basicStock = castle.ResourceLedger.GetBasic(recipe.RequiredBasic);
                        sb.AppendLine($"• {recipe.WeaponType}: {recipe.RequiredAdvanced} x{recipe.CostAdvanced} + {recipe.RequiredBasic} x{recipe.CostBasic} → {recipe.OutputCount} | advanced {advStock}, basic {basicStock}");
                    }
                    sb.AppendLine($"Supplies: {castle.Supplies}");
                    break;
                default:
                    sb.Append("Production details unavailable.");
                    break;
            }

            return sb.ToString();
        }

        private static string FormatBasicLedger(SettlementResourceLedger ledger, int maxEntries = 3)
        {
            if (ledger?.BasicResources == null || ledger.BasicResources.Count == 0)
            {
                return "None";
            }

            return string.Join(", ",
                ledger.BasicResources
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(maxEntries)
                    .Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        }

        private static string FormatAdvancedLedger(SettlementResourceLedger ledger, int maxEntries = 3)
        {
            if (ledger?.AdvancedResources == null || ledger.AdvancedResources.Count == 0)
            {
                return "None";
            }

            return string.Join(", ",
                ledger.AdvancedResources
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(maxEntries)
                    .Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        }

        private static string FormatArmory(IDictionary<WeaponTypes, int> armory, int maxEntries = 3)
        {
            if (armory == null || armory.Count == 0)
            {
                return "Empty";
            }

            return string.Join(", ",
                armory
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(maxEntries)
                    .Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        }

        private Faction GetFactionForSettlement(ISettlement settlement)
        {
            var repo = ResolveWorldSimulation()?.State;
            if (repo == null || settlement == null)
            {
                return null;
            }

            int factionId = settlement.FactionId;
            if (factionId < 0 || factionId >= repo.Factions.Count)
            {
                return null;
            }

            return repo.Factions[factionId];
        }
    }
}
