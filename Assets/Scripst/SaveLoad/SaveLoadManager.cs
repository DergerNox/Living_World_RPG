using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    private static SaveLoadManager instance;
    private static string _saveFilePath;

    public static string SaveFilePath
    {
        get
        {
            if (_saveFilePath == null)
            {
                _saveFilePath = Application.persistentDataPath + "/savegame.json";
            }
            return _saveFilePath;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void SaveGame(GameData data)
    {
        instance.SaveGameImpl(data);
    }

    private void SaveGameImpl(GameData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SaveFilePath, json);
        Debug.Log("Game saved to " + SaveFilePath);
    }

    public static GameData LoadGame()
    {
        return instance.LoadGameImpl();
    }

    private GameData LoadGameImpl()
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.LogWarning("Save file not found at " + SaveFilePath);
            return null;
        }

        string json = File.ReadAllText(SaveFilePath);
        GameData data = JsonUtility.FromJson<GameData>(json);
        Debug.Log("Game loaded from " + SaveFilePath);
        return data;
    }
}

[System.Serializable]
public class GameData
{
    public string playerName;
    public int playerHealth;
    public int playerMaxHealth;
    public int playerMana;
    public int playerMaxMana;
    public int playerGold;
    public int playerLevel;
    public int playerExperience;
    public Inventory inventory;
    public Equipment equipment;
    public int playerAttack;
    public int playerDefense;
    public Vector3 playerPosition; // Added for saving/loading player position
    // Add more fields as needed for full game state

    public GameData()
    {
        // Initialize default values if needed
    }
}
