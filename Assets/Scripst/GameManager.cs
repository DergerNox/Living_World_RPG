using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject saveLoadPanel;
    public Transform playerTransform;

    void Awake()
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
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGame();
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadGame();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            LogInventory();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (saveLoadPanel != null)
            {
                saveLoadPanel.SetActive(!saveLoadPanel.activeSelf);
            }
        }
    }

    public void SaveGame()
    {
        if (WorldHandler.Player == null)
        {
            Debug.LogError("Player is not set in WorldHandler. Cannot save game.");
            return;
        }

        GameData data = new GameData();
        data.playerName = WorldHandler.Player.Name;
        data.playerHealth = WorldHandler.Player.Health;
        data.playerMaxHealth = WorldHandler.Player.MaxHealth;
        data.playerMana = WorldHandler.Player.Mana;
        data.playerMaxMana = WorldHandler.Player.MaxMana;
        data.playerGold = WorldHandler.Player.Gold;
        data.playerLevel = WorldHandler.Player.Level;
        data.playerExperience = WorldHandler.Player.Experience;
        data.inventory = WorldHandler.Player.Inventory;
        data.equipment = WorldHandler.Player.Equipment;
        data.playerAttack = WorldHandler.Player.Attack;
        data.playerDefense = WorldHandler.Player.Defense;
        // Save player position using assigned Transform or find by tag
        if (playerTransform != null)
        {
            data.playerPosition = playerTransform.position;
        }
        else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                data.playerPosition = playerObj.transform.position;
            }
            else
            {
                Debug.LogWarning("Player Transform not assigned and GameObject not found with tag 'Player'. Position not saved.");
            }
        }

        SaveLoadManager.SaveGame(data);
    }

    public void LoadGame()
    {
        GameData data = SaveLoadManager.LoadGame();
        if (data != null)
        {
            if (WorldHandler.Player == null)
            {
                Debug.LogError("Player is not set in WorldHandler. Cannot load game.");
                return;
            }

            WorldHandler.Player.Name = data.playerName;
            WorldHandler.Player.Health = data.playerHealth;
            WorldHandler.Player.MaxHealth = data.playerMaxHealth;
            WorldHandler.Player.Mana = data.playerMana;
            WorldHandler.Player.MaxMana = data.playerMaxMana;
            WorldHandler.Player.Gold = data.playerGold;
            WorldHandler.Player.Level = data.playerLevel;
            WorldHandler.Player.Experience = data.playerExperience;
            WorldHandler.Player.Inventory = data.inventory;
            WorldHandler.Player.Equipment = data.equipment;
            WorldHandler.Player.Attack = data.playerAttack;
            WorldHandler.Player.Defense = data.playerDefense;

            // Restore player position using assigned Transform or find by tag
            if (playerTransform != null)
            {
                playerTransform.position = data.playerPosition;
                Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero; // Reset velocity for smooth movement
                }
            }
            else
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerObj.transform.position = data.playerPosition;
                    Rigidbody rb = playerObj.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero; // Reset velocity for smooth movement
                    }
                }
                else
                {
                    Debug.LogWarning("Player Transform not assigned and GameObject not found with tag 'Player'. Position not restored.");
                }
            }
        }
    }

    void LogInventory()
    {
        if (WorldHandler.Player == null)
        {
            Debug.LogError("Player is not set in WorldHandler. Cannot log inventory.");
            return;
        }

        Debug.Log("Inventory:");
        foreach (var item in WorldHandler.Player.Inventory.items)
        {
            Debug.Log(item.name);
        }
        Debug.Log("Equipment:");
        foreach (var eq in WorldHandler.Player.Equipment.equippedItems)
        {
            Debug.Log(eq.slot + ": " + eq.item.name);
        }
    }
}
