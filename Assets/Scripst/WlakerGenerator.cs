using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    public System.Random random = new System.Random();
    public Grid[,] gridHandler;
    public List<WalkerObject> Walkers;
    public Tilemap floorTilemap;
    public Tilemap structureTilemap; // New Tilemap for structures (Castle, Town, Village)
    public Tile Floor;
    public Tile Wall;
    public Tile Castle;
    public Tile Town;
    public Tile Village;
    public int MapWidth = 30;
    public int MapHeight = 30;
    public int maxNumOfWalkers = 10;
    public int tileCount = default;
    public int CastleCount = 30;
    public int TownCount = 20;
    public int VillageCount = 50;
    public float waitTime = 0.05f;
    public float fillPercentage = 0.4f;
    public int NumberOfFactions = 4;
    public GameObject GameLogic;

    // Collections to store the settlements at specific positions
    private Dictionary<Vector3Int, Town> townMap = new Dictionary<Vector3Int, Town>();
    private Dictionary<Vector3Int, Castle> castleMap = new Dictionary<Vector3Int, Castle>();
    private Dictionary<Vector3Int, Village> villageMap = new Dictionary<Vector3Int, Village>();
    private IWorldStateRepository worldStateRepository;

    void Start()
    {
        if (!ServiceLocator.TryResolve(out worldStateRepository) || worldStateRepository == null)
        {
            Debug.LogError("WalkerGenerator requires an IWorldStateRepository. Generation aborted.");
            enabled = false;
            return;
        }

        InitializeGrid();
        FactionMaker.Instance.AssignFactions(castleMap, townMap, villageMap);
        worldStateRepository.RebuildSpatialCaches();
    }

    void InitializeGrid()
    {
        gridHandler = new Grid[MapWidth, MapHeight];
        for (int x = 0; x < gridHandler.GetLength(0); x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1); y++)
            {
                gridHandler[x, y] = Grid.EMPTY;
            }
        }

        Walkers = new List<WalkerObject>();
        Vector3Int TileCenter = new Vector3Int(gridHandler.GetLength(0) / 2, gridHandler.GetLength(1) / 2, 0);
        WalkerObject curWalker = new WalkerObject(new Vector2(TileCenter.x, TileCenter.y), GetDirection(), 0.5f);
        gridHandler[TileCenter.x, TileCenter.y] = Grid.FLOOR;
        floorTilemap.SetTile(TileCenter, Floor);
        Walkers.Add(curWalker);
        tileCount++;

        CreateFloors();
        CreateCastles();
        CreateTowns();
        CreateVillages();
    }

    Vector2 GetDirection()
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
    void CreateFloors()
    {
        while ((float)tileCount / (float)(gridHandler.GetLength(0) * gridHandler.GetLength(1)) < fillPercentage)
        {
            bool hasCreatedFloor = false;
            foreach (WalkerObject curWalker in Walkers)
            {
                Vector3Int curPos = new Vector3Int((int)curWalker.Posistion.x, (int)curWalker.Posistion.y, 0);
                if (gridHandler[curPos.x, curPos.y] != Grid.FLOOR)
                {
                    floorTilemap.SetTile(curPos, Floor);
                    tileCount++;
                    gridHandler[curPos.x, curPos.y] = Grid.FLOOR;
                    hasCreatedFloor = true;
                }
            }

            ChanceToRemove();
            ChanceToRedirect();
            ChanceToCreate();
            UpdatePosition();
        }

        CreateWalls();
    }
 void ChanceToRemove()
    {
        int updatedCount = Walkers.Count;
        for (int i = 0; i < updatedCount; i++)
        {
            if (UnityEngine.Random.value < Walkers[i].ChanceToChange && Walkers.Count > 1)
            {
                Walkers.RemoveAt(i);
                break;
            }
        }
    }
 void ChanceToRedirect()
    {
        for (int i = 0; i < Walkers.Count; i++)
        {
            if (UnityEngine.Random.value < Walkers[i].ChanceToChange)
            {
                WalkerObject curWalker = Walkers[i];
                curWalker.Direction = GetDirection();
                Walkers[i] = curWalker;
            }
        }
    }
 void ChanceToCreate()
    {
        int updatedCount = Walkers.Count;
        for (int i = 0; i < updatedCount; i++)
        {
            if (UnityEngine.Random.value < Walkers[i].ChanceToChange && Walkers.Count < maxNumOfWalkers)
            {
                Vector2 newDirection = GetDirection();
                Vector2 newPosition = Walkers[i].Posistion;
                WalkerObject newWalker = new WalkerObject(newPosition, newDirection, 0.5f);
                Walkers.Add(newWalker);
            }
        }
    }


 void UpdatePosition()
    {
        for (int i = 0; i < Walkers.Count; i++)
        {
            WalkerObject foundWalker = Walkers[i];
            foundWalker.Posistion += foundWalker.Direction;
            foundWalker.Posistion.x = Mathf.Clamp(foundWalker.Posistion.x, 1, gridHandler.GetLength(0) - 2);
            foundWalker.Posistion.y = Mathf.Clamp(foundWalker.Posistion.y, 1, gridHandler.GetLength(1) - 2);
            Walkers[i] = foundWalker;
        }
    }
