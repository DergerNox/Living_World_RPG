using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

public class Battle
{
    //public variables
    public List<Party> AttackingParties { get => attackingParties; set => attackingParties = value; }
    public List<Party> DefendingParties { get => defendingParties; set => defendingParties = value; }
    public TerrainType Terrain { get; set; } = TerrainType.Plain;
    //public methods
    public bool BattleIndividual(Troop i_troopA, Troop i_troopB)
    {
        while (i_troopA.IsAlive && i_troopB.IsAlive)
        {
            // TroopA attacks TroopB
            // Play attack sound for troopA
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayAttackSound();
            }

            // Apply terrain modifier
            float terrainModifierA = GetTerrainModifier(i_troopA, Terrain);
            int damageA = Mathf.RoundToInt(i_troopA.Damage * terrainModifierA);

            i_troopB.Health -= damageA;
            if (i_troopB.Health <= 0)
            {
                i_troopB.IsAlive = false;
                i_troopB.Health = 0;
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
            }

            if (!i_troopB.IsAlive) break;

            // TroopB attacks TroopA
            // Play attack sound for troopB
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayAttackSound();
            }

            // Apply terrain modifier
            float terrainModifierB = GetTerrainModifier(i_troopB, Terrain);
            int damageB = Mathf.RoundToInt(i_troopB.Damage * terrainModifierB);

            i_troopA.Health -= damageB;
            if (i_troopA.Health <= 0)
            {
                i_troopA.IsAlive = false;
                i_troopA.Health = 0;
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
            }
        }
        return i_troopA.IsAlive;
    }

    public void BattleParties(List<Party> i_attackingParties, List<Party> i_defendingParties)
    {
        ConcurrentBag<(Troop attacker, bool survived)> battleResults = new ConcurrentBag<(Troop, bool)>();
        ConcurrentBag<(Troop defender, bool survived)> defenderResults = new ConcurrentBag<(Troop, bool)>();

        List<Troop> allAttackers = new List<Troop>();
        List<Troop> allDefenders = new List<Troop>();
        foreach (var party in i_attackingParties)
        {
            allAttackers.AddRange(party.Troops);
        }
        foreach (var party in i_defendingParties)
        {
            allDefenders.AddRange(party.Troops);
        }

        // Parallel processing of battles
        Parallel.For(0, Mathf.Min(allAttackers.Count, allDefenders.Count), i =>
        {
            Troop troopA = allAttackers[i];
            Troop troopB = allDefenders[i];
            bool attackerSurvived = BattleIndividual(troopA, troopB);
            battleResults.Add((troopA, attackerSurvived));
            defenderResults.Add((troopB, !attackerSurvived));
        });

        // Aggregate results
        List<Troop> survivingAttackers = new List<Troop>();
        List<Troop> survivingDefenders = new List<Troop>();
        foreach (var result in battleResults)
        {
            if (result.survived)
            {
                survivingAttackers.Add(result.attacker);
            }
        }
        foreach (var result in defenderResults)
        {
            if (result.survived)
            {
                survivingDefenders.Add(result.defender);
            }
        }

        // Update parties
        int attackerIndex = 0;
        foreach (var party in i_attackingParties)
        {
            party.Troops.Clear();
            int troopLimit = party.Size - party.Members.Count;
            while (attackerIndex < survivingAttackers.Count && party.Troops.Count < troopLimit)
            {
                party.Troops.Add(survivingAttackers[attackerIndex++]);
            }
        }
        int defenderIndex = 0;
        foreach (var party in i_defendingParties)
        {
            party.Troops.Clear();
            int troopLimit = party.Size - party.Members.Count;
            while (defenderIndex < survivingDefenders.Count && party.Troops.Count < troopLimit)
            {
                party.Troops.Add(survivingDefenders[defenderIndex++]);
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
