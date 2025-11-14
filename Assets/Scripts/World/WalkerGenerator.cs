using LivingWorldRPG.World;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
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
        [SerializeField, Range(0.1f, 0.9f)] private float fillPercentage = 0.4f;
        [SerializeField, Min(0)] private int castleCount = 30;
        [SerializeField, Min(0)] private int townCount = 60;
        [SerializeField, Min(0)] private int villageCount = 160;
        [SerializeField, Min(1)] private int minSettlementSpacing = 4;
        [SerializeField, Range(2f, 80f)] private float villageMinSpacing = 8f;

        [Header("Noise Settings")]
        [SerializeField, Min(0.0001f)] private float landNoiseScale = 0.008f;
        [SerializeField, Min(0.0001f)] private float detailNoiseScale = 0.04f;
        [SerializeField, Range(0f, 1f)] private float detailBlend = 0.35f;
        [SerializeField] private Vector2 noiseOffset = Vector2.zero;
        [SerializeField, Min(0)] private int smoothingIterations = 2;
        [SerializeField, Range(0f, 1f)] private float fertileHeightMin = 0.12f;
        [SerializeField, Range(0f, 1f)] private float fertileHeightMax = 0.9f;
        [SerializeField] private bool useFixedNoiseSeed = false;
        [SerializeField] private int noiseSeed = 0;

        [Header("Layered Placement")]
        [SerializeField, Min(1)] private int minVillagesPerTown = 3;
        [SerializeField, Min(1)] private int townClusterRadius = 6;
        [SerializeField, Min(1)] private int castleCoverageRadius = 10;
        [SerializeField, Min(1)] private int maxTownsPerCastle = 3;

        [Header("Auto Scaling")]
        [SerializeField] private bool autoScaleSettlements = true;
        [Tooltip("Castles per 100 fertile tiles when auto scaling is enabled.")]
        [SerializeField, Min(0f)] private float castleDensityPerHundredTiles = 0.18f;
        [Tooltip("Towns per 100 fertile tiles when auto scaling is enabled.")]
        [SerializeField, Min(0f)] private float townDensityPerHundredTiles = 0.55f;
        [Tooltip("Villages per 100 fertile tiles when auto scaling is enabled.")]
        [SerializeField, Min(0f)] private float villageDensityPerHundredTiles = 1.2f;

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

        private readonly Dictionary<Vector3Int, Town> townMap = new Dictionary<Vector3Int, Town>();
        private readonly Dictionary<Vector3Int, Castle> castleMap = new Dictionary<Vector3Int, Castle>();
        private readonly Dictionary<Vector3Int, Village> villageMap = new Dictionary<Vector3Int, Village>();

        private readonly List<Vector3Int> villagePositions = new List<Vector3Int>();
        private readonly List<Vector3Int> townPositions = new List<Vector3Int>();
        private readonly List<Vector3Int> castlePositions = new List<Vector3Int>();

        private readonly List<Vector2Int> spacingOffsets = new List<Vector2Int>();
        private int cachedSpacingRadius = -1;

        private TileBase[] floorTileBuffer;
        private TileBase[] structureTileBuffer;
        private float[] heightBuffer;
        private const int NoiseHistogramBuckets = 256;
        private readonly int[] noiseHistogram = new int[NoiseHistogramBuckets];
        private int fertileCellCount;

        private IWorldSimulationService worldSimulation;
        private IWorldStateRepository WorldState => worldSimulation?.State;

        public event Action GenerationCompleted;
        public bool IsGenerationComplete { get; private set; }
        private float previousTimeScale = 1f;

        private void Start()
        {
            if (!ServiceLocator.TryResolve(out worldSimulation) || worldSimulation == null)
            {
                Debug.LogError("WalkerGenerator requires an IWorldSimulationService. Generation aborted.");
                enabled = false;
                return;
            }

            WorldGenerationState.BeginGeneration();
            StartCoroutine(GenerateWorldRoutine());
        }

        private IEnumerator GenerateWorldRoutine()
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            ResetWorldState();
            EnsureBuffers();

            yield return GenerateTerrainCoroutine();
            CalculateSettlementCounts();
            yield return CreateVillagesCoroutine();
            yield return CreateTownsCoroutine();
            yield return CreateCastlesCoroutine();

            ApplyTilemaps();
            FactionMaker.Instance?.AssignFactions(castleMap, townMap, villageMap);
            WorldState?.RebuildSpatialCaches();
            CompleteGeneration();
        }

        private void ResetWorldState()
        {
            villagePositions.Clear();
            townPositions.Clear();
            castlePositions.Clear();
            townMap.Clear();
            castleMap.Clear();
            villageMap.Clear();
            gridHandler = new Grid[mapWidth, mapHeight];
            IsGenerationComplete = false;
        }

        private void EnsureBuffers()
        {
            int size = mapWidth * mapHeight;
            if (size <= 0)
            {
                return;
            }

            if (floorTileBuffer == null || floorTileBuffer.Length != size)
            {
                floorTileBuffer = new TileBase[size];
            }

            if (structureTileBuffer == null || structureTileBuffer.Length != size)
            {
                structureTileBuffer = new TileBase[size];
            }

            if (heightBuffer == null || heightBuffer.Length != size)
            {
                heightBuffer = new float[size];
            }

            Array.Clear(noiseHistogram, 0, noiseHistogram.Length);
        }

        private IEnumerator GenerateTerrainCoroutine()
        {
            System.Random noiseRandom = useFixedNoiseSeed ? new System.Random(noiseSeed) : random;
            float offsetX = noiseOffset.x + noiseRandom.Next(-10000, 10000);
            float offsetY = noiseOffset.y + noiseRandom.Next(-10000, 10000);

            int totalCells = mapWidth * mapHeight;
            NativeArray<float> noiseResults = new NativeArray<float>(totalCells, Allocator.TempJob);
            var job = new DualPerlinNoiseJob
            {
                width = mapWidth,
                height = mapHeight,
                offsetX = offsetX,
                offsetY = offsetY,
                landScale = landNoiseScale,
                detailScale = detailNoiseScale,
                detailBlend = detailBlend,
                result = noiseResults
            };

            JobHandle handle = job.Schedule(totalCells, 512);
            while (!handle.IsCompleted)
            {
                yield return null;
            }
            handle.Complete();

            noiseResults.CopyTo(heightBuffer);
            noiseResults.Dispose();

            int idx = 0;
            int rowsPerYield = Mathf.Max(1, mapHeight / 32);
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++, idx++)
                {
                    float noiseValue = heightBuffer[idx];
                    int bucket = Mathf.Clamp(Mathf.RoundToInt(noiseValue * (NoiseHistogramBuckets - 1)), 0, NoiseHistogramBuckets - 1);
                    noiseHistogram[bucket]++;
                }

                if ((y % rowsPerYield) == 0)
                {
                    yield return null;
                }
            }

            float threshold = DetermineNoiseThreshold(totalCells);

            NativeArray<int> gridArray = new NativeArray<int>(totalCells, Allocator.TempJob);
            idx = 0;
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++, idx++)
                {
                    gridArray[idx] = heightBuffer[idx] >= threshold ? 1 : 0;
                }

                if ((y % rowsPerYield) == 0)
                {
                    yield return null;
                }
            }

            for (int iter = 0; iter < Mathf.Max(0, smoothingIterations); iter++)
            {
                NativeArray<int> next = new NativeArray<int>(totalCells, Allocator.TempJob);
                var smoothJob = new SmoothingJob
                {
                    width = mapWidth,
                    height = mapHeight,
                    source = gridArray,
                    destination = next
                };
                JobHandle smoothHandle = smoothJob.Schedule(totalCells, 512);
                while (!smoothHandle.IsCompleted)
                {
                    yield return null;
                }
                smoothHandle.Complete();

                gridArray.Dispose();
                gridArray = next;
            }

            idx = 0;
            fertileCellCount = 0;
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++, idx++)
                {
                    bool isFloor = gridArray[idx] == 1;
                    if (isFloor && IsHeightFertile(heightBuffer[idx]))
                    {
                        fertileCellCount++;
                    }
                    gridHandler[x, y] = isFloor ? Grid.FLOOR : Grid.WALL;
                }

                if ((y % rowsPerYield) == 0)
                {
                    yield return null;
                }
            }

            gridArray.Dispose();
        }

        private bool IsHeightFertile(float height)
        {
            return height >= fertileHeightMin && height <= fertileHeightMax;
        }

        private float DetermineNoiseThreshold(int totalCells)
        {
            if (totalCells <= 0)
            {
                return 0.5f;
            }

            int targetLandCells = Mathf.Clamp(Mathf.RoundToInt(totalCells * fillPercentage), 1, totalCells);
            int cumulative = 0;

            for (int bucket = NoiseHistogramBuckets - 1; bucket >= 0; bucket--)
            {
                cumulative += noiseHistogram[bucket];
                if (cumulative >= targetLandCells)
                {
                    return bucket / (float)(NoiseHistogramBuckets - 1);
                }
            }

            return fillPercentage;
        }

        private void CalculateSettlementCounts()
        {
            if (!autoScaleSettlements)
            {
                return;
            }

            float fertileHundreds = Mathf.Max(1f, fertileCellCount / 100f);

            castleCount = Mathf.Clamp(Mathf.RoundToInt(fertileHundreds * castleDensityPerHundredTiles), 15, 55);
            townCount = Mathf.Clamp(Mathf.RoundToInt(fertileHundreds * townDensityPerHundredTiles), 40, 150);
            villageCount = Mathf.Clamp(Mathf.RoundToInt(fertileHundreds * villageDensityPerHundredTiles), 80, 250);
        }

        private IEnumerator CreateTownsCoroutine()
        {
            if (villagePositions.Count == 0)
            {
                yield break;
            }

            float minDistance = Mathf.Max(4f, townClusterRadius);
            float minDistanceSqr = minDistance * minDistance;
            int targetTowns = Mathf.Max(1, townCount);
            int iterations = 0;
            float chunkSize = Mathf.Max(2f, townClusterRadius);
            int chunksX = Mathf.Max(1, Mathf.CeilToInt(mapWidth / chunkSize));
            int chunksY = Mathf.Max(1, Mathf.CeilToInt(mapHeight / chunkSize));
            var chunkVillages = new List<Vector3Int>[chunksX * chunksY];
            var chunkScores = new List<ChunkInfo>();

            foreach (var village in villagePositions)
            {
                int cx = Mathf.Clamp(Mathf.FloorToInt(village.x / chunkSize), 0, chunksX - 1);
                int cy = Mathf.Clamp(Mathf.FloorToInt(village.y / chunkSize), 0, chunksY - 1);
                int idx = cx + cy * chunksX;
                var list = chunkVillages[idx] ??= new List<Vector3Int>();
                list.Add(village);
            }

            for (int i = 0; i < chunkVillages.Length; i++)
            {
                var list = chunkVillages[i];
                if (list == null || list.Count == 0)
                {
                    continue;
                }

                float score = 0f;
                foreach (var cell in list)
                {
                    score += SampleHeight(cell);
                }
                score /= list.Count;
                chunkScores.Add(new ChunkInfo { ChunkIndex = i, Score = score });
            }

            chunkScores.Sort((a, b) => b.Score.CompareTo(a.Score));

            foreach (var chunk in chunkScores)
            {
                if (townPositions.Count >= targetTowns)
                {
                    break;
                }

                var list = chunkVillages[chunk.ChunkIndex];
                if (list == null || list.Count == 0)
                {
                    continue;
                }

                Vector3 centroid = Vector3.zero;
                foreach (var cell in list)
                {
                    centroid += (Vector3)cell;
                }
                centroid /= list.Count;

                Vector3Int candidate = new Vector3Int(Mathf.RoundToInt(centroid.x), Mathf.RoundToInt(centroid.y), 0);
                if (!TryFindFloorCellNear(candidate, out Vector3Int townCell, townClusterRadius))
                {
                    townCell = list[0];
                }

                bool tooClose = false;
                foreach (var existing in townPositions)
                {
                    if ((existing - townCell).sqrMagnitude < minDistanceSqr)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose)
                {
                    continue;
                }

                TryPlaceTownCell(townCell);

                if (++iterations % 6 == 0)
                {
                    yield return null;
                }
            }

            if (townPositions.Count == 0 && villagePositions.Count > 0)
            {
                TryPlaceTownCell(villagePositions[0]);
            }
        }

        private IEnumerator CreateVillagesCoroutine()
        {
            float radius = Mathf.Max(2f, villageMinSpacing);
            float cellSize = radius / Mathf.Sqrt(2f);
            int bucketsX = Mathf.Max(1, Mathf.CeilToInt(mapWidth / cellSize));
            int bucketsY = Mathf.Max(1, Mathf.CeilToInt(mapHeight / cellSize));
            var buckets = new List<Vector3Int>[bucketsX * bucketsY];
            float radiusSqr = radius * radius;
            int stride = Mathf.Max(1, Mathf.RoundToInt(radius));
            int halfStride = Mathf.Max(1, stride / 2);
            System.Random localRandom = useFixedNoiseSeed ? new System.Random(noiseSeed) : random;

            NativeArray<int> floorMask = new NativeArray<int>(mapWidth * mapHeight, Allocator.TempJob);
            int idx = 0;
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++, idx++)
                {
                    floorMask[idx] = gridHandler[x, y] == Grid.FLOOR && IsHeightFertile(heightBuffer[idx]) ? 1 : 0;
                }

                if ((y % 64) == 0)
                {
                    yield return null;
                }
            }

            int estimatedCandidates = Mathf.Max(1, (mapWidth / stride + 2) * (mapHeight / stride + 2));
            NativeList<int> candidateList = new NativeList<int>(estimatedCandidates, Allocator.TempJob);
            var candidateJob = new RegionCandidateJob
            {
                width = mapWidth,
                height = mapHeight,
                stride = stride,
                halfStride = halfStride,
                floorMask = floorMask,
                randomSeed = (uint)(noiseSeed == 0 ? 1 : noiseSeed) + 123u,
                results = candidateList
            };

            JobHandle handle = candidateJob.Schedule();
            while (!handle.IsCompleted)
            {
                yield return null;
            }
            handle.Complete();
            floorMask.Dispose();

            var managedCandidates = new List<Vector3Int>(candidateList.Length);
            for (int i = 0; i < candidateList.Length; i++)
            {
                int flat = candidateList[i];
                managedCandidates.Add(new Vector3Int(flat % mapWidth, flat / mapWidth, 0));
            }
            candidateList.Dispose();

            // Shuffle candidates
            for (int i = managedCandidates.Count - 1; i > 0; i--)
            {
                int j = localRandom.Next(i + 1);
                (managedCandidates[i], managedCandidates[j]) = (managedCandidates[j], managedCandidates[i]);
            }

            int placed = 0;
            for (int i = 0; i < managedCandidates.Count && placed < villageCount; i++)
            {
                var candidate = managedCandidates[i];
                if (!IsValidVillageCandidate(candidate, buckets, bucketsX, bucketsY, cellSize, radiusSqr, mapWidth, mapHeight))
                {
                    continue;
                }

                if (TryPlaceVillageCell(candidate))
                {
                    placed++;
                    AddToBucket(candidate, buckets, bucketsX, bucketsY, cellSize, mapWidth, mapHeight);
                }

                if ((i % 24) == 0)
                {
                    yield return null;
                }
            }

            if (placed >= villageCount)
            {
                yield break;
            }

            float fallbackRadius = Mathf.Max(2f, radius * 0.5f);
            float fallbackRadiusSqr = fallbackRadius * fallbackRadius;
            for (int y = 1; y < mapHeight - 1 && placed < villageCount; y++)
            {
                for (int x = 1; x < mapWidth - 1 && placed < villageCount; x++)
                {
                    var candidate = new Vector3Int(x, y, 0);
                    if (!IsValidVillageCandidate(candidate, buckets, bucketsX, bucketsY, cellSize, fallbackRadiusSqr, mapWidth, mapHeight))
                    {
                        continue;
                    }

                    if (TryPlaceVillageCell(candidate))
                    {
                        placed++;
                        AddToBucket(candidate, buckets, bucketsX, bucketsY, cellSize, mapWidth, mapHeight);
                    }

                    if (((x + y) % 96) == 0)
                    {
                        yield return null;
                    }
                }
            }

            if (placed < villageCount)
            {
                for (int y = 1; y < mapHeight - 1 && placed < villageCount; y++)
                {
                    for (int x = 1; x < mapWidth - 1 && placed < villageCount; x++)
                    {
                        if (gridHandler[x, y] != Grid.FLOOR || !IsHeightFertile(heightBuffer[x + y * mapWidth]))
                        {
                            continue;
                        }

                        if (TryPlaceVillageCell(new Vector3Int(x, y, 0)))
                        {
                            placed++;
                        }

                        if (((x + y) % 160) == 0)
                        {
                            yield return null;
                        }
                    }
                }
            }
        }

        private IEnumerator CreateCastlesCoroutine()
        {
            if (townPositions.Count == 0)
            {
                yield break;
            }

            float minDistance = Mathf.Max(6f, castleCoverageRadius);
            float minDistanceSqr = minDistance * minDistance;
            int targetCastles = Mathf.Max(1, castleCount);
            int iterations = 0;
            float chunkSize = Mathf.Max(2f, castleCoverageRadius);
            int chunksX = Mathf.Max(1, Mathf.CeilToInt(mapWidth / chunkSize));
            int chunksY = Mathf.Max(1, Mathf.CeilToInt(mapHeight / chunkSize));
            var chunkTowns = new List<Vector3Int>[chunksX * chunksY];
            var chunkScores = new List<ChunkInfo>();

            foreach (var town in townPositions)
            {
                int cx = Mathf.Clamp(Mathf.FloorToInt(town.x / chunkSize), 0, chunksX - 1);
                int cy = Mathf.Clamp(Mathf.FloorToInt(town.y / chunkSize), 0, chunksY - 1);
                int idx = cx + cy * chunksX;
                var list = chunkTowns[idx] ??= new List<Vector3Int>();
                list.Add(town);
            }

            for (int i = 0; i < chunkTowns.Length; i++)
            {
                var list = chunkTowns[i];
                if (list == null || list.Count == 0)
                {
                    continue;
                }

                float score = 0f;
                foreach (var cell in list)
                {
                    score += SampleHeight(cell);
                }
                score /= list.Count;
                chunkScores.Add(new ChunkInfo { ChunkIndex = i, Score = score });
            }

            chunkScores.Sort((a, b) => b.Score.CompareTo(a.Score));

            foreach (var chunk in chunkScores)
            {
                if (castlePositions.Count >= targetCastles)
                {
                    break;
                }

                var list = chunkTowns[chunk.ChunkIndex];
                if (list == null || list.Count == 0)
                {
                    continue;
                }

                Vector3 centroid = Vector3.zero;
                foreach (var cell in list)
                {
                    centroid += (Vector3)cell;
                }
                centroid /= list.Count;

                Vector3Int candidate = new Vector3Int(Mathf.RoundToInt(centroid.x), Mathf.RoundToInt(centroid.y), 0);
                if (!TryFindFloorCellNear(candidate, out Vector3Int castleCell, castleCoverageRadius))
                {
                    castleCell = list[0];
                }

                bool tooClose = false;
                foreach (var existing in castlePositions)
                {
                    if ((existing - castleCell).sqrMagnitude < minDistanceSqr)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose)
                {
                    continue;
                }

                TryPlaceCastleCell(castleCell);

                if (++iterations % 6 == 0)
                {
                    yield return null;
                }
            }

            if (castlePositions.Count == 0)
            {
                TryPlaceCastleCell(townPositions[0]);
            }
        }

        private void CompleteGeneration()
        {
            if (IsGenerationComplete)
            {
                return;
            }

            IsGenerationComplete = true;
            GenerationCompleted?.Invoke();
            Time.timeScale = previousTimeScale;
            WorldGenerationState.EndGeneration();
        }

        public ISettlement GetTownAtPosition(Vector3Int position) =>
            townMap.TryGetValue(position, out var town) ? town : null;

        public ISettlement GetCastleAtPosition(Vector3Int position) =>
            castleMap.TryGetValue(position, out var castle) ? castle : null;

        public ISettlement GetVillageAtPosition(Vector3Int position) =>
            villageMap.TryGetValue(position, out var village) ? village : null;

        private bool TryFindFloorCellNear(Vector3Int center, out Vector3Int cell, int searchRadius = 5)
        {
            if (IsValidFloorCell(center))
            {
                cell = center;
                return true;
            }

            for (int radius = 1; radius <= searchRadius; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        Vector3Int candidate = new Vector3Int(center.x + dx, center.y + dy, 0);
                        if (IsValidFloorCell(candidate))
                        {
                            cell = candidate;
                            return true;
                        }
                    }
                }
            }

            cell = Vector3Int.zero;
            return false;
        }

        private bool IsValidFloorCell(Vector3Int cell)
        {
            if (cell.x <= 0 || cell.x >= mapWidth || cell.y <= 0 || cell.y >= mapHeight)
            {
                return false;
            }

            return gridHandler[cell.x, cell.y] == Grid.FLOOR && !SettlementNearby(cell.x, cell.y);
        }

        private bool TryPlaceVillageCell(Vector3Int cell)
        {
            if (!IsValidFloorCell(cell))
            {
                return false;
            }

            gridHandler[cell.x, cell.y] = Grid.VILLAGE;
            villagePositions.Add(cell);
            Village village = new Village(cell.x, cell.y);
            villageMap[cell] = village;
            WorldState?.Villages.Add(village);
            return true;
        }

        private bool TryPlaceTownCell(Vector3Int cell)
        {
            if (!IsValidFloorCell(cell))
            {
                return false;
            }

            gridHandler[cell.x, cell.y] = Grid.TOWN;
            townPositions.Add(cell);
            Town town = new Town(cell.x, cell.y);
            townMap[cell] = town;
            WorldState?.Towns.Add(town);
            return true;
        }

        private bool TryPlaceCastleCell(Vector3Int cell)
        {
            if (!IsValidFloorCell(cell))
            {
                return false;
            }

            gridHandler[cell.x, cell.y] = Grid.CASTLE;
            castlePositions.Add(cell);
            Castle castle = new Castle(cell.x, cell.y, new global::Characters("Lord", 100, 100, 1));
            castleMap[cell] = castle;
            WorldState?.Castles.Add(castle);
            return true;
        }

        private float SampleHeight(Vector3Int cell)
        {
            int idx = cell.x + cell.y * mapWidth;
            if (idx < 0 || idx >= heightBuffer.Length)
            {
                return 0f;
            }
            return heightBuffer[idx];
        }

        private static int ChebyshevDistance(Vector3Int a, Vector3Int b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }

        private void EnsureSpacingOffsets()
        {
            int radius = Mathf.Max(1, minSettlementSpacing);
            if (cachedSpacingRadius == radius && spacingOffsets.Count > 0)
            {
                return;
            }

            spacingOffsets.Clear();
            cachedSpacingRadius = radius;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) <= radius)
                    {
                        spacingOffsets.Add(new Vector2Int(dx, dy));
                    }
                }
            }
        }

        private bool SettlementNearby(int xCenter, int yCenter)
        {
            EnsureSpacingOffsets();

            for (int i = 0; i < spacingOffsets.Count; i++)
            {
                int x = xCenter + spacingOffsets[i].x;
                int y = yCenter + spacingOffsets[i].y;

                if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight)
                {
                    continue;
                }

                Grid cell = gridHandler[x, y];
                if (cell == Grid.CASTLE || cell == Grid.TOWN || cell == Grid.VILLAGE)
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyTilemaps()
        {
            if (gridHandler == null)
            {
                return;
            }

            EnsureBuffers();

            BoundsInt bounds = new BoundsInt(Vector3Int.zero, new Vector3Int(mapWidth, mapHeight, 1));

            if (floorTilemap != null)
            {
                int idx = 0;
                for (int y = 0; y < mapHeight; y++)
                for (int x = 0; x < mapWidth; x++, idx++)
                {
                    switch (gridHandler[x, y])
                    {
                        case Grid.FLOOR:
                            floorTileBuffer[idx] = floorTile;
                            break;
                        case Grid.WALL:
                            floorTileBuffer[idx] = wallTile;
                            break;
                        default:
                            floorTileBuffer[idx] = null;
                            break;
                    }
                }

                floorTilemap.ClearAllTiles();
                floorTilemap.SetTilesBlock(bounds, floorTileBuffer);
            }

            if (structureTilemap != null)
            {
                int idx = 0;
                for (int y = 0; y < mapHeight; y++)
                for (int x = 0; x < mapWidth; x++, idx++)
                {
                    switch (gridHandler[x, y])
                    {
                        case Grid.CASTLE:
                            structureTileBuffer[idx] = castleTile;
                            break;
                        case Grid.TOWN:
                            structureTileBuffer[idx] = townTile;
                            break;
                        case Grid.VILLAGE:
                            structureTileBuffer[idx] = villageTile;
                            break;
                        default:
                            structureTileBuffer[idx] = null;
                            break;
                    }
                }

                structureTilemap.ClearAllTiles();
                structureTilemap.SetTilesBlock(bounds, structureTileBuffer);
            }
        }

        private struct DualPerlinNoiseJob : IJobParallelFor
        {
            public int width;
            public int height;
            public float landScale;
            public float detailScale;
            public float detailBlend;
            public float offsetX;
            public float offsetY;
            public NativeArray<float> result;

            public void Execute(int index)
            {
                int x = index % width;
                int y = index / width;
                float lx = (x + offsetX) * landScale;
                float ly = (y + offsetY) * landScale;
                float dx = (x + offsetX * 1.37f) * detailScale;
                float dy = (y + offsetY * 1.91f) * detailScale;

                float land = Mathf.PerlinNoise(lx, ly);
                float detail = Mathf.PerlinNoise(dx, dy);

                result[index] = Mathf.Lerp(land, detail, detailBlend);
            }
        }

        private struct VillageBlockJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<int> floorMask;
            public int width;
            public int height;
            public int stride;
            public int halfStride;
            public int blocksX;
            public float minSpacing;
            public NativeArray<int> results;

            public void Execute(int index)
            {
                int blockX = index % blocksX;
                int blockY = index / blocksX;

                int baseX = 1 + blockX * stride;
                int baseY = 1 + blockY * stride;
                int centerX = Mathf.Clamp(baseX + halfStride, 1, width - 2);
                int centerY = Mathf.Clamp(baseY + halfStride, 1, height - 2);

                int searchRadius = Mathf.CeilToInt(minSpacing);
                int bestIndex = FindNearestFloor(centerX, centerY, searchRadius);
                results[index] = bestIndex;
            }

            private int FindNearestFloor(int cx, int cy, int radius)
            {
                if (IsFloor(cx, cy))
                {
                    return Flatten(cx, cy);
                }

                for (int r = 1; r <= radius; r++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        int x1 = cx + dx;
                        int y1 = cy + r;
                        if (IsFloor(x1, y1))
                        {
                            return Flatten(x1, y1);
                        }
                        y1 = cy - r;
                        if (IsFloor(x1, y1))
                        {
                            return Flatten(x1, y1);
                        }
                    }

                    for (int dy = -(r - 1); dy <= r - 1; dy++)
                    {
                        int y1 = cy + dy;
                        int x1 = cx + r;
                        if (IsFloor(x1, y1))
                        {
                            return Flatten(x1, y1);
                        }
                        x1 = cx - r;
                        if (IsFloor(x1, y1))
                        {
                            return Flatten(x1, y1);
                        }
                    }
                }

                return -1;
            }

            private bool IsFloor(int x, int y)
            {
                if (x < 1 || y < 1 || x >= width - 1 || y >= height - 1)
                {
                    return false;
                }

                return floorMask[Flatten(x, y)] == 1;
            }

        private int Flatten(int x, int y) => x + y * width;
        }

        private struct SmoothingJob : IJobParallelFor
        {
            public int width;
            public int height;
            [ReadOnly] public NativeArray<int> source;
            public NativeArray<int> destination;

            public void Execute(int index)
            {
                int x = index % width;
                int y = index / width;

                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    destination[index] = source[index];
                    return;
                }

                int floorCount = 0;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0)
                        {
                            continue;
                        }

                        int nx = x + dx;
                        int ny = y + dy;
                        int nIndex = nx + ny * width;
                        floorCount += source[nIndex];
                    }
                }

                destination[index] = floorCount >= 5 ? 1 : 0;
            }
        }

        private struct ChunkInfo
        {
            public int ChunkIndex;
            public float Score;
        }

        private struct RegionCandidateJob : IJob
        {
            public int width;
            public int height;
            public int stride;
            public int halfStride;
            [ReadOnly] public NativeArray<int> floorMask;
            public NativeList<int> results;
            public uint randomSeed;

            public void Execute()
            {
                var rng = new Unity.Mathematics.Random(randomSeed == 0 ? 1u : randomSeed);
                for (int y = halfStride; y < height - halfStride; y += stride)
                {
                    for (int x = halfStride; x < width - halfStride; x += stride)
                    {
                        int jitterX = math.clamp(x + rng.NextInt(-halfStride, halfStride + 1), 1, width - 2);
                        int jitterY = math.clamp(y + rng.NextInt(-halfStride, halfStride + 1), 1, height - 2);
                        int idx = jitterX + jitterY * width;
                        if (floorMask[idx] == 1)
                        {
                            results.Add(idx);
                        }
                    }
                }
            }
        }

        private static void AddToBucket(Vector3Int cell, List<Vector3Int>[] buckets, int bucketsX, int bucketsY, float cellSize, int width, int height)
        {
            int bx = Mathf.Clamp(Mathf.FloorToInt(cell.x / cellSize), 0, bucketsX - 1);
            int by = Mathf.Clamp(Mathf.FloorToInt(cell.y / cellSize), 0, bucketsY - 1);
            int idx = bx + by * bucketsX;
            var list = buckets[idx] ??= new List<Vector3Int>();
            list.Add(cell);
        }

        private static bool IsValidVillageCandidate(Vector3Int cell, List<Vector3Int>[] buckets, int bucketsX, int bucketsY, float cellSize, float minDistSqr, int width, int height)
        {
            if (cell.x < 1 || cell.y < 1 || cell.x >= width - 1 || cell.y >= height - 1)
            {
                return false;
            }

            int bx = Mathf.Clamp(Mathf.FloorToInt(cell.x / cellSize), 0, bucketsX - 1);
            int by = Mathf.Clamp(Mathf.FloorToInt(cell.y / cellSize), 0, bucketsY - 1);

            for (int nx = bx - 1; nx <= bx + 1; nx++)
            {
                if (nx < 0 || nx >= bucketsX)
                {
                    continue;
                }

                for (int ny = by - 1; ny <= by + 1; ny++)
                {
                    if (ny < 0 || ny >= bucketsY)
                    {
                        continue;
                    }

                        var bucket = buckets[nx + ny * bucketsX];
                        if (bucket == null)
                        {
                            continue;
                        }

                        for (int i = 0; i < bucket.Count; i++)
                        {
                            if ((bucket[i] - cell).sqrMagnitude < minDistSqr)
                            {
                                return false;
                            }
                        }
                }
            }

            return true;
        }
    }
}
