using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleUI : MonoBehaviour
{
    public static BattleUI Instance;

    [Header("UI Panels")]
    public GameObject battlePanel;
    public GameObject troopSelectionPanel;
    public GameObject skillSelectionPanel;

    [Header("Battle Info")]
    public TextMeshProUGUI battleStatusText;
    public TextMeshProUGUI attackerTroopText;
    public TextMeshProUGUI defenderTroopText;

    [Header("Buttons")]
    public Button attackButton;
    public Button useSkillButton;
    public Button retreatButton;
    public Button endTurnButton;

    [Header("Troop Lists")]
    public Transform attackerTroopContainer;
    public Transform defenderTroopContainer;
    public GameObject troopCardPrefab;

    private IList<Party> currentAttackers;
    private IList<Party> currentDefenders;
    private TerrainType currentTerrain = TerrainType.Plain;
    private Troop selectedAttackerTroop;
    private Troop selectedDefenderTroop;
    private bool isPlayerTurn = true;
    private IBattleResolver battleResolver;

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

        if (!ServiceLocator.TryResolve(out battleResolver))
        {
            battleResolver = new Battle();
        }
    }

    private void Start()
    {
        if (attackButton != null)
            attackButton.onClick.AddListener(OnAttackButtonClicked);
        if (useSkillButton != null)
            useSkillButton.onClick.AddListener(OnUseSkillButtonClicked);
        if (retreatButton != null)
            retreatButton.onClick.AddListener(OnRetreatButtonClicked);
        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);

        HideBattleUI();
    }

    public void StartBattle(IList<Party> attackers, IList<Party> defenders, TerrainType terrain)
    {
        currentAttackers = attackers;
        currentDefenders = defenders;
        currentTerrain = terrain;
        battlePanel.SetActive(true);
        UpdateBattleUI();
        battleStatusText.text = "Battle Started!";
    }

    private void UpdateBattleUI()
    {
        // Clear existing troop cards
        foreach (Transform child in attackerTroopContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in defenderTroopContainer)
        {
            Destroy(child.gameObject);
        }

        // Populate attacker troops
        if (currentAttackers == null || currentDefenders == null)
        {
            return;
        }

        foreach (var party in currentAttackers)
        {
            foreach (var troop in party.Troops)
            {
                if (troop.IsAlive)
                {
                    GameObject troopCard = Instantiate(troopCardPrefab, attackerTroopContainer);
                    TroopCard cardScript = troopCard.GetComponent<TroopCard>();
                    if (cardScript != null)
                    {
                        cardScript.SetUpTroopCard(troop);
                        Button cardButton = troopCard.GetComponent<Button>();
                        if (cardButton != null)
                        {
                            cardButton.onClick.AddListener(() => SelectAttackerTroop(troop));
                        }
                    }
                }
            }
        }

        // Populate defender troops
        foreach (var party in currentDefenders)
        {
            foreach (var troop in party.Troops)
            {
                if (troop.IsAlive)
                {
                    GameObject troopCard = Instantiate(troopCardPrefab, defenderTroopContainer);
                    TroopCard cardScript = troopCard.GetComponent<TroopCard>();
                    if (cardScript != null)
                    {
                        cardScript.SetUpTroopCard(troop);
                        Button cardButton = troopCard.GetComponent<Button>();
                        if (cardButton != null)
                        {
                            cardButton.onClick.AddListener(() => SelectDefenderTroop(troop));
                        }
                    }
                }
            }
        }
    }

    private void SelectAttackerTroop(Troop troop)
    {
        selectedAttackerTroop = troop;
        attackerTroopText.text = $"Selected: {troop.Name}";
    }

    private void SelectDefenderTroop(Troop troop)
    {
        selectedDefenderTroop = troop;
        defenderTroopText.text = $"Target: {troop.Name}";
    }

    private void OnAttackButtonClicked()
    {
        if (selectedAttackerTroop != null && selectedDefenderTroop != null && isPlayerTurn)
        {
            bool attackerWins = battleResolver != null
                ? battleResolver.ResolveDuel(selectedAttackerTroop, selectedDefenderTroop, currentTerrain)
                : new Battle().BattleIndividual(selectedAttackerTroop, selectedDefenderTroop);
            battleStatusText.text = attackerWins ? "Attacker wins the duel!" : "Defender wins the duel!";
            UpdateBattleUI();
            CheckBattleEnd();
            isPlayerTurn = false;
        }
    }

    private void OnUseSkillButtonClicked()
    {
        if (selectedAttackerTroop != null && isPlayerTurn)
        {
            // Open skill selection panel
            skillSelectionPanel.SetActive(true);
        }
    }

    private void OnRetreatButtonClicked()
    {
        // Implement retreat logic
        battleStatusText.text = "Retreating from battle!";
        EndBattle();
    }

    private void OnEndTurnButtonClicked()
    {
        isPlayerTurn = !isPlayerTurn;
        battleStatusText.text = isPlayerTurn ? "Your turn!" : "Enemy turn!";
        if (!isPlayerTurn)
        {
            // AI turn logic would go here
            // For now, just end the turn
            isPlayerTurn = true;
            battleStatusText.text = "Your turn!";
        }
    }

    private void CheckBattleEnd()
    {
        bool attackersAlive = false;
        bool defendersAlive = false;

        if (currentAttackers == null || currentDefenders == null)
        {
            return;
        }

        foreach (var party in currentAttackers)
        {
            if (party.Troops.Count > 0)
            {
                attackersAlive = true;
                break;
            }
        }

        foreach (var party in currentDefenders)
        {
            if (party.Troops.Count > 0)
            {
                defendersAlive = true;
                break;
            }
        }

        if (!attackersAlive || !defendersAlive)
        {
            string result = attackersAlive ? "Attackers won!" : "Defenders won!";
            battleStatusText.text = result;
            Debug.Log(result);

            // Award experience if attackers won
            if (attackersAlive)
            {
                foreach (var party in currentAttackers)
                {
                    foreach (var troop in party.Troops)
                    {
                        troop.GainExperience(50);
                    }
                }
            }

            EndBattle();
        }
    }

    private void EndBattle()
    {
        battlePanel.SetActive(false);
        selectedAttackerTroop = null;
        selectedDefenderTroop = null;
        currentAttackers = null;
        currentDefenders = null;
    }

    private void HideBattleUI()
    {
        battlePanel.SetActive(false);
        troopSelectionPanel.SetActive(false);
        skillSelectionPanel.SetActive(false);
    }
}
