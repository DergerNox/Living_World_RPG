using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// Singleton A* Pathfinding manager using a tile-based grid from WorldHandler.
/// Grid dimensions are derived from the WalkerGenerator's MapWidth and MapHeight, with fallback to WorldGenerator.
/// </summary>
public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager Instance { get; private set; }

    [Header("Node Settings")]
    [Tooltip("World units per grid cell (should match your tilemap cell size)")]
    public float nodeSize = 1f;      // world units per cell

    private Vector2Int gridSize;      // dimensions in cells, set at runtime
    private Node[,] grid;
    private IWorldGenerator worldGenerator;
    private IWorldStateRepository worldStateRepository;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        var generator = ResolveWorldGenerator();
        if (generator != null)
        {
            gridSize = new Vector2Int(generator.WorldWidth, generator.WorldHeight);
        }
        else
        {
            Debug.LogError("WorldGenerator not found in scene. Cannot set grid size.");
            gridSize = new Vector2Int(100, 100);
        }

        InitGrid();
    }

    /// <summary>
    /// Build the node grid and mark walkable vs obstacle.
    /// </summary>
    void InitGrid()
    {
        grid = new Node[gridSize.x, gridSize.y];
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                bool walkable = !IsObstacleAt(cell);
                grid[x, y] = new Node(walkable, cell);
            }
        }
    }

    /// <summary>
    /// Update the grid when obstacles change (e.g., new settlements).
    /// </summary>
    public void UpdateGrid()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                grid[x, y].Walkable = !IsObstacleAt(cell);
            }
        }
    }

    bool IsObstacleAt(Vector3Int cell)
    {
        var repo = ResolveWorldStateRepository();
        if (repo != null)
        {
            if (repo.GetTownAtPosition(cell) != null) return true;
            if (repo.GetCastleAtPosition(cell) != null) return true;
            if (repo.GetVillageAtPosition(cell) != null) return true;
        }
        else
        {
            if (WorldHandler.GetTownAtPosition(cell) != null) return true;
            if (WorldHandler.GetCastleAtPosition(cell) != null) return true;
            if (WorldHandler.GetVillageAtPosition(cell) != null) return true;
        }
        return false;
    }

    /// <summary>
    /// Finds a path from start to target cells (inclusive), returns list of cell positions or null.
    /// Optimized for large grids by limiting iterations.
    /// </summary>
    public List<Vector3Int> FindPath(Vector3Int startCell, Vector3Int targetCell)
    {
        Profiler.BeginSample("Pathfinding.FindPath");
        if (!InBounds(startCell) || !InBounds(targetCell)) return null;

        // Reset node data
        foreach (var node in grid)
        {
            node.GCost = int.MaxValue;
            node.HCost = 0;
            node.Parent = null;
        }

        Node startNode = grid[startCell.x, startCell.y];
        Node targetNode = grid[targetCell.x, targetCell.y];
        if (!startNode.Walkable || !targetNode.Walkable) return null;

        startNode.GCost = 0;
        startNode.HCost = GetDistance(startNode, targetNode);

        var openSet = new Heap<Node>(gridSize.x * gridSize.y);
        var closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        int iterations = 0;
        int maxIterations = gridSize.x * gridSize.y * 2; // Safety limit

        while (openSet.Count > 0 && iterations < maxIterations)
        {
            Node current = openSet.RemoveFirst();
            if (current == targetNode)
            {
                var p = RetracePath(startNode, targetNode);
                Profiler.EndSample();
                return p;
            }

            closedSet.Add(current);
            foreach (Node neighbor in GetNeighbors(current))
            {
                if (!neighbor.Walkable || closedSet.Contains(neighbor)) continue;

                int tentativeG = current.GCost + GetDistance(current, neighbor);
                if (tentativeG < neighbor.GCost)
                {
                    neighbor.GCost = tentativeG;
                    neighbor.HCost = GetDistance(neighbor, targetNode);
                    neighbor.Parent = current;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                    else
                        openSet.UpdateItem(neighbor);
                }
            }
            iterations++;
        }

        Profiler.EndSample();
        return null; // no path found
    }

    bool InBounds(Vector3Int c) =>
        c.x >= 0 && c.x < gridSize.x && c.y >= 0 && c.y < gridSize.y;

    private static readonly Vector2Int[] _neighborDirs = new Vector2Int[] {
        new Vector2Int(1,0), new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1)
    };
    private readonly List<Node> _neighborBuffer = new List<Node>(4);

    List<Node> GetNeighbors(Node node)
    {
        var neighbors = _neighborBuffer;
        neighbors.Clear();
        int x = node.Position.x, y = node.Position.y;
        foreach (var d in _neighborDirs)
        {
            int nx = x + d.x, ny = y + d.y;
            if (nx >= 0 && nx < gridSize.x && ny >= 0 && ny < gridSize.y)
                neighbors.Add(grid[nx, ny]);
        }
        return neighbors;
    }

    int GetDistance(Node a, Node b)
    {
        return Mathf.Abs(a.Position.x - b.Position.x)
             + Mathf.Abs(a.Position.y - b.Position.y);
    }

    List<Vector3Int> RetracePath(Node start, Node end)
    {
        var path = new List<Vector3Int>();
        Node curr = end;
        while (curr != start)
        {
            path.Add(curr.Position);
            curr = curr.Parent;
        }
        path.Reverse();
        return path;
    }
}

    private IWorldGenerator ResolveWorldGenerator()
    {
        if (worldGenerator == null)
        {
            if (!ServiceLocator.TryResolve(out worldGenerator))
            {
                worldGenerator = WorldGenerator.Instance;
            }
        }
        return worldGenerator;
    }

    private IWorldStateRepository ResolveWorldStateRepository()
    {
        if (worldStateRepository == null)
        {
            if (!ServiceLocator.TryResolve(out worldStateRepository))
            {
                worldStateRepository = WorldHandler.Instance;
            }
        }
        return worldStateRepository;
    }
}
