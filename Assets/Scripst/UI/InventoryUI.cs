using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
    private Item selectedItem;

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
    }

    public void OpenInventory(Characters character)
    {
        currentCharacter = character;
        UpdateInventoryUI();

        inventoryPanel.SetActive(true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIMenuOpen();
        }
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        itemDetailsPanel.SetActive(false);
        currentCharacter = null;
        selectedItem = null;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIMenuClose();
        }
    }

    private void UpdateInventoryUI()
    {
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

    private void SelectItem(Item item)
    {
        selectedItem = item;
        itemNameText.text = item.name;
        itemDescriptionText.text = GetItemDescription(item);

        useButton.gameObject.SetActive(item.type == ItemType.Consumable);
        equipButton.gameObject.SetActive(item.type == ItemType.Weapon || item.type == ItemType.Armor);

        itemDetailsPanel.SetActive(true);
    }

    private string GetItemDescription(Item item)
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
        if (selectedItem != null && (selectedItem.type == ItemType.Weapon || selectedItem.type == ItemType.Armor))
        {
            currentCharacter.EquipItem(selectedItem);
            UpdateInventoryUI();
            itemDetailsPanel.SetActive(false);
        }
    }

    private void DropItem()
    {
        if (selectedItem != null)
        {
            currentCharacter.Inventory.RemoveItem(selectedItem);
            UpdateInventoryUI();
            itemDetailsPanel.SetActive(false);
        }
    }
}
