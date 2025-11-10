using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterStatsUI : MonoBehaviour
{
    public static CharacterStatsUI Instance;

    [Header("UI Panels")]
    public GameObject statsPanel;
    public Button closeButton;

    [Header("Character Info")]
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI experienceText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI agilityText;
    public TextMeshProUGUI intelligenceText;
    public TextMeshProUGUI defenseText;

    [Header("Inventory")]
    public Transform inventoryContainer;
    public GameObject itemSlotPrefab;

    private Characters currentCharacter;
    private IAudioService audioService;

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
    }

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseStatsUI);
        }

        statsPanel.SetActive(false);
    }

    public void OpenStatsUI(Characters character)
    {
        currentCharacter = character;
        UpdateUI();

        statsPanel.SetActive(true);

        AudioService?.PlayUIMenuOpen();
    }

    public void CloseStatsUI()
    {
        statsPanel.SetActive(false);
        currentCharacter = null;

        AudioService?.PlayUIMenuClose();
    }

    private void UpdateUI()
    {
        if (currentCharacter == null) return;

        characterNameText.text = currentCharacter.Name;
        levelText.text = $"Level: {currentCharacter.Level}";
        experienceText.text = $"Experience: {currentCharacter.Experience}";
        healthText.text = $"Health: {currentCharacter.Health}/{currentCharacter.MaxHealth}";
        manaText.text = $"Mana: {currentCharacter.Mana}/{currentCharacter.MaxMana}";
        strengthText.text = $"Strength: {currentCharacter.Strength}";
        agilityText.text = $"Agility: {currentCharacter.Agility}";
        intelligenceText.text = $"Intelligence: {currentCharacter.Intelligence}";
        defenseText.text = $"Defense: {currentCharacter.Defense}";

        UpdateInventoryUI();
    }

    private void UpdateInventoryUI()
    {
        // Clear existing item slots
        foreach (Transform child in inventoryContainer)
        {
            Destroy(child.gameObject);
        }

        // Populate inventory
        foreach (var item in currentCharacter.Inventory.items)
        {
            GameObject itemSlot = Instantiate(itemSlotPrefab, inventoryContainer);
            TextMeshProUGUI itemText = itemSlot.GetComponentInChildren<TextMeshProUGUI>();
            if (itemText != null)
            {
                itemText.text = item.name;
            }
        }
    }
}
