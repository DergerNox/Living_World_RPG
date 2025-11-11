using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using LivingWorldRPG.World;

namespace LivingWorldRPG.UI
{
    public class WorldMapUI : MonoBehaviour
    {
        public static WorldMapUI Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject worldMapPanel;
        [SerializeField] private Button closeButton;

        [Header("Map Display")]
        [SerializeField] private RawImage mapImage;
        [SerializeField] private RectTransform mapRect;
        [SerializeField, Min(1f)] private float mapScale = 10f;

        [Header("Settlement Markers")]
        [SerializeField] private GameObject settlementMarkerPrefab;
        [SerializeField] private Transform markerContainer;

        [Header("Info Panel")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private TextMeshProUGUI infoText;

        private Texture2D worldMapTexture;
        private readonly List<GameObject> settlementMarkers = new List<GameObject>();
        private IAudioService audioService;
        private IWorldGenerator worldGenerator;
        private IWorldSimulationService worldSimulation;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ResolveServices();
        }

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseWorldMap);
            }

            if (worldMapPanel != null)
            {
                worldMapPanel.SetActive(false);
            }

            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }
        }

        public void OpenWorldMap()
        {
            ResolveServices();

            GenerateWorldMapTexture();
            UpdateSettlementMarkers();

            if (worldMapPanel != null)
            {
                worldMapPanel.SetActive(true);
            }

            audioService?.PlayUIMenuOpen();
        }

        public void CloseWorldMap()
        {
            if (worldMapPanel != null)
            {
                worldMapPanel.SetActive(false);
            }

            audioService?.PlayUIMenuClose();
        }

        private void GenerateWorldMapTexture()
        {
            var generator = GetWorldGenerator();
            if (generator == null || generator.TerrainMap == null || mapImage == null)
            {
                return;
            }

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
            var simulation = ResolveWorldSimulation();
            var generator = GetWorldGenerator();
            if (simulation == null || generator == null || markerContainer == null || settlementMarkerPrefab == null)
            {
                return;
            }

            foreach (var marker in settlementMarkers)
            {
                Destroy(marker);
            }
            settlementMarkers.Clear();

            foreach (var castle in simulation.State.Castles)
            {
                CreateSettlementMarker(castle.Position, "Castle", Color.red);
            }

            foreach (var town in simulation.State.Towns)
            {
                CreateSettlementMarker(town.Position, "Town", Color.yellow);
            }

            foreach (var village in simulation.State.Villages)
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
            if (audioService == null && !ServiceLocator.TryResolve(out audioService))
            {
                Debug.LogError("WorldMapUI requires an IAudioService.");
            }

            if (worldGenerator == null && !ServiceLocator.TryResolve(out worldGenerator))
            {
                Debug.LogError("WorldMapUI requires an IWorldGenerator.");
            }

            if (worldSimulation == null && !ServiceLocator.TryResolve(out worldSimulation))
            {
                Debug.LogError("WorldMapUI requires an IWorldSimulationService.");
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

        private IWorldSimulationService ResolveWorldSimulation()
        {
            if (worldSimulation == null)
            {
                ResolveServices();
            }

            return worldSimulation;
        }
    }
}
