using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using LivingWorldRPG.PartySystem;

namespace LivingWorldRPG.UI
{
    public class PartyUI : MonoBehaviour
    {
        public static PartyUI Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject partyPanel;
        [SerializeField] private GameObject memberPanel;
        [SerializeField] private GameObject troopPanel;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI partyNameText;
        [SerializeField] private TextMeshProUGUI partySizeText;
        [SerializeField] private TextMeshProUGUI moraleText;
        [SerializeField] private TextMeshProUGUI fatigueText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button restButton;
        [SerializeField] private Button mergeButton;
        [SerializeField] private Button splitButton;

        [Header("Lists")]
        [SerializeField] private Transform memberListContainer;
        [SerializeField] private Transform troopListContainer;
        [SerializeField] private GameObject memberItemPrefab;
        [SerializeField] private GameObject troopItemPrefab;

        private Party currentParty;
        private IAudioService audioService;
        private IPartyService partyService;

    private IAudioService AudioService
    {
        get
        {
            if (audioService == null)
            {
                if (!ServiceLocator.TryResolve(out audioService))
                {
                    audioService = AudioManager.Instance;
                }
            }

            return audioService;
        }
    }

        private IPartyService PartyService
        {
            get
            {
                if (partyService == null)
                {
                    if (!ServiceLocator.TryResolve(out partyService))
                    {
                        partyService = PartyManager.Instance;
                    }
                }

                return partyService;
            }
        }

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

        _ = AudioService;
        _ = PartyService;
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

            partyPanel?.SetActive(false);
        }

        public void OpenPartyUI(Party party)
        {
        currentParty = party;
        UpdateUI();
        partyPanel.SetActive(true);

        AudioService?.PlayUIMenuOpen();
    }

        public void ClosePartyUI()
        {
            partyPanel?.SetActive(false);
            currentParty = null;

            AudioService?.PlayUIMenuClose();
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
            foreach (global::Characters member in currentParty.Members)
            {
                GameObject item = Instantiate(memberItemPrefab, memberListContainer);
                TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = member.Name + " (Lv." + member.Level + ")";
                }
            }

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
            var service = PartyService;
            if (service == null || currentParty == null) return;

            List<Party> partiesAtPos = service.GetPartiesAtPosition(currentParty.Position);
            if (partiesAtPos.Count > 1)
            {
                Party otherParty = partiesAtPos.Find(p => p != currentParty);
                Party merged = currentParty.MergeWith(otherParty);
                service.RemoveParty(currentParty);
                service.RemoveParty(otherParty);
                service.AddParty(merged);
                currentParty = merged;
                UpdateUI();
            }
        }

        private void SplitParty()
        {
            if (currentParty != null && currentParty.Size > 2)
            {
                var service = PartyService;
                if (service == null) return;

                List<Party> splitParties = currentParty.Split(currentParty.Size / 2);
                service.RemoveParty(currentParty);
                foreach (Party p in splitParties)
                {
                    service.AddParty(p);
                }
                currentParty = splitParties[0];
                UpdateUI();
            }
        }
    }
}