void CreateWalls()
{
    for (int x = 0; x < gridHandler.GetLength(0) - 1; x++)
    {
        for (int y = 0; y < gridHandler.GetLength(1) - 1; y++)
        {
            if (gridHandler[x, y] == Grid.FLOOR)
            {
                bool hasCreatedWall = false;
                if (gridHandler[x + 1, y] == Grid.EMPTY)
                {
                    floorTilemap.SetTile(new Vector3Int(x + 1, y, 0), Wall);
                    gridHandler[x + 1, y] = Grid.WALL;
                    hasCreatedWall = true;
                }
                if (gridHandler[x - 1, y] == Grid.EMPTY)
                {
                    floorTilemap.SetTile(new Vector3Int(x - 1, y, 0), Wall);
                    gridHandler[x - 1, y] = Grid.WALL;
                    hasCreatedWall = true;
                }
                if (gridHandler[x, y + 1] == Grid.EMPTY)
                {
                    floorTilemap.SetTile(new Vector3Int(x, y + 1, 0), Wall);
                    gridHandler[x, y + 1] = Grid.WALL;
                    hasCreatedWall = true;
                }
                if (gridHandler[x, y - 1] == Grid.EMPTY)
                {
                    floorTilemap.SetTile(new Vector3Int(x, y - 1, 0), Wall);
                    gridHandler[x, y - 1] = Grid.WALL;
                    hasCreatedWall = true;
                }
            }
        }
    }
}



    void CreateCastles()
    {
        int count = 0;
        int cycleCount = 0;
        bool hasCreatedCastle = false;

        while (CastleCount != count)
        {
            int x = random.Next(0, gridHandler.GetLength(0) - 1);
            int y = random.Next(0, gridHandler.GetLength(1) - 1);

            if (gridHandler[x, y] == Grid.FLOOR && !SettlementNearby(x, y))
            {
                structureTilemap.SetTile(new Vector3Int(x, y, 0), Castle);
                gridHandler[x, y] = Grid.CASTLE;
                hasCreatedCastle = true;
                Castle castle = new Castle(x, y,new Characters("Lord", 100, 100, 1));
                worldStateRepository?.Castles.Add(castle);

                // Add the created Castle to the map
                castleMap[new Vector3Int(x, y)] = castle;


                count++;
            }
            cycleCount++;
            if (cycleCount == 30 && !hasCreatedCastle)
            {
                CastleCount--;
                cycleCount = 0;
            }
            hasCreatedCastle = false;
        }
    }

    void CreateTowns()
    {
        int count = 0;
        int cycleCount = 0;
        bool hasCreatedTown = false;

        while (TownCount != count)
        {
            int x = random.Next(0, gridHandler.GetLength(0) - 1);
            int y = random.Next(0, gridHandler.GetLength(1) - 1);

            if (gridHandler[x, y] == Grid.FLOOR && !SettlementNearby(x, y))
            {
                structureTilemap.SetTile(new Vector3Int(x, y, 0), Town);
                gridHandler[x, y] = Grid.TOWN;
                hasCreatedTown = true;
                Town town = new Town(x, y);

                // Add the created Town to the map
                townMap[new Vector3Int(x, y)] = town;


                worldStateRepository?.Towns.Add(town);
                count++;
            }
            cycleCount++;
            if (cycleCount == 30 && !hasCreatedTown)
            {
                TownCount--;
                cycleCount = 0;
            }
            hasCreatedTown = false;
        }
    }

    void CreateVillages()
    {
        int count = 0;
        int cycleCount = 0;
        bool hasCreatedVillage = false;

        while (VillageCount != count)
        {
            int x = random.Next(0, gridHandler.GetLength(0) - 1);
            int y = random.Next(0, gridHandler.GetLength(1) - 1);

            if (gridHandler[x, y] == Grid.FLOOR && !SettlementNearby(x, y))
            {
                structureTilemap.SetTile(new Vector3Int(x, y, 0), Village);
                gridHandler[x, y] = Grid.VILLAGE;
                hasCreatedVillage = true;
                Village village = new Village(x, y);

                // Add the created Village to the map
                villageMap[new Vector3Int(x, y)] = village;


                worldStateRepository?.Villages.Add(village);
                count++;
            }
            cycleCount++;
            if (cycleCount == 50 && !hasCreatedVillage)
            {
                VillageCount--;
                cycleCount = 0;
            }
            hasCreatedVillage = false;
        }
    }

    // New methods to get settlements at specific positions
    public ISettlement GetTownAtPosition(Vector3Int position)
    {
        if (townMap.ContainsKey(position))
        {
            return townMap[position];
        }
        return null;
    }

    public ISettlement GetCastleAtPosition(Vector3Int position)
    {
        if (castleMap.ContainsKey(position))
        {
            return castleMap[position];
        }
        return null;
    }

    public ISettlement GetVillageAtPosition(Vector3Int position)
    {
        if (villageMap.ContainsKey(position))
        {
            return villageMap[position];
        }
        return null;
    }

    private bool SettlementNearby(int i_xExisting, int i_yExisting)
    {
        bool foundSettlement = false;
        for (int x = i_xExisting + 1; x < i_xExisting + 5; x++)
        {
            for (int y = i_yExisting + 1; y < i_yExisting + 5; y++)
            {
                if (x >= gridHandler.GetLength(0) - 1 || y >= gridHandler.GetLength(1) - 1 || x < 0 || y < 0 && foundSettlement == false)
                {
                    return false;
                }

                if (gridHandler[x, y] == Grid.CASTLE || gridHandler[x, y] == Grid.TOWN || gridHandler[x, y] == Grid.VILLAGE)
                {
                    foundSettlement = true;
                    return foundSettlement;
                }
            }
        }
        return false;
    }
}
