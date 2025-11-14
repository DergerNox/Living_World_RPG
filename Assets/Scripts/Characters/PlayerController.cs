using UnityEngine;
using UnityEngine.Tilemaps;
using LivingWorldRPG.World;
using LivingWorldRPG.UI;

namespace LivingWorldRPG.Characters
{
    public class PlayerController : MonoBehaviour
    {
        [Tooltip("Reference to your WalkerGenerator that holds the grid and tilemaps.")]
        [SerializeField] private WalkerGenerator generator;

        [Tooltip("Movement speed in tiles per second.")]
        [SerializeField, Min(0.1f)] private float moveSpeed = 5f;

        [SerializeField] private SettlementUIManager settlementUIManager;

        private Tilemap baseTilemap;
        private Tilemap structureTilemap;
        private Vector3 targetPosition;
        private bool isMoving;
        private bool playerInitialized;
        private IWorldSimulationService worldSimulation;

        private IWorldStateRepository WorldRepository => worldSimulation?.State;

        private void Start()
        {
            if (generator == null)
            {
                generator = FindFirstObjectByType<WalkerGenerator>();
            }

            if (generator == null)
            {
                Debug.LogError("PlayerController requires a WalkerGenerator reference.");
                enabled = false;
                return;
            }

            if (settlementUIManager == null)
            {
                settlementUIManager = FindFirstObjectByType<SettlementUIManager>();
            }

            ServiceLocator.TryResolve(out worldSimulation);
            if (worldSimulation == null)
            {
                Debug.LogWarning("PlayerController: IWorldSimulationService not available; falling back to WalkerGenerator data.");
            }

            if (generator.IsGenerationComplete)
            {
                InitializePlayerState();
            }
            else
            {
                generator.GenerationCompleted += HandleWorldGenerated;
            }
        }

        private void OnDestroy()
        {
            if (generator != null)
            {
                generator.GenerationCompleted -= HandleWorldGenerated;
            }
        }

        private void InitializePlayerState()
        {
            baseTilemap = generator.FloorTilemap;
            structureTilemap = generator.StructureTilemap;

            Vector3Int startCell = new Vector3Int(generator.MapWidth / 2, generator.MapHeight / 2, 0);
            targetPosition = baseTilemap.GetCellCenterWorld(startCell);
            transform.position = targetPosition;
            isMoving = false;
            playerInitialized = true;
        }

        private void HandleWorldGenerated()
        {
            generator.GenerationCompleted -= HandleWorldGenerated;
            InitializePlayerState();
        }

        private void Update()
    {
        if (!playerInitialized)
        {
            return;
        }

        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
        else
        {
            HandleHoldInput();
        }

        // Check for "E" key press to interact with settlements
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteractWithSettlement();
        }
    }

        private void HandleHoldInput()
    {
        int x = 0, y = 0;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) y = 1;
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) y = -1;

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) x = 1;
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) x = -1;

        Vector3Int dir = new Vector3Int(x, y, 0);
        if (dir != Vector3Int.zero)
            TryMove(dir);
    }

        private void TryMove(Vector3Int direction)
    {
        Vector3Int currentCell = baseTilemap.WorldToCell(transform.position);
        Vector3Int nextCell = currentCell + direction;

        if (nextCell.x < 0 || nextCell.x >= generator.MapWidth || nextCell.y < 0 || nextCell.y >= generator.MapHeight)
            return;

        if (IsPassableTile(nextCell))
        {
            targetPosition = baseTilemap.GetCellCenterWorld(nextCell);
            isMoving = true;
        }
    }

        private bool IsPassableTile(Vector3Int position)
    {
        var baseTile = baseTilemap.GetTile(position);
        if (baseTile != null && baseTile == generator.FloorTile)
            return true;

        var structureTile = structureTilemap.GetTile(position);
        if (structureTile != null && (
            structureTile == generator.CastleTile ||
            structureTile == generator.TownTile ||
            structureTile == generator.VillageTile))
        {
            return true;
        }

        return false;
    }

        private void TryInteractWithSettlement()
    {
        // Check if player is standing on a settlement tile (town, castle, village)
        Vector3Int currentCell = baseTilemap.WorldToCell(transform.position);
        TileBase structureTile = structureTilemap.GetTile(currentCell);

        if (structureTile != null && (
            structureTile == generator.CastleTile ||
            structureTile == generator.TownTile ||
            structureTile == generator.VillageTile))
        {
            ISettlement settlement = GetSettlementAtPosition(currentCell);

            if (settlement != null)
            {
                // Open UI for the settlement
                settlementUIManager.OpenSettlementUI(settlement);
            }
        }
    }

        private ISettlement GetSettlementAtPosition(Vector3Int position)
    {
        // Get the tile at the current position
        var structureTile = structureTilemap.GetTile(position);

        // Check if the tile is a known settlement tile (Town, Castle, Village)
        if (structureTile == generator.TownTile)
        {
            // Reference an existing Town object
            return WorldRepository != null ? WorldRepository.GetTownAtPosition(position) : generator.GetTownAtPosition(position);
        }
        else if (structureTile == generator.CastleTile)
        {
            // Reference an existing Castle object
            return WorldRepository != null ? WorldRepository.GetCastleAtPosition(position) : generator.GetCastleAtPosition(position);
        }
        else if (structureTile == generator.VillageTile)
        {
            // Reference an existing Village object
            return WorldRepository != null ? WorldRepository.GetVillageAtPosition(position) : generator.GetVillageAtPosition(position);
        }

        // Return null if no settlement was found
            return null;
        }
    }
}
