using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using LivingWorldRPG.Core;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("UI Panels")]
    public GameObject inventoryPanel;
    public Button closeButton;

    [Header("Inventory Display")]
    public Transform itemContainer;
    public GameObject itemSlotPrefab;

    [Header("Item Details")]
    public GameObject itemDetailsPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public Button useButton;
    public Button equipButton;
    public Button dropButton;

    private Characters currentCharacter;
    private ItemData selectedItem;
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
            closeButton.onClick.AddListener(CloseInventory);
        }

        if (useButton != null)
        {
            useButton.onClick.AddListener(UseItem);
        }

        if (equipButton != null)
        {
            equipButton.onClick.AddListener(EquipItem);
        }

        if (dropButton != null)
        {
            dropButton.onClick.AddListener(DropItem);
        }

        inventoryPanel.SetActive(false);
        itemDetailsPanel.SetActive(false);

        if (!GameFeatureFlags.ItemsEnabled)
        {
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            if (itemDetailsPanel != null) itemDetailsPanel.SetActive(false);
            gameObject.SetActive(false);
            return;
        }
    }

    public void OpenInventory(Characters character)
    {
        if (!GameFeatureFlags.ItemsEnabled)
        {
            Debug.LogWarning("Inventory is disabled for launch.");
            return;
        }
        currentCharacter = character;
        UpdateInventoryUI();

        inventoryPanel.SetActive(true);

        AudioService?.PlayUIMenuOpen();
    }

    public void CloseInventory()
    {
        if (!GameFeatureFlags.ItemsEnabled)
        {
            return;
        }
        inventoryPanel.SetActive(false);
        itemDetailsPanel.SetActive(false);
        currentCharacter = null;
        selectedItem = null;

        AudioService?.PlayUIMenuClose();
    }

    private void UpdateInventoryUI()
    {
        if (!GameFeatureFlags.ItemsEnabled || currentCharacter == null)
        {
            return;
        }

        // Clear existing item slots
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        // Populate inventory
        foreach (var item in currentCharacter.Inventory.items)
        {
            GameObject itemSlot = Instantiate(itemSlotPrefab, itemContainer);
            TextMeshProUGUI itemText = itemSlot.GetComponentInChildren<TextMeshProUGUI>();
            if (itemText != null)
            {
                itemText.text = item.name;
            }

            // Add click handler
            Button slotButton = itemSlot.GetComponent<Button>();
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(() => SelectItem(item));
            }
        }
    }

    private void SelectItem(ItemData item)
    {
        if (!GameFeatureFlags.ItemsEnabled)
        {
            return;
        }
        selectedItem = item;
        itemNameText.text = item.name;
        itemDescriptionText.text = GetItemDescription(item);

        useButton.gameObject.SetActive(item.type == ItemType.Consumable);
        equipButton.gameObject.SetActive(item.type == ItemType.Weapon || item.type == ItemType.Armor);

        itemDetailsPanel.SetActive(true);
    }

    private string GetItemDescription(ItemData item)
    {
        string description = $"{item.name}\nType: {item.type}";

        if (item.attackBonus > 0)
            description += $"\nAttack Bonus: +{item.attackBonus}";
        if (item.defenseBonus > 0)
            description += $"\nDefense Bonus: +{item.defenseBonus}";
        if (item.healthBonus > 0)
            description += $"\nHealth Bonus: +{item.healthBonus}";

        return description;
    }

    private void UseItem()
    {
        if (!GameFeatureFlags.ItemsEnabled)
        {
            return;
        }
        if (selectedItem != null && selectedItem.type == ItemType.Consumable)
        {
            currentCharacter.UseItem(selectedItem);
            currentCharacter.Inventory.RemoveItem(selectedItem);
            UpdateInventoryUI();
            itemDetailsPanel.SetActive(false);
        }
    }

    private void EquipItem()
    {
        if (!GameFeatureFlags.ItemsEnabled)
        {
            return;
        }
        if (selectedItem != null && (selectedItem.type == ItemType.Weapon || selectedItem.type == ItemType.Armor))
        {
            EquipmentSlot slot = selectedItem.type == ItemType.Weapon ? EquipmentSlot.Weapon : EquipmentSlot.Body;
            currentCharacter.EquipItem(slot, selectedItem);
            UpdateInventoryUI();
            itemDetailsPanel.SetActive(false);
        }
    }

    private void DropItem()
    {
        if (!GameFeatureFlags.ItemsEnabled)
        {
            return;
        }
        if (selectedItem != null)
        {
            currentCharacter.Inventory.RemoveItem(selectedItem);
            UpdateInventoryUI();
            itemDetailsPanel.SetActive(false);
        }
    }
}
