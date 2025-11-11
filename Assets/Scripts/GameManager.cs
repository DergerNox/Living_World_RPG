using UnityEngine;
using LivingWorldRPG.World;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameObject saveLoadPanel;
    [SerializeField] private Transform playerTransform;

    private IWorldSimulationService worldSimulation;

    void Awake()
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

        ServiceLocator.TryResolve(out worldSimulation);
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
        var player = GetPlayer();
        if (player == null)
        {
            Debug.LogError("Player is not available. Cannot save game.");
            return;
        }

        GameData data = new GameData
        {
            playerName = player.Name,
            playerHealth = player.Health,
            playerMaxHealth = player.MaxHealth,
            playerMana = player.Mana,
            playerMaxMana = player.MaxMana,
            playerGold = player.Gold,
            playerLevel = player.Level,
            playerExperience = player.Experience,
            inventory = player.Inventory,
            equipment = player.Equipment,
            playerAttack = player.Attack,
            playerDefense = player.Defense
        };
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
            var player = GetPlayer();
            if (player == null)
            {
                Debug.LogError("Player is not available. Cannot load game.");
                return;
            }

            player.Name = data.playerName;
            player.Health = data.playerHealth;
            player.MaxHealth = data.playerMaxHealth;
            player.Mana = data.playerMana;
            player.MaxMana = data.playerMaxMana;
            player.Gold = data.playerGold;
            player.Level = data.playerLevel;
            player.Experience = data.playerExperience;
            player.Inventory = data.inventory;
            player.Equipment = data.equipment;
            player.Attack = data.playerAttack;
            player.Defense = data.playerDefense;

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
        var player = GetPlayer();
        if (player == null)
        {
            Debug.LogError("Player is not available. Cannot log inventory.");
            return;
        }

        Debug.Log("Inventory:");
        foreach (var item in player.Inventory.items)
        {
            Debug.Log(item.name);
        }
        Debug.Log("Equipment:");
        foreach (var eq in player.Equipment.equippedItems)
        {
            Debug.Log(eq.slot + ": " + eq.item.name);
        }
    }

    private Player GetPlayer()
    {
        if (worldSimulation == null && !ServiceLocator.TryResolve(out worldSimulation))
        {
            Debug.LogError("GameManager requires an IWorldSimulationService. Save/Load operations will be disabled.");
            return null;
        }
        return worldSimulation.State.Player;
    }
}
