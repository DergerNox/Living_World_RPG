using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LivingWorldRPG.World
{
    /// <summary>
    /// Legacy walker-based terrain/settlement generator used for prototyping.
    /// Writes its results back into the shared world simulation service.
    /// </summary>
    public class WalkerGenerator : MonoBehaviour
    {
        public enum Grid
        {
            FLOOR,
            WALL,
            EMPTY,
            CASTLE,
            TOWN,
            VILLAGE
        }

        [Header("Tilemaps")]
        [SerializeField] private Tilemap floorTilemap;
        [SerializeField] private Tilemap structureTilemap;

        [Header("Tiles")]
        [SerializeField] private Tile floorTile;
        [SerializeField] private Tile wallTile;
        [SerializeField] private Tile castleTile;
        [SerializeField] private Tile townTile;
        [SerializeField] private Tile villageTile;

        [Header("Generation Settings")]
        [SerializeField, Min(1)] private int mapWidth = 30;
        [SerializeField, Min(1)] private int mapHeight = 30;
        [SerializeField, Min(1)] private int maxNumOfWalkers = 10;
        [SerializeField, Range(0.1f, 0.9f)] private float fillPercentage = 0.4f;
        [SerializeField, Min(0)] private int castleCount = 30;
        [SerializeField, Min(0)] private int townCount = 20;
        [SerializeField, Min(0)] private int villageCount = 50;

        [Header("Debug/References")]
        [SerializeField] private GameObject gameLogicRoot;

        public int MapWidth => mapWidth;
        public int MapHeight => mapHeight;
        public Tilemap FloorTilemap => floorTilemap;
        public Tilemap StructureTilemap => structureTilemap;
        public Tile FloorTile => floorTile;
        public Tile CastleTile => castleTile;
        public Tile TownTile => townTile;
        public Tile VillageTile => villageTile;

        private readonly System.Random random = new System.Random();
        private Grid[,] gridHandler;
        private List<WalkerObject> walkers;

        private readonly Dictionary<Vector3Int, Town> townMap = new Dictionary<Vector3Int, Town>();
        private readonly Dictionary<Vector3Int, Castle> castleMap = new Dictionary<Vector3Int, Castle>();
        private readonly Dictionary<Vector3Int, Village> villageMap = new Dictionary<Vector3Int, Village>();

        private IWorldSimulationService worldSimulation;
        private IWorldStateRepository WorldState => worldSimulation?.State;

        private int tileCount;

        private void Start()
        {
            if (!ServiceLocator.TryResolve(out worldSimulation) || worldSimulation == null)
            {
                Debug.LogError("WalkerGenerator requires an IWorldSimulationService. Generation aborted.");
                enabled = false;
                return;
            }

            InitializeGrid();
            FactionMaker.Instance?.AssignFactions(castleMap, townMap, villageMap);
            WorldState?.RebuildSpatialCaches();
        }

        private void InitializeGrid()
        {
            gridHandler = new Grid[mapWidth, mapHeight];
            for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
            {
                gridHandler[x, y] = Grid.EMPTY;
            }

            walkers = new List<WalkerObject>();
            Vector3Int tileCenter = new Vector3Int(mapWidth / 2, mapHeight / 2, 0);
            WalkerObject curWalker = new WalkerObject(new Vector2(tileCenter.x, tileCenter.y), GetDirection(), 0.5f);
            gridHandler[tileCenter.x, tileCenter.y] = Grid.FLOOR;
            floorTilemap?.SetTile(tileCenter, floorTile);
            walkers.Add(curWalker);
            tileCount = 1;

            CreateFloors();
            CreateCastles();
            CreateTowns();
            CreateVillages();
        }

        private static Vector2 GetDirection()
        {
            int choice = Mathf.FloorToInt(UnityEngine.Random.value * 3.99f);
            switch (choice)
            {
                case 0: return Vector2.down;
                case 1: return Vector2.left;
                case 2: return Vector2.up;
                case 3: return Vector2.right;
                default: return Vector2.zero;
            }
        }

        private void CreateFloors()
        {
            while (tileCount / (float)(mapWidth * mapHeight) < fillPercentage)
            {
                foreach (WalkerObject curWalker in walkers)
                {
                    Vector3Int curPos = new Vector3Int((int)curWalker.Posistion.x, (int)curWalker.Posistion.y, 0);
                    if (gridHandler[curPos.x, curPos.y] != Grid.FLOOR)
                    {
                        floorTilemap?.SetTile(curPos, floorTile);
                        tileCount++;
                        gridHandler[curPos.x, curPos.y] = Grid.FLOOR;
                    }
                }

                ChanceToRemove();
                ChanceToRedirect();
                ChanceToCreate();
                UpdatePosition();
            }

            CreateWalls();
        }

        private void ChanceToRemove()
        {
            int updatedCount = walkers.Count;
            for (int i = 0; i < updatedCount; i++)
            {
                if (UnityEngine.Random.value < walkers[i].ChanceToChange && walkers.Count > 1)
                {
                    walkers.RemoveAt(i);
                    break;
                }
            }
        }

        private void ChanceToRedirect()
        {
            for (int i = 0; i < walkers.Count; i++)
            {
                if (UnityEngine.Random.value < walkers[i].ChanceToChange)
                {
                    WalkerObject curWalker = walkers[i];
                    curWalker.Direction = GetDirection();
                    walkers[i] = curWalker;
                }
            }
        }

        private void ChanceToCreate()
        {
            int updatedCount = walkers.Count;
            for (int i = 0; i < updatedCount; i++)
            {
                if (UnityEngine.Random.value < walkers[i].ChanceToChange && walkers.Count < maxNumOfWalkers)
                {
                    Vector2 newDirection = GetDirection();
                    Vector2 newPosition = walkers[i].Posistion;
                    WalkerObject newWalker = new WalkerObject(newPosition, newDirection, 0.5f);
                    walkers.Add(newWalker);
                }
            }
        }

        private void UpdatePosition()
        {
            for (int i = 0; i < walkers.Count; i++)
            {
                WalkerObject foundWalker = walkers[i];
                foundWalker.Posistion += foundWalker.Direction;
                foundWalker.Posistion.x = Mathf.Clamp(foundWalker.Posistion.x, 1, mapWidth - 2);
                foundWalker.Posistion.y = Mathf.Clamp(foundWalker.Posistion.y, 1, mapHeight - 2);
                walkers[i] = foundWalker;
            }
        }

        private void CreateWalls()
        {
            for (int x = 0; x < mapWidth - 1; x++)
            for (int y = 0; y < mapHeight - 1; y++)
            {
                if (gridHandler[x, y] != Grid.FLOOR)
                {
                    continue;
                }

                TryPlaceWall(new Vector3Int(x + 1, y, 0));
                TryPlaceWall(new Vector3Int(x - 1, y, 0));
                TryPlaceWall(new Vector3Int(x, y + 1, 0));
                TryPlaceWall(new Vector3Int(x, y - 1, 0));
            }
        }

        private void TryPlaceWall(Vector3Int cell)
        {
            if (cell.x <= 0 || cell.x >= mapWidth || cell.y <= 0 || cell.y >= mapHeight)
            {
                return;
            }

            if (gridHandler[cell.x, cell.y] == Grid.EMPTY)
            {
                floorTilemap?.SetTile(cell, wallTile);
                gridHandler[cell.x, cell.y] = Grid.WALL;
            }
        }

        private void CreateCastles()
        {
            int count = 0;
            int cycleCount = 0;
            bool hasCreated = false;

            while (count < castleCount)
            {
                int x = random.Next(0, mapWidth - 1);
                int y = random.Next(0, mapHeight - 1);

                if (gridHandler[x, y] == Grid.FLOOR && !SettlementNearby(x, y))
                {
                    structureTilemap?.SetTile(new Vector3Int(x, y, 0), castleTile);
                    gridHandler[x, y] = Grid.CASTLE;
                    hasCreated = true;
                    Castle castle = new Castle(x, y, new global::Characters("Lord", 100, 100, 1));
                    castleMap[new Vector3Int(x, y)] = castle;
                    WorldState?.Castles.Add(castle);
                    count++;
                }

                cycleCount++;
                if (cycleCount == 30 && !hasCreated)
                {
                    castleCount--;
                    cycleCount = 0;
                }
                hasCreated = false;
            }
        }

        private void CreateTowns()
        {
            int count = 0;
            int cycleCount = 0;
            bool hasCreated = false;

            while (count < townCount)
            {
                int x = random.Next(0, mapWidth - 1);
                int y = random.Next(0, mapHeight - 1);

                if (gridHandler[x, y] == Grid.FLOOR && !SettlementNearby(x, y))
                {
                    structureTilemap?.SetTile(new Vector3Int(x, y, 0), townTile);
                    gridHandler[x, y] = Grid.TOWN;
                    hasCreated = true;
                    Town town = new Town(x, y);
                    townMap[new Vector3Int(x, y)] = town;
                    WorldState?.Towns.Add(town);
                    count++;
                }

                cycleCount++;
                if (cycleCount == 30 && !hasCreated)
                {
                    townCount--;
                    cycleCount = 0;
                }
                hasCreated = false;
            }
        }

        private void CreateVillages()
        {
            int count = 0;
            int cycleCount = 0;
            bool hasCreated = false;

            while (count < villageCount)
            {
                int x = random.Next(0, mapWidth - 1);
                int y = random.Next(0, mapHeight - 1);

                if (gridHandler[x, y] == Grid.FLOOR && !SettlementNearby(x, y))
                {
                    structureTilemap?.SetTile(new Vector3Int(x, y, 0), villageTile);
                    gridHandler[x, y] = Grid.VILLAGE;
                    hasCreated = true;
                    Village village = new Village(x, y);
                    villageMap[new Vector3Int(x, y)] = village;
                    WorldState?.Villages.Add(village);
                    count++;
                }

                cycleCount++;
                if (cycleCount == 50 && !hasCreated)
                {
                    villageCount--;
                    cycleCount = 0;
                }
                hasCreated = false;
            }
        }

        public ISettlement GetTownAtPosition(Vector3Int position) =>
            townMap.TryGetValue(position, out var town) ? town : null;

        public ISettlement GetCastleAtPosition(Vector3Int position) =>
            castleMap.TryGetValue(position, out var castle) ? castle : null;

        public ISettlement GetVillageAtPosition(Vector3Int position) =>
            villageMap.TryGetValue(position, out var village) ? village : null;

        private bool SettlementNearby(int existingX, int existingY)
        {
            for (int x = existingX + 1; x < existingX + 5; x++)
            for (int y = existingY + 1; y < existingY + 5; y++)
            {
                if (x >= mapWidth - 1 || y >= mapHeight - 1 || x < 0 || y < 0)
                {
                    return false;
                }

                if (gridHandler[x, y] == Grid.CASTLE || gridHandler[x, y] == Grid.TOWN || gridHandler[x, y] == Grid.VILLAGE)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
