using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class WorldMapUI : MonoBehaviour
{
    public static WorldMapUI Instance;

    [Header("UI Panels")]
    public GameObject worldMapPanel;
    public Button closeButton;

    [Header("Map Display")]
    public RawImage mapImage;
    public RectTransform mapRect;
    public float mapScale = 10f;

    [Header("Settlement Markers")]
    public GameObject settlementMarkerPrefab;
    public Transform markerContainer;

    [Header("Info Panel")]
    public GameObject infoPanel;
    public TextMeshProUGUI infoText;

    private Texture2D worldMapTexture;
    private List<GameObject> settlementMarkers = new List<GameObject>();
    private IAudioService audioService;
    private IWorldGenerator worldGenerator;
    private IWorldStateRepository worldStateRepository;

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

        ResolveServices();
    }

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseWorldMap);
        }

        worldMapPanel.SetActive(false);
        infoPanel.SetActive(false);
    }

    public void OpenWorldMap()
    {
        ResolveServices();
        GenerateWorldMapTexture();
        UpdateSettlementMarkers();

        worldMapPanel.SetActive(true);

        audioService?.PlayUIMenuOpen();
    }

    public void CloseWorldMap()
    {
        worldMapPanel.SetActive(false);

        audioService?.PlayUIMenuClose();
    }

    private void GenerateWorldMapTexture()
    {
        var generator = GetWorldGenerator();
        if (generator == null || generator.TerrainMap == null) return;

        int width = generator.WorldWidth;
        int height = generator.WorldHeight;

        worldMapTexture = new Texture2D(width, height);
        worldMapTexture.filterMode = FilterMode.Point;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = GetTerrainColor(generator.TerrainMap[x, y]);
                worldMapTexture.SetPixel(x, y, color);
            }
        }

        worldMapTexture.Apply();
        mapImage.texture = worldMapTexture;
    }

    private Color GetTerrainColor(TerrainType terrain)
    {
        switch (terrain)
        {
            case TerrainType.Grass: return Color.green;
            case TerrainType.Forest: return new Color(0.2f, 0.5f, 0.2f);
            case TerrainType.Water: return Color.blue;
            case TerrainType.Hills: return new Color(0.6f, 0.4f, 0.2f);
            case TerrainType.Mountains: return Color.gray;
            default: return Color.white;
        }
    }

    private void UpdateSettlementMarkers()
    {
        var repository = GetWorldStateRepository();
        var generator = GetWorldGenerator();
        if (repository == null || generator == null) return;

        // Clear existing markers
        foreach (var marker in settlementMarkers)
        {
            Destroy(marker);
        }
        settlementMarkers.Clear();

        // Add markers for settlements
        foreach (var castle in repository.Castles)
        {
            CreateSettlementMarker(castle.Position, "Castle", Color.red);
        }

        foreach (var town in repository.Towns)
        {
            CreateSettlementMarker(town.Position, "Town", Color.yellow);
        }

        foreach (var village in repository.Villages)
        {
            CreateSettlementMarker(village.Position, "Village", Color.cyan);
        }
    }

    private void CreateSettlementMarker(Vector3Int position, string type, Color color)
    {
        var generator = GetWorldGenerator();
        if (generator == null) return;

        GameObject marker = Instantiate(settlementMarkerPrefab, markerContainer);
        RectTransform markerRect = marker.GetComponent<RectTransform>();

        // Convert world position to UI position
        Vector2 uiPosition = new Vector2(
            (position.x / (float)generator.WorldWidth - 0.5f) * mapRect.rect.width,
            (position.y / (float)generator.WorldHeight - 0.5f) * mapRect.rect.height
        );

        markerRect.anchoredPosition = uiPosition;

        // Set marker color
        Image markerImage = marker.GetComponent<Image>();
        if (markerImage != null)
        {
            markerImage.color = color;
        }

        // Add click handler
        Button markerButton = marker.GetComponent<Button>();
        if (markerButton != null)
        {
            Vector2Int cachedPosition = new Vector2Int(position.x, position.y);
            markerButton.onClick.AddListener(() => ShowSettlementInfo(cachedPosition, type));
        }

        settlementMarkers.Add(marker);
    }

    private void ShowSettlementInfo(Vector2Int position, string type)
    {
        infoText.text = $"{type} at ({position.x}, {position.y})";
        infoPanel.SetActive(true);
    }

    private void ResolveServices()
    {
        if (audioService == null)
        {
            if (!ServiceLocator.TryResolve(out audioService) || audioService == null)
            {
                Debug.LogError("WorldMapUI requires an IAudioService.");
            }
        }

        if (worldGenerator == null)
        {
            if (!ServiceLocator.TryResolve(out worldGenerator) || worldGenerator == null)
            {
                Debug.LogError("WorldMapUI requires an IWorldGenerator.");
            }
        }

        if (worldStateRepository == null)
        {
            if (!ServiceLocator.TryResolve(out worldStateRepository) || worldStateRepository == null)
            {
                Debug.LogError("WorldMapUI requires an IWorldStateRepository.");
            }
        }
    }

    private IWorldGenerator GetWorldGenerator()
    {
        if (worldGenerator == null)
        {
            ResolveServices();
        }
        return worldGenerator;
    }

    private IWorldStateRepository GetWorldStateRepository()
    {
        if (worldStateRepository == null)
        {
            ResolveServices();
        }
        return worldStateRepository;
    }
}
