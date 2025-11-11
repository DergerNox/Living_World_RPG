using System.Collections.Generic;
using UnityEngine;

public class Player:Characters
{
    //Constructor

    public Player(string name, int maxHealth = 100, int maxMana = 100, int level = 1) : base(name, maxHealth, maxMana, level)
    {
        Name = name;
        MaxHealth = maxHealth;
        Health = maxHealth; // Initialize health to max health
        MaxMana = maxMana;
        Mana = maxMana; // Initialize mana to max mana
        Level = level;
        Experience = 0;
        Gold = 1000; // Starting gold
        //SkillBook = new SkillBook();
        Party = new Party(); // Initialize party
        Party.Leader = this; // Set the player as the party leader
        Party.FactionId = 0; // Player starts in faction 0 or neutral
        Inventory.AddItem(new ItemData { name = "Sword", type = ItemType.Weapon, attackBonus = 5 });
        Inventory.AddItem(new ItemData { name = "Health Potion", type = ItemType.Consumable, healthBonus = 20 });
        Inventory.AddItem(new ItemData { name = "Armor", type = ItemType.Armor, defenseBonus = 3 });

    }
}
