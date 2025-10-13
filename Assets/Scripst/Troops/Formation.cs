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
                break;
            case FormationType.Square:
                AttackBonus = 0.9f;
                DefenseBonus = 1.3f;
                SpeedPenalty = 0.2f;
                break;
            case FormationType.Wedge:
                AttackBonus = 1.4f;
                DefenseBonus = 0.7f;
                SpeedPenalty = 0.3f;
                break;
            case FormationType.Circle:
                AttackBonus = 0.8f;
                DefenseBonus = 1.2f;
                SpeedPenalty = 0.1f;
                break;
            case FormationType.Skirmish:
                AttackBonus = 1.1f;
                DefenseBonus = 0.9f;
                SpeedPenalty = 0.0f;
                break;
        }
    }
}
