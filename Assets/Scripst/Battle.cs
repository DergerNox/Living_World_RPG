using System.Collections.Generic;
using UnityEngine;

public class Battle
{
    //public variables
    public List<Party> AttackingParties { get => attackingParties; set => attackingParties = value; }
    public List<Party> DefendingParties { get => defendingParties; set => defendingParties = value; }
    public TerrainType Terrain { get; set; } = TerrainType.Plain;
    //public methods
    public bool BattleIndividual(Troop i_troopA,Troop i_troopB)
    {
        while(i_troopA.IsAlive && i_troopB.IsAlive)
        {
            // Play attack sound for troopA
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayAttackSound();
            }

            // Apply terrain and formation modifiers
            float terrainModifier = GetTerrainModifier(i_troopA, Terrain);
            float formationModifier = GetFormationModifier(i_troopA, i_troopB);
            int damage = Mathf.RoundToInt(i_troopB.Damage * terrainModifier * formationModifier);

            // Check for skill usage (simplified: random chance to use a skill)
            if (Random.value < 0.3f && i_troopA.SkillBook.GetActiveSkills().Count > 0)
            {
                Skill randomSkill = i_troopA.SkillBook.GetActiveSkills()[Random.Range(0, i_troopA.SkillBook.GetActiveSkills().Count)];
                randomSkill.UseSkill(i_troopA);
                // Skills might modify attack, so recalculate damage
                damage = Mathf.Max(1, Mathf.RoundToInt((i_troopA.Attack - i_troopB.Defense) * terrainModifier));
            }

            i_troopA.Health -= damage;
            if(i_troopA.Health<=0)
            {
                i_troopA.IsAlive=false;
                i_troopA.Health=0;
                // Play death sound for troopA
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayDeathSound();
                }
            }
            else
            {
                // Play hit sound for troopA
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayHitSound();
                }
                // Play hit visual effect
                if (VisualEffectManager.Instance != null)
                {
                    VisualEffectManager.Instance.PlayHitEffect(i_troopA.transform.position);
                }
            }

            // Play attack sound for troopB
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayAttackSound();
            }

            i_troopB.Health-=i_troopA.Damage;
            if(i_troopB.Health<=0)
            {
                i_troopB.IsAlive=false;
                i_troopB.Health=0;
                // Play death sound for troopB
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayDeathSound();
                }
            }
            else
            {
                // Play hit sound for troopB
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayHitSound();
                }
                // Play hit visual effect
                if (VisualEffectManager.Instance != null)
                {
                    VisualEffectManager.Instance.PlayHitEffect(i_troopB.transform.position);
                }
            }
        }
        if(i_troopA.IsAlive)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public void BattleParties(List<Party> i_attackingParties,List<Party> i_defendingParties)
    {
       foreach(var x in i_attackingParties)
       {
           foreach(var y in i_defendingParties)
           {
               foreach(var troopA in x.Troops)
               {
                   foreach(var troopB in y.Troops)
                   {
                       if(BattleIndividual(troopA,troopB))
                       {
                        y.Troops.Remove(troopB);
                       }
                       else
                       {
                        x.Troops.Remove(troopA);
                       }
                   }
               }
           }
       }
    }
    
    private float GetTerrainModifier(Troop troop, TerrainType terrain)
    {
        switch (terrain)
        {
            case TerrainType.Forest:
                return troop.TroopType == TroopType.Archer ? 1.2f : 0.8f; // Archers get bonus in forest
            case TerrainType.Mountain:
                return troop.TroopType == TroopType.Infantry ? 1.3f : 0.7f; // Infantry bonus in mountains
            case TerrainType.Desert:
                return troop.TroopType == TroopType.Cavalry ? 1.1f : 0.9f; // Cavalry slight bonus in desert
            case TerrainType.Swamp:
                return troop.TroopType == TroopType.Engineer ? 1.2f : 0.8f; // Engineers bonus in swamp
            case TerrainType.River:
                return 0.9f; // General penalty near rivers
            case TerrainType.City:
                return 1.1f; // Slight bonus in cities
            case TerrainType.Castle:
                return 1.2f; // Bonus in castles
            default: // Plain
                return 1.0f;
        }
    }

    private float GetFormationModifier(Troop attacker, Troop defender)
    {
        // Simplified formation logic - in a real implementation, this would consider party formations
        // For now, assume default formation bonuses
        float attackerBonus = 1.0f; // Would be based on attacker's party formation
        float defenderBonus = 1.0f; // Would be based on defender's party formation

        // Example: Wedge formation gives attack bonus but defense penalty
        // This is a placeholder - actual implementation would access party formations

        return attackerBonus / defenderBonus; // Attacker bonus vs defender bonus
    }

    //private variables
    private List<Party> attackingParties;
    private List<Party> defendingParties;
}
