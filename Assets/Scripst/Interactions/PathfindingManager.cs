using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using LivingWorldRPG.World;

namespace LivingWorldRPG.Interactions
{
    /// <summary>
    /// Singleton A* Pathfinding manager that queries the shared world generator/repository services.
    /// Grid dimensions come from the registered `IWorldGenerator` implementation.
    /// </summary>
    public class PathfindingManager : MonoBehaviour
    {
        public static PathfindingManager Instance { get; private set; }

        [Header("Node Settings")]
        [Tooltip("World units per grid cell (should match your tilemap cell size)")]
        [SerializeField, Min(0.1f)] private float nodeSize = 1f;

        private Vector2Int gridSize;      // dimensions in cells, set at runtime
        private Node[,] grid;
        private IWorldGenerator worldGenerator;
        private IWorldSimulationService worldSimulation;

        public float NodeSize => nodeSize;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private IEnumerator Start()
        {
            yield return InitializeWhenReady();
        }

        private IEnumerator InitializeWhenReady()
        {
            const float timeoutSeconds = 5f;
            float waited = 0f;
            while (ResolveWorldGenerator() == null && waited < timeoutSeconds)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            if (worldGenerator == null)
            {
                Debug.LogError("PathfindingManager: unable to resolve IWorldGenerator after waiting; pathfinding disabled.");
                enabled = false;
                yield break;
            }

            gridSize = new Vector2Int(worldGenerator.WorldWidth, worldGenerator.WorldHeight);
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
        if (grid == null)
        {
            return;
        }

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                grid[x, y].Walkable = !IsObstacleAt(cell);
            }
        }
    }

        private bool IsObstacleAt(Vector3Int cell)
        {
            var state = ResolveWorldSimulation()?.State;
            if (state == null)
            {
                return true;
            }

            if (state.GetTownAtPosition(cell) != null) return true;
            if (state.GetCastleAtPosition(cell) != null) return true;
            if (state.GetVillageAtPosition(cell) != null) return true;
            return false;
        }

    /// <summary>
    /// Finds a path from start to target cells (inclusive), returns list of cell positions or null.
    /// Optimized for large grids by limiting iterations.
    /// </summary>
    public List<Vector3Int> FindPath(Vector3Int startCell, Vector3Int targetCell)
    {
        Profiler.BeginSample("Pathfinding.FindPath");
        if (grid == null)
        {
            Profiler.EndSample();
            return null;
        }

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

    private bool InBounds(Vector3Int c) =>
        c.x >= 0 && c.x < gridSize.x && c.y >= 0 && c.y < gridSize.y;

    private static readonly Vector2Int[] _neighborDirs = new Vector2Int[] {
        new Vector2Int(1,0), new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1)
    };
    private readonly List<Node> _neighborBuffer = new List<Node>(4);

    private List<Node> GetNeighbors(Node node)
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

    private static int GetDistance(Node a, Node b)
    {
        return Mathf.Abs(a.Position.x - b.Position.x)
             + Mathf.Abs(a.Position.y - b.Position.y);
    }

    private static List<Vector3Int> RetracePath(Node start, Node end)
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

        private IWorldGenerator ResolveWorldGenerator()
        {
            if (worldGenerator != null)
            {
                return worldGenerator;
            }

            if (ServiceLocator.TryResolve(out worldGenerator) && worldGenerator != null)
            {
                return worldGenerator;
            }

            // Fallback for early execution before Bootstrapper registers services.
            var discovered = FindFirstObjectByType<WorldGenerator>();
            if (discovered != null)
            {
                worldGenerator = discovered;
                ServiceLocator.Register<WorldGenerator>(discovered);
                ServiceLocator.Register<IWorldGenerator>(discovered);
                Debug.LogWarning("PathfindingManager resolved IWorldGenerator via scene lookup and registered it with the ServiceLocator.");
            }
            else
            {
                Debug.LogError("PathfindingManager requires an IWorldGenerator registered in the ServiceLocator.");
            }

            return worldGenerator;
        }

        private IWorldSimulationService ResolveWorldSimulation()
        {
            if (worldSimulation != null)
            {
                return worldSimulation;
            }

            if (ServiceLocator.TryResolve(out worldSimulation) && worldSimulation != null)
            {
                return worldSimulation;
            }

            if (!ServiceLocator.TryResolve(out IWorldStateRepository stateRepository) || stateRepository == null)
            {
                var handler = FindFirstObjectByType<WorldHandler>();
                if (handler != null)
                {
                    stateRepository = handler;
                    ServiceLocator.Register<WorldHandler>(handler);
                    ServiceLocator.Register<IWorldStateRepository>(handler);
                }
            }

            var generator = ResolveWorldGenerator();
            if (stateRepository != null && generator != null)
            {
                worldSimulation = new WorldSimulationService(stateRepository, generator);
                ServiceLocator.Register<IWorldSimulationService>(worldSimulation);
                return worldSimulation;
            }

            Debug.LogError("PathfindingManager requires an IWorldSimulationService.");
            return null;
        }
    }
}
