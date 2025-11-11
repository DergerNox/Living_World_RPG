using UnityEngine;

public enum FormationType
{
    Line,
    Square,
    Wedge,
    Circle,
    Skirmish
}

public class Formation
{
    public FormationType Type { get; set; }
    public float AttackBonus { get; private set; }
    public float DefenseBonus { get; private set; }
    public float SpeedPenalty { get; private set; }
    public string Description { get; private set; }

    public Formation(FormationType type)
    {
        Type = type;
        SetFormationBonuses();
    }

    private void SetFormationBonuses()
    {
        switch (Type)
        {
            case FormationType.Line:
                AttackBonus = 1.2f;
                DefenseBonus = 0.8f;
                SpeedPenalty = 0.1f;
                Description = "Standard battle line formation. Good for ranged attacks.";
                break;
            case FormationType.Square:
                AttackBonus = 0.9f;
                DefenseBonus = 1.3f;
                SpeedPenalty = 0.2f;
                Description = "Defensive square formation. Excellent for holding ground.";
                break;
            case FormationType.Wedge:
                AttackBonus = 1.4f;
                DefenseBonus = 0.7f;
                SpeedPenalty = 0.3f;
                Description = "Aggressive wedge formation. Perfect for breaking enemy lines.";
                break;
            case FormationType.Circle:
                AttackBonus = 0.8f;
                DefenseBonus = 1.2f;
                SpeedPenalty = 0.1f;
                Description = "Circular formation. Good all-around defense.";
                break;
            case FormationType.Skirmish:
                AttackBonus = 1.1f;
                DefenseBonus = 0.9f;
                SpeedPenalty = 0.0f;
                Description = "Loose skirmish formation. High mobility, low defense.";
                break;
        }
    }
}
