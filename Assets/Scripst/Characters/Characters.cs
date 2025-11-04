using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Characters : ISkillTarget
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
    // REMOVED: public SkillBook SkillBook { get; set; } - replaced with direct skill management
    public Party Party { get { return party; } set { party = value; } }
    public Inventory Inventory { get; set; } = new Inventory();
    public Equipment Equipment { get; set; } = new Equipment();
    public int Strength { get; set; } = 10;
    public int Agility { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Attack { get; set; } = 10;
    public int Defense { get; set; } = 5;

    // REPLACES SkillBook - Direct skill management
    private Dictionary<string, BaseSkill> learnedSkills = new Dictionary<string, BaseSkill>();
    private List<IStatusEffect> activeStatusEffects = new List<IStatusEffect>();
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
        // REMOVED: this.skillBook = new SkillBook();
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

    // ISkillTarget implementation
    public void TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(0, damage - Defense);
        Health -= actualDamage;
        if (Health < 0) Health = 0;
    }

    public void Heal(int amount)
    {
        Health += amount;
        if (Health > MaxHealth) Health = MaxHealth;
    }

    public void ApplyStatusEffect(IStatusEffect effect)
    {
        activeStatusEffects.Add(effect);
        effect.Apply(this);
    }

    // Skill Management Methods (replaces SkillBook functionality)
    public void LearnSkill(BaseSkill skill)
    {
        if (!learnedSkills.ContainsKey(skill.SkillId))
        {
            learnedSkills.Add(skill.SkillId, skill);
            SkillEvents.SkillLearned(this, skill);
            Debug.Log($"{Name} learned {skill.SkillName}");
        }
    }

    public void ForgetSkill(string skillId)
    {
        learnedSkills.Remove(skillId);
    }

    public bool HasSkill(string skillId)
    {
        return learnedSkills.ContainsKey(skillId);
    }

    public BaseSkill GetSkill(string skillId)
    {
        return learnedSkills.ContainsKey(skillId) ? learnedSkills[skillId] : null;
    }

    public List<BaseSkill> GetAllSkills()
    {
        return new List<BaseSkill>(learnedSkills.Values);
    }

    public List<BaseSkill> GetSkillsByType(SkillType type)
    {
        return learnedSkills.Values.Where(s => s.SkillType == type).ToList();
    }

    public List<BaseSkill> GetActiveSkills()
    {
        return learnedSkills.Values.Where(s => !s.IsPassive).ToList();
    }

    public List<BaseSkill> GetPassiveSkills()
    {
        return learnedSkills.Values.Where(s => s.IsPassive).ToList();
    }

    public void UpdateSkillCooldowns(float deltaTime)
    {
        foreach (var skill in learnedSkills.Values)
        {
            skill.UpdateCooldown(deltaTime);
        }
    }

    public void UpdateStatusEffects(float deltaTime)
    {
        for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
        {
            activeStatusEffects[i].Update(deltaTime);
            if (activeStatusEffects[i].IsExpired)
            {
                activeStatusEffects[i].Remove(this);
                activeStatusEffects.RemoveAt(i);
            }
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
    private Party party;
}
