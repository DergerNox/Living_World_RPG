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
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public static void SaveGame(GameData data)
    {
        EnsureInstance();
        instance.SaveGameImpl(data);
    }

    public static bool HasSaveData()
    {
        return File.Exists(SaveFilePath);
    }

    private void SaveGameImpl(GameData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SaveFilePath, json);
        Debug.Log("Game saved to " + SaveFilePath);
    }

    public static GameData LoadGame()
    {
        EnsureInstance();
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

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject go = new GameObject("SaveLoadManager");
        instance = go.AddComponent<SaveLoadManager>();
        DontDestroyOnLoad(go);
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
    public InventoryData inventory;
    public EquipmentData equipment;
    public int playerAttack;
    public int playerDefense;
    public Vector3 playerPosition; // Added for saving/loading player position
    // Add more fields as needed for full game state

    public GameData()
    {
        // Initialize default values if needed
    }
}
