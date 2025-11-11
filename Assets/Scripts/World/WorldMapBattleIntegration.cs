using UnityEngine;
using System.Collections.Generic;

public class WorldMapBattleIntegration : MonoBehaviour
{
    public static WorldMapBattleIntegration Instance;

    [Header("Battle Settings")]
    public TerrainType defaultTerrain = TerrainType.Plain;

    private IBattleResolver battleResolver;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (!ServiceLocator.TryResolve(out battleResolver))
        {
            battleResolver = new Battle();
        }
    }

    public void InitiateBattle(List<Party> attackers, List<Party> defenders, TerrainType terrain = TerrainType.Plain)
    {
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.StartBattle(attackers, defenders, terrain);
            return;
        }

        // If no UI is available, resolve immediately
        battleResolver?.ResolveParties(attackers, defenders, terrain);
        HandleBattleResults(attackers, defenders);
    }

    private void HandleBattleResults(List<Party> attackers, List<Party> defenders)
    {
        // Check if attackers won
        bool attackersWon = true;
        foreach (var defender in defenders)
        {
            if (defender.Troops.Count > 0)
            {
                attackersWon = false;
                break;
            }
        }

        if (attackersWon)
        {
            // Award experience, loot, etc.
            foreach (var attacker in attackers)
            {
                foreach (var troop in attacker.Troops)
                {
                    troop.GainExperience(50); // Example experience gain
                }
            }
            Debug.Log("Attackers won the battle!");
        }
        else
        {
            Debug.Log("Defenders won the battle!");
        }
    }

    // Method to determine terrain based on world position
    public TerrainType GetTerrainAtPosition(Vector3 position)
    {
        // Placeholder logic - in a real implementation, this would check the world map
        // For example, check biome, elevation, etc.
        return defaultTerrain;
    }
}
