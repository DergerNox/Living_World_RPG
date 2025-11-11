using UnityEngine;

public class SpecialAbility
{
    public string Name { get; set; }
    public string Description { get; set; }
    public AbilityType Type { get; set; }
    public float Cooldown { get; set; }
    public float CurrentCooldown { get; set; }
    public System.Action<Troop, Troop> Effect { get; set; } // Action to apply the ability

    public SpecialAbility(string name, string description, AbilityType type, float cooldown, System.Action<Troop, Troop> effect)
    {
        Name = name;
        Description = description;
        Type = type;
        Cooldown = cooldown;
        CurrentCooldown = 0;
        Effect = effect;
    }

    public void UseAbility(Troop user, Troop target)
    {
        if (CurrentCooldown <= 0)
        {
            Effect?.Invoke(user, target);
            CurrentCooldown = Cooldown;
        }
    }

    public void ReduceCooldown(float amount)
    {
        CurrentCooldown = Mathf.Max(0, CurrentCooldown - amount);
    }
}

public enum AbilityType
{
    Offensive,
    Defensive,
    Support,
    Passive
}
