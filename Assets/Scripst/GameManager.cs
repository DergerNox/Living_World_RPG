using UnityEngine;

public class GameManager : MonoBehaviour
{
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
    }

    public void SaveGame()
    {
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

        SaveLoadManager.SaveGame(data);
    }

    public void LoadGame()
    {
        GameData data = SaveLoadManager.LoadGame();
        if (data != null)
        {
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
        }
    }

    void LogInventory()
    {
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
