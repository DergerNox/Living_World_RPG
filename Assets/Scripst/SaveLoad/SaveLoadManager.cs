using System.IO;
using UnityEngine;

public static class SaveLoadManager
{
    private static string saveFilePath = Application.persistentDataPath + "/savegame.json";

    public static void SaveGame(GameData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Game saved to " + saveFilePath);
    }

    public static GameData LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("Save file not found at " + saveFilePath);
            return null;
        }

        string json = File.ReadAllText(saveFilePath);
        GameData data = JsonUtility.FromJson<GameData>(json);
        Debug.Log("Game loaded from " + saveFilePath);
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
    // Add more fields as needed for full game state

    public GameData()
    {
        // Initialize default values if needed
    }
}
