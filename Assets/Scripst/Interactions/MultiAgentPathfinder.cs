using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles multi-agent pathfinding to avoid collisions.
/// Uses reservation table to prevent overlapping paths.
/// </summary>
public class MultiAgentPathfinder : MonoBehaviour
{
    public static MultiAgentPathfinder Instance { get; private set; }

    private Dictionary<Vector3Int, int> reservationTable = new Dictionary<Vector3Int, int>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Finds paths for multiple agents, avoiding collisions.
    /// </summary>
    public List<List<Vector3Int>> FindMultiAgentPaths(List<Vector3Int> starts, List<Vector3Int> targets, int maxTime = 100)
    {
        var paths = new List<List<Vector3Int>>();
        reservationTable.Clear();

        for (int i = 0; i < starts.Count; i++)
        {
            var path = FindPathWithReservations(starts[i], targets[i], maxTime);
            if (path != null)
            {
                paths.Add(path);
                ReservePath(path, i);
            }
            else
            {
                paths.Add(null); // Failed to find path
            }
        }

        return paths;
    }

    private List<Vector3Int> FindPathWithReservations(Vector3Int start, Vector3Int target, int maxTime)
    {
        if (PathfindingManager.Instance == null) return null;

        var path = PathfindingManager.Instance.FindPath(start, target);
        if (path == null) return null;

        // Check for conflicts
        for (int t = 0; t < path.Count && t < maxTime; t++)
        {
            var pos = path[t];
            if (reservationTable.ContainsKey(pos) && reservationTable[pos] <= t)
            {
                // Conflict, try to find alternative
                return FindAlternativePath(start, target, maxTime);
            }
        }

        return path;
    }

    private List<Vector3Int> FindAlternativePath(Vector3Int start, Vector3Int target, int maxTime)
    {
        // Simple implementation: try diagonal or longer paths
        // For now, return null if conflict
        return null;
    }

    private void ReservePath(List<Vector3Int> path, int agentId)
    {
        for (int t = 0; t < path.Count; t++)
        {
            var pos = path[t];
            reservationTable[pos] = t;
        }
    }
}
