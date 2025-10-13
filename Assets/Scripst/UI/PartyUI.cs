using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PartyUI : MonoBehaviour
{
    public static PartyUI Instance;

    [Header("UI Panels")]
    public GameObject partyPanel;
    public GameObject memberPanel;
    public GameObject troopPanel;

    [Header("UI Elements")]
    public TextMeshProUGUI partyNameText;
    public TextMeshProUGUI partySizeText;
    public TextMeshProUGUI moraleText;
    public TextMeshProUGUI fatigueText;
    public TextMeshProUGUI goldText;
    public Button closeButton;
    public Button restButton;
    public Button mergeButton;
    public Button splitButton;

    [Header("Lists")]
    public Transform memberListContainer;
    public Transform troopListContainer;
    public GameObject memberItemPrefab;
    public GameObject troopItemPrefab;

    private Party currentParty;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePartyUI);
        }
        if (restButton != null)
        {
            restButton.onClick.AddListener(RestParty);
        }
        if (mergeButton != null)
        {
            mergeButton.onClick.AddListener(MergeParties);
        }
        if (splitButton != null)
        {
            splitButton.onClick.AddListener(SplitParty);
        }

        partyPanel.SetActive(false);
    }

    public void OpenPartyUI(Party party)
    {
        currentParty = party;
        UpdateUI();
        partyPanel.SetActive(true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIMenuOpen();
        }
    }

    public void ClosePartyUI()
    {
        partyPanel.SetActive(false);
        currentParty = null;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIMenuClose();
        }
    }

    private void UpdateUI()
    {
        if (currentParty == null) return;

        partyNameText.text = currentParty.Name;
        partySizeText.text = "Size: " + currentParty.Size;
        moraleText.text = "Morale: " + currentParty.Morale;
        fatigueText.text = "Fatigue: " + currentParty.Fatigue;
        goldText.text = "Gold: " + currentParty.Gold;

        // Clear and populate member list
        foreach (Transform child in memberListContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Characters member in currentParty.Members)
        {
            GameObject item = Instantiate(memberItemPrefab, memberListContainer);
            TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = member.Name + " (Lv." + member.Level + ")";
            }
        }

        // Clear and populate troop list
        foreach (Transform child in troopListContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Troop troop in currentParty.Troops)
        {
            GameObject item = Instantiate(troopItemPrefab, troopListContainer);
            TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = troop.Name + " (Lv." + troop.Level + ")";
            }
        }
    }

    private void RestParty()
    {
        if (currentParty != null)
        {
            currentParty.Rest();
            UpdateUI();
        }
    }

    private void MergeParties()
    {
        // Simple merge with first available party at same position
        List<Party> partiesAtPos = PartyManager.Instance.GetPartiesAtPosition(currentParty.Position);
        if (partiesAtPos.Count > 1)
        {
            Party otherParty = partiesAtPos.Find(p => p != currentParty);
            Party merged = currentParty.MergeWith(otherParty);
            PartyManager.Instance.RemoveParty(currentParty);
            PartyManager.Instance.RemoveParty(otherParty);
            PartyManager.Instance.AddParty(merged);
            currentParty = merged;
            UpdateUI();
        }
    }

    private void SplitParty()
    {
        if (currentParty.Size > 2)
        {
            List<Party> splitParties = currentParty.Split(currentParty.Size / 2);
            PartyManager.Instance.RemoveParty(currentParty);
            foreach (Party p in splitParties)
            {
                PartyManager.Instance.AddParty(p);
            }
            currentParty = splitParties[0]; // Focus on first split party
            UpdateUI();
        }
    }
}
