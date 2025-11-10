using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [Tooltip("Reference to your WalkerGenerator (or WorldHandler) that holds the grid and tilemaps.")]
    public WalkerGenerator generator;

    private Tilemap baseTilemap;
    private Tilemap structureTilemap;
    private Vector3 targetPosition;
    private bool isMoving;

    [Tooltip("Movement speed in tiles per second.")]
    public float moveSpeed = 5f;

    // Reference to the Settlement UI Manager
    public SettlementUIManager settlementUIManager;
    private IWorldStateRepository worldStateRepository;

    void Start()
    {
        if (generator == null)
            generator = FindFirstObjectByType<WalkerGenerator>();
        if (!ServiceLocator.TryResolve(out worldStateRepository))
        {
            worldStateRepository = WorldHandler.Instance;
        }

        // Correctly assign the tilemaps from WalkerGenerator
        baseTilemap = generator.floorTilemap;
        structureTilemap = generator.structureTilemap;

        Vector3Int startCell = new Vector3Int(generator.MapWidth / 2, generator.MapHeight / 2, 0);
        targetPosition = baseTilemap.GetCellCenterWorld(startCell);
        transform.position = targetPosition;
        isMoving = false;
    }

    void Update()
    {
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

    void TryMove(Vector3Int direction)
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

    bool IsPassableTile(Vector3Int position)
    {
        var baseTile = baseTilemap.GetTile(position);
        if (baseTile != null && baseTile == generator.Floor)
            return true;

        var structureTile = structureTilemap.GetTile(position);
        if (structureTile != null && (
            structureTile == generator.Castle ||
            structureTile == generator.Town ||
            structureTile == generator.Village))
        {
            return true;
        }

        return false;
    }

    void TryInteractWithSettlement()
    {
        // Check if player is standing on a settlement tile (town, castle, village)
        Vector3Int currentCell = baseTilemap.WorldToCell(transform.position);
        TileBase structureTile = structureTilemap.GetTile(currentCell);

        if (structureTile != null && (
            structureTile == generator.Castle ||
            structureTile == generator.Town ||
            structureTile == generator.Village))
        {
            ISettlement settlement = GetSettlementAtPosition(currentCell);

            if (settlement != null)
            {
                // Open UI for the settlement
                settlementUIManager.OpenSettlementUI(settlement);
            }
        }
    }

   ISettlement GetSettlementAtPosition(Vector3Int position)
    {
        // Get the tile at the current position
        var structureTile = structureTilemap.GetTile(position);

        // Check if the tile is a known settlement tile (Town, Castle, Village)
        if (structureTile == generator.Town)
        {
            // Reference an existing Town object
            return worldStateRepository?.GetTownAtPosition(position) ?? generator.GetTownAtPosition(position);  // Ensure this method is implemented in your generator
        }
        else if (structureTile == generator.Castle)
        {
            // Reference an existing Castle object
            return worldStateRepository?.GetCastleAtPosition(position) ?? generator.GetCastleAtPosition(position);  // Ensure this method is implemented in your generator
        }
        else if (structureTile == generator.Village)
        {
            // Reference an existing Village object
            return worldStateRepository?.GetVillageAtPosition(position) ?? generator.GetVillageAtPosition(position);  // Ensure this method is implemented in your generator
        }

        // Return null if no settlement was found
        return null;
    }

}
