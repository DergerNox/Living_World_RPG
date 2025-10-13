using System.Collections.Generic;
using UnityEngine;

public class Characters
{
    //public variables
    public string Name { get { return name; } set { name = value; } }
    public int Health { get { return health; } set { health = value; } }
    public int MaxHealth { get { return maxHealth; } set { maxHealth = value; } }
    public int Mana { get { return mana; } set { mana = value; } }
    public int MaxMana { get { return maxMana; } set { maxMana = value; } }
    public int Gold { get { return gold; } set { gold = value; } }
    public int Level { get { return level; } set { level = value; } }
    public int Experience { get { return experience; } set { experience = value; } }
    public SkillBook SkillBook { get { return skillBook; } set { skillBook = value; } }
    public Party Party { get { return party; } set { party = value; } }
    public Inventory Inventory { get; set; } = new Inventory();
    public Equipment Equipment { get; set; } = new Equipment();
    public int Attack { get; set; } = 10;
    public int Defense { get; set; } = 5;
    public Characters(string name, int maxHealth, int maxMana, int level = 1)
    {
        this.name = name;
        this.maxHealth = maxHealth;
        this.health = maxHealth; // Initialize health to max health
        this.maxMana = maxMana;
        this.mana = maxMana; // Initialize mana to max mana
        this.level = level;
        this.experience = 0;
        this.gold = 0;
        this.skillBook = new SkillBook();
        this.Inventory = new Inventory();
        this.Equipment = new Equipment();
        this.Attack = 10;
        this.Defense = 5;
    }
    public Characters()
    {
        this.Inventory = new Inventory();
        this.Equipment = new Equipment();
        this.Attack = 10;
        this.Defense = 5;
    }
    public void GainExperience(int exp)
    {
        experience += exp;
        if (experience >= 100 * level) // Example: level up after every 100 * level experience points
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        maxHealth += 10; // Increase max health per level
        maxMana += 5; // Increase max mana per level
        experience = 0; // Reset experience after leveling up
    }
    public void AddGold(int amount)
    {
        gold += amount;
    }

    public void SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
        }
        else
        {
            Debug.Log("Not enough gold!");
        }
    }

    public void EquipItem(EquipmentSlot slot, Item item)
    {
        Item oldItem = Equipment.UnequipItem(slot);
        if (oldItem != null)
        {
            Inventory.AddItem(oldItem);
            // remove old bonuses
            Attack -= oldItem.attackBonus;
            Defense -= oldItem.defenseBonus;
            Health -= oldItem.healthBonus;
            Mana -= oldItem.manaBonus;
        }
        Equipment.EquipItem(slot, item);
        Inventory.RemoveItem(item);
        // add new bonuses
        Attack += item.attackBonus;
        Defense += item.defenseBonus;
        Health += item.healthBonus;
        Mana += item.manaBonus;
    }

    public void UseItem(Item item)
    {
        if (item.type == ItemType.Consumable)
        {
            Health += item.healthBonus;
            if (Health > MaxHealth) Health = MaxHealth;
            Mana += item.manaBonus;
            if (Mana > MaxMana) Mana = MaxMana;
            Inventory.RemoveItem(item);
        }
    }

    //private variables
    private string name;
    private int health;
    private int maxHealth;
    private int mana;
    private int maxMana;
    private int gold;
    private int level;
    private int experience;
    private SkillBook skillBook;
    private Party party;
}
