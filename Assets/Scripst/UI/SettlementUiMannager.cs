using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class SettlementUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject uiPanel;             // Settlement info panel
    public GameObject recruitmentPanel;    // Recruitment panel
    public GameObject TroopSelectionPanel; // Troop selection panel
    [Header("Scripts")]
    public TroopSelectionPanel troopSelectionPanelScript; // Drag the script component in Inspector

    [Header("UI Elements")]
    public TextMeshProUGUI settlementNameText;
    public UnityEngine.UI.Button recruitButton;
    public UnityEngine.UI.Button tradeButton;

    public Transform troopContainer;        // Parent object where troop cards will be displayed
    public GameObject troopCardPrefab;      // Prefab for displaying each troop card

    private ISettlement currentSettlement;

    void Start()
    {
        if (recruitmentPanel != null)
            recruitmentPanel.SetActive(false);  // Ensure recruitment panel is hidden on start
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }

    private void HandleEscapeKey()
    {
        if (recruitmentPanel.activeSelf)
        {
            OnCloseRecruitment(); // Close recruitment panel and return to settlement UI
        }
        else if (uiPanel.activeSelf)
        {
            CloseUI(); // Close settlement UI entirely
        }
    }

    public void OpenSettlementUI(ISettlement settlement)
    {
        currentSettlement = settlement;

        settlementNameText.text = GetSettlementName(settlement);
        uiPanel.SetActive(true);

        // Play UI menu open sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIMenuOpen();
        }

        // Show recruit button for all settlements
        recruitButton.gameObject.SetActive(true);

        // Hide trade button only for castles
        if (settlement is Castle)
        {
            tradeButton.gameObject.SetActive(false);
        }
        else
        {
            tradeButton.gameObject.SetActive(true);
        }
    }

    public void CloseUI()
    {
        uiPanel.SetActive(false);
        currentSettlement = null;

        // Play UI menu close sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIMenuClose();
        }
    }

    public void OnRecruitButton()
    {
        // Play UI click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }

        Debug.Log($"Opening recruitment at {GetSettlementName(currentSettlement)}");

        uiPanel.SetActive(false);                 // Hide the settlement UI

        if (currentSettlement is Village village)
        {
            recruitmentPanel.SetActive(true);
            LoadTroopCards(village.RecruitmentPool);  // Show only rank 1 and 2 troops
        }
        else if (currentSettlement is Town town)
        {
            recruitmentPanel.SetActive(true);
            LoadTroopCards(town.AvailableUnits);  // Show all troops (no change)
        }
        else if (currentSettlement is Castle castle)
        {
            recruitmentPanel.SetActive(true);
            LoadTroopCards(castle.AvailableUnits);  // Show all troops for castle like town and village
        }
        else
        {
            recruitmentPanel.SetActive(false); // No recruitment for other settlement types
        }
    }

    public void OnCloseRecruitment()
    {
        recruitmentPanel.SetActive(false);        // Hide recruitment panel
        uiPanel.SetActive(true);
        if (currentSettlement != null)
        {
            settlementNameText.text = GetSettlementName(currentSettlement);
        }                  // Return to settlement UI
    }

    public void OnTradeButton()
    {
        // Play UI click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }

        Debug.Log($"Opening trade at {GetSettlementName(currentSettlement)}");
        // TODO: Open trade UI
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
    // Clear existing troop cards
    foreach (Transform child in troopContainer)
    {
        Destroy(child.gameObject);
    }

    // Instantiate and set up troop cards
    foreach (var troop in availableUnits)
    {
        GameObject troopCard = Instantiate(troopCardPrefab, troopContainer);
        TroopCard troopCardScript = troopCard.GetComponent<TroopCard>();

        if (troopCardScript == null)
        {
            Debug.LogError("TroopCard script not found on the prefab!");
            continue;
        }

        // Set the name and image on the card
        troopCardScript.SetUpTroopCard(troop);

        // Add listener to open the Troop Selection Panel when clicked
        UnityEngine.UI.Button cardButton = troopCard.GetComponent<UnityEngine.UI.Button>();
        if (cardButton != null && troopSelectionPanelScript != null)
        {
            cardButton.onClick.AddListener(() =>
            {
                // Play UI click sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayUIClick();
                }
                troopSelectionPanelScript.Open(troop);
            });
        }
        else
        {
            Debug.LogWarning("Missing Button or TroopSelectionPanel reference!");
        }
    }
}


    // Recruit the selected troop
    private void RecruitTroop(Troop selectedTroop)
    {
        // Handle recruitment logic here
        Debug.Log($"Recruited {selectedTroop.Name}");
        // You could also instantiate troops into the player's party, handle cost deduction, etc.
    }
}
