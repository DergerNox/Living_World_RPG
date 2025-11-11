using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using LivingWorldRPG.World;

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
        [SerializeField] private Button recruitButton;
        [SerializeField] private Button tradeButton;

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

            recruitButton.gameObject.SetActive(true);
            tradeButton.gameObject.SetActive(!(settlement is Castle));
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
        }                  // Return to settlement UI
    }

    public void OnTradeButton()
    {
        // Play UI click sound
        AudioService?.PlayUIClick();

        Debug.Log($"Opening trade at {GetSettlementName(currentSettlement)}");
        // TODO: Implement trade UI
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
    }
}
