using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections;
using System.Collections.Generic;

namespace LivingWorldRPG.World
{
    public class WalkerGenerator : MonoBehaviour
    {
        private enum Cell
        {
            Floor,
            Wall
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
        [SerializeField, Min(1)] private int mapWidth = 512;
        [SerializeField, Min(1)] private int mapHeight = 512;
        [SerializeField, Range(2f, 80f)] private float villageMinSpacing = 6f;
        [SerializeField, Min(0.1f)] private float villagesPerHundredLand = 1.4f;
        [SerializeField, Min(0.1f)] private float villagesPerTownRatio = 5.5f;
        [SerializeField, Min(0.1f)] private float townsPerCastleRatio = 2.2f;
        [SerializeField, Range(0.5f, 3f)] private float densityMultiplier = 1.4f;
        [SerializeField, Min(1)] private int townLloydIterations = 2;
        [SerializeField, Min(1)] private int castleLloydIterations = 2;
        [SerializeField, Min(1)] private int numberOfFactions = 5;

        [Header("Noise Settings")]
        [SerializeField, Min(0.00001f)] private float continentScale = 0.0025f;
        [SerializeField, Min(0.0001f)] private float detailNoiseScale = 0.02f;
        [SerializeField, Range(0f, 1f)] private float detailBlend = 0.2f;
        [SerializeField, Range(0f, 2f)] private float falloffStrength = 0.9f;
        [SerializeField, Range(1f, 6f)] private float falloffPower = 3f;
        [SerializeField] private Vector2 noiseOffset = Vector2.zero;
        [SerializeField, Min(0)] private int terrainBlurIterations = 1;
        [SerializeField, Min(0)] private int smoothingIterations = 2;
        [SerializeField, Range(0f, 1f)] private float fertileHeightMin = 0.12f;
        [SerializeField, Range(0f, 1f)] private float fertileHeightMax = 0.88f;
        [SerializeField] private bool useFixedNoiseSeed = false;
        [SerializeField] private int noiseSeed = 0;

        [Header("Spacing Controls")]
        [SerializeField, Min(1)] private int townClusterRadius = 8;
        [SerializeField, Min(1)] private int castleCoverageRadius = 16;

        private readonly List<Vector3Int> villagePositions = new List<Vector3Int>();
        private readonly List<Vector3Int> townPositions = new List<Vector3Int>();
        private readonly List<Vector3Int> castlePositions = new List<Vector3Int>();
        private readonly Dictionary<Vector3Int, Village> villagesByPosition = new Dictionary<Vector3Int, Village>();
        private readonly Dictionary<Vector3Int, Town> townsByPosition = new Dictionary<Vector3Int, Town>();
        private readonly Dictionary<Vector3Int, Castle> castlesByPosition = new Dictionary<Vector3Int, Castle>();
        private FactionMaker factionMaker;

        private const int HistogramBuckets = 256;
        private readonly int[] noiseHistogram = new int[HistogramBuckets];
        private TileBase[] floorTileBuffer;
        private TileBase[] structureTileBuffer;
        private float[] heightBuffer;
        private int landCellCount;
        private int fertileCellCount;
        private int villageCount;
        private int townCount;
        private int castleCount;

        private Cell[,] grid;
        private IWorldSimulationService worldSimulation;
        private IWorldStateRepository WorldRepository => worldSimulation?.State;

        public Tilemap FloorTilemap => floorTilemap;
        public Tilemap StructureTilemap => structureTilemap;
        public Tile FloorTile => floorTile;
        public Tile CastleTile => castleTile;
        public Tile TownTile => townTile;
        public Tile VillageTile => villageTile;
        public int MapWidth => mapWidth;
        public int MapHeight => mapHeight;
        public Village GetVillageAtPosition(Vector3Int position)
        {
            position.z = 0;
            villagesByPosition.TryGetValue(position, out var village);
            return village;
        }

        public Town GetTownAtPosition(Vector3Int position)
        {
            position.z = 0;
            townsByPosition.TryGetValue(position, out var town);
            return town;
        }

        public Castle GetCastleAtPosition(Vector3Int position)
        {
            position.z = 0;
            castlesByPosition.TryGetValue(position, out var castle);
            return castle;
        }

        public event Action GenerationCompleted;
        public bool IsGenerationComplete { get; private set; }
        private float cachedTimeScale = 1f;

        private void Start()
        {
            if (!ServiceLocator.TryResolve(out worldSimulation) || worldSimulation == null)
            {
                Debug.LogError("WalkerGenerator requires an IWorldSimulationService.");
                enabled = false;
                return;
            }

            ServiceLocator.TryResolve(out factionMaker);

            WorldGenerationState.BeginGeneration();
            StartCoroutine(GenerateWorldRoutine());
        }

        private IEnumerator GenerateWorldRoutine()
        {
            cachedTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            ResetState();
            EnsureBuffers();

            yield return GenerateTerrainCoroutine();
            CalculateCounts();
            yield return CreateVillagesCoroutine();
            yield return CreateTownsCoroutine();
            yield return CreateCastlesCoroutine();

            CacheSettlementInstances();
            ApplyTilemaps();
            WorldGenerationState.EndGeneration();
            Time.timeScale = cachedTimeScale;
            IsGenerationComplete = true;
            GenerationCompleted?.Invoke();
        }

        private void ResetState()
        {
            villagePositions.Clear();
            townPositions.Clear();
            castlePositions.Clear();
            villagesByPosition.Clear();
            townsByPosition.Clear();
            castlesByPosition.Clear();
            grid = new Cell[mapWidth, mapHeight];
            landCellCount = fertileCellCount = villageCount = townCount = castleCount = 0;
            IsGenerationComplete = false;
        }

        private void EnsureBuffers()
        {
            int size = mapWidth * mapHeight;
            floorTileBuffer = floorTileBuffer?.Length == size ? floorTileBuffer : new TileBase[size];
            structureTileBuffer = structureTileBuffer?.Length == size ? structureTileBuffer : new TileBase[size];
            heightBuffer = heightBuffer?.Length == size ? heightBuffer : new float[size];
        }

        private IEnumerator GenerateTerrainCoroutine()
        {
            var rng = useFixedNoiseSeed ? new System.Random(noiseSeed) : new System.Random();
            float offsetX = noiseOffset.x + rng.Next(-65536, 65536);
            float offsetY = noiseOffset.y + rng.Next(-65536, 65536);
            float detailOffsetX = noiseOffset.x + rng.Next(-65536, 65536);
            float detailOffsetY = noiseOffset.y + rng.Next(-65536, 65536);

            int total = mapWidth * mapHeight;
            NativeArray<float> noise = new NativeArray<float>(total, Allocator.Persistent);
            var noiseJob = new ContinentNoiseJob
            {
                width = mapWidth,
                height = mapHeight,
                continentScale = continentScale,
                detailScale = detailNoiseScale,
                detailBlend = detailBlend,
                falloffPower = falloffPower,
                falloffStrength = falloffStrength,
                offsetX = offsetX,
                offsetY = offsetY,
                detailOffsetX = detailOffsetX,
                detailOffsetY = detailOffsetY,
                result = noise
            };

            JobHandle handle = noiseJob.Schedule(total, 256);
            while (!handle.IsCompleted)
            {
                yield return null;
            }
            handle.Complete();

            NativeArray<float> current = noise;
            NativeArray<float> temp = default;
            if (terrainBlurIterations > 0)
            {
                temp = new NativeArray<float>(total, Allocator.Persistent);
            }

            for (int i = 0; i < terrainBlurIterations; i++)
            {
                var blur = new BlurHeightJob
                {
                    width = mapWidth,
                    height = mapHeight,
                    source = current,
                    destination = temp
                };
                JobHandle blurHandle = blur.Schedule(total, 256);
                while (!blurHandle.IsCompleted)
                {
                    yield return null;
                }
                blurHandle.Complete();

                var swap = current;
                current = temp;
                temp = swap;
            }

            current.CopyTo(heightBuffer);

            bool noiseDisposed = false;
            if (current.IsCreated)
            {
                if (current == noise) noiseDisposed = true;
                current.Dispose();
            }
            if (temp.IsCreated && temp != current)
            {
                if (temp == noise) noiseDisposed = true;
                temp.Dispose();
            }
            if (!noiseDisposed && noise.IsCreated)
            {
                noise.Dispose();
            }

            Array.Clear(noiseHistogram, 0, noiseHistogram.Length);
            int idx = 0;
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++, idx++)
                {
                    float val = heightBuffer[idx];
                    int bucket = Mathf.Clamp(Mathf.RoundToInt(val * (noiseHistogram.Length - 1)), 0, noiseHistogram.Length - 1);
                    noiseHistogram[bucket]++;
                }
                if (y % 64 == 0)
                {
                    yield return null;
                }
            }

            float threshold = DetermineThreshold(mapWidth * mapHeight);
            NativeArray<int> gridArray = new NativeArray<int>(total, Allocator.Persistent);
            idx = 0;
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++, idx++)
                {
                    gridArray[idx] = heightBuffer[idx] >= threshold ? 1 : 0;
                }
                if (y % 64 == 0) yield return null;
            }

            for (int i = 0; i < smoothingIterations; i++)
            {
                NativeArray<int> next = new NativeArray<int>(total, Allocator.Persistent);
                var smooth = new SmoothingJob
                {
                    width = mapWidth,
                    height = mapHeight,
                    source = gridArray,
                    destination = next
                };
                JobHandle smoothHandle = smooth.Schedule(total, 256);
                while (!smoothHandle.IsCompleted) yield return null;
                smoothHandle.Complete();
                gridArray.Dispose();
                gridArray = next;
            }

            idx = 0;
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++, idx++)
                {
                    bool floor = gridArray[idx] == 1;
                    if (floor)
                    {
                        landCellCount++;
                        if (IsHeightFertile(heightBuffer[idx])) fertileCellCount++;
                    }
                    grid[x, y] = floor ? Cell.Floor : Cell.Wall;
                }
                if (y % 64 == 0) yield return null;
            }

            gridArray.Dispose();
        }

        private void CalculateCounts()
        {
            float landHundreds = Mathf.Max(1f, landCellCount / 100f);
            villageCount = Mathf.RoundToInt(Mathf.Clamp(landHundreds * villagesPerHundredLand, 20f, 400f) * densityMultiplier);
            townCount = Mathf.RoundToInt(Mathf.Clamp(villageCount / villagesPerTownRatio, 8f, 200f));
            castleCount = Mathf.RoundToInt(Mathf.Clamp(townCount / townsPerCastleRatio, 4f, 90f));
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

            NativeArray<int> floorMask = new NativeArray<int>(mapWidth * mapHeight, Allocator.Persistent);
            int idx = 0;
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++, idx++)
                {
                    floorMask[idx] = grid[x, y] == Cell.Floor && IsHeightFertile(heightBuffer[idx]) ? 1 : 0;
                }
                if (y % 64 == 0) yield return null;
            }

            int estimate = Mathf.Max(1, (mapWidth / stride + 2) * (mapHeight / stride + 2));
            NativeList<int> candidateList = new NativeList<int>(estimate, Allocator.Persistent);
            var candidateJob = new RegionCandidateJob
            {
                width = mapWidth,
                height = mapHeight,
                stride = stride,
                halfStride = halfStride,
                floorMask = floorMask,
                randomSeed = (uint)(noiseSeed == 0 ? 1 : noiseSeed) + 1337u,
                results = candidateList
            };

            JobHandle handle = candidateJob.Schedule();
            while (!handle.IsCompleted) yield return null;
            handle.Complete();
            floorMask.Dispose();

            var candidates = new List<Vector3Int>(candidateList.Length);
            for (int i = 0; i < candidateList.Length; i++)
            {
                int flat = candidateList[i];
                candidates.Add(new Vector3Int(flat % mapWidth, flat / mapWidth, 0));
            }
            candidateList.Dispose();

            var rng = new System.Random(noiseSeed + 97);
            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int swap = rng.Next(i + 1);
                (candidates[i], candidates[swap]) = (candidates[swap], candidates[i]);
            }

            int placed = 0;
            for (int i = 0; i < candidates.Count && placed < villageCount; i++)
            {
                var candidate = candidates[i];
                if (!IsValidVillageCandidate(candidate, buckets, bucketsX, bucketsY, cellSize, radiusSqr))
                    continue;

                if (TryPlaceVillage(candidate))
                {
                    placed++;
                    AddToBucket(candidate, buckets, bucketsX, bucketsY, cellSize);
                }

                if (i % 32 == 0) yield return null;
            }

            float relaxedRadiusSqr = Mathf.Max(1f, radiusSqr * 0.25f);
            for (int y = 1; y < mapHeight - 1 && placed < villageCount; y++)
            {
                for (int x = 1; x < mapWidth - 1 && placed < villageCount; x++)
                {
                    var candidate = new Vector3Int(x, y, 0);
                    if (!IsValidVillageCandidate(candidate, buckets, bucketsX, bucketsY, cellSize, relaxedRadiusSqr))
                        continue;

                    if (TryPlaceVillage(candidate))
                    {
                        placed++;
                        AddToBucket(candidate, buckets, bucketsX, bucketsY, cellSize);
                    }

                    if (((x + y) & 63) == 0) yield return null;
                }
            }

            for (int y = 1; y < mapHeight - 1 && placed < villageCount; y++)
            {
                for (int x = 1; x < mapWidth - 1 && placed < villageCount; x++)
                {
                    if (grid[x, y] != Cell.Floor || !IsHeightFertile(heightBuffer[x + y * mapWidth]))
                        continue;

                    if (TryPlaceVillage(new Vector3Int(x, y, 0)))
                    {
                        placed++;
                    }

                    if (((x + y) & 127) == 0) yield return null;
                }
            }
        }

        private IEnumerator CreateTownsCoroutine()
        {
            if (villagePositions.Count == 0)
            {
                yield break;
            }

            int targetTowns = Mathf.Clamp(townCount, 1, villagePositions.Count);
            System.Random rng = useFixedNoiseSeed ? new System.Random(noiseSeed + 741) : new System.Random();
            List<Vector3> centroids = InitializeCentroids(villagePositions, targetTowns, rng);
            var assignments = new List<Vector3Int>[centroids.Count];
            for (int i = 0; i < assignments.Length; i++) assignments[i] = new List<Vector3Int>();

            for (int iter = 0; iter < townLloydIterations; iter++)
            {
                foreach (var list in assignments) list.Clear();
                foreach (var village in villagePositions)
                {
                    int idx = FindClosestCentroid(centroids, village);
                    assignments[idx].Add(village);
                }
                for (int i = 0; i < centroids.Count; i++)
                {
                    if (assignments[i].Count > 0)
                    {
                        centroids[i] = ComputeAverage(assignments[i]);
                    }
                }
                yield return null;
            }

            float minDist = Mathf.Max(2f, townClusterRadius);
            float minDistSqr = minDist * minDist;
            for (int i = 0; i < assignments.Length && townPositions.Count < targetTowns; i++)
            {
                var cluster = assignments[i];
                if (cluster.Count == 0) continue;

                Vector3 centroid = ComputeAverage(cluster);
                Vector3Int desired = new Vector3Int(Mathf.RoundToInt(centroid.x), Mathf.RoundToInt(centroid.y), 0);
                if (!TryFindFloorCellNear(desired, out Vector3Int townCell, Mathf.CeilToInt(minDist)))
                {
                    townCell = cluster[0];
                }

                if (!IsFarEnough(townPositions, townCell, minDistSqr))
                    continue;

                PlaceTown(townCell);
                if ((i & 3) == 0) yield return null;
            }

            for (int i = 0; i < villagePositions.Count && townPositions.Count < targetTowns; i++)
            {
                Vector3Int candidate = villagePositions[i];
                if (!IsFarEnough(townPositions, candidate, minDistSqr))
                    continue;

                PlaceTown(candidate);
                if ((i & 7) == 0) yield return null;
            }
        }

        private IEnumerator CreateCastlesCoroutine()
        {
            if (townPositions.Count == 0)
            {
                yield break;
            }

            int targetCastles = Mathf.Clamp(castleCount, 1, townPositions.Count);
            System.Random rng = useFixedNoiseSeed ? new System.Random(noiseSeed + 963) : new System.Random();
            List<Vector3> centroids = InitializeCentroids(townPositions, targetCastles, rng);
            var assignments = new List<Vector3Int>[centroids.Count];
            for (int i = 0; i < assignments.Length; i++) assignments[i] = new List<Vector3Int>();

            for (int iter = 0; iter < castleLloydIterations; iter++)
            {
                foreach (var list in assignments) list.Clear();
                foreach (var town in townPositions)
                {
                    int idx = FindClosestCentroid(centroids, town);
                    assignments[idx].Add(town);
                }
                for (int i = 0; i < centroids.Count; i++)
                {
                    if (assignments[i].Count > 0)
                    {
                        centroids[i] = ComputeAverage(assignments[i]);
                    }
                }
                yield return null;
            }

            float minDist = Mathf.Max(2f, castleCoverageRadius);
            float minDistSqr = minDist * minDist;
            for (int i = 0; i < assignments.Length && castlePositions.Count < targetCastles; i++)
            {
                var cluster = assignments[i];
                if (cluster.Count == 0) continue;

                Vector3 centroid = ComputeAverage(cluster);
                Vector3Int desired = new Vector3Int(Mathf.RoundToInt(centroid.x), Mathf.RoundToInt(centroid.y), 0);
                if (!TryFindFloorCellNear(desired, out Vector3Int castleCell, Mathf.CeilToInt(minDist)))
                {
                    castleCell = cluster[0];
                }

                if (!IsFarEnough(castlePositions, castleCell, minDistSqr))
                    continue;

                PlaceCastle(castleCell);
                if ((i & 3) == 0) yield return null;
            }

            for (int i = 0; i < townPositions.Count && castlePositions.Count < targetCastles; i++)
            {
                Vector3Int candidate = townPositions[i];
                if (!IsFarEnough(castlePositions, candidate, minDistSqr))
                    continue;

                PlaceCastle(candidate);
                if ((i & 5) == 0) yield return null;
            }
        }

        private void ApplyTilemaps()
        {
            BoundsInt bounds = new BoundsInt(Vector3Int.zero, new Vector3Int(mapWidth, mapHeight, 1));
            int idx = 0;
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++, idx++)
                {
                    floorTileBuffer[idx] = grid[x, y] == Cell.Floor ? floorTile : wallTile;
                    structureTileBuffer[idx] = null;
                }
            }

            floorTilemap.ClearAllTiles();
            floorTilemap.SetTilesBlock(bounds, floorTileBuffer);

            foreach (var v in villagePositions)
            {
                floorTilemap.SetTile(v, floorTile);
                structureTilemap.SetTile(v, villageTile);
            }
            foreach (var t in townPositions)
            {
                structureTilemap.SetTile(t, townTile);
            }
            foreach (var c in castlePositions)
            {
                structureTilemap.SetTile(c, castleTile);
            }
        }

        private void CacheSettlementInstances()
        {
            villagesByPosition.Clear();
            townsByPosition.Clear();
            castlesByPosition.Clear();

            foreach (var position in villagePositions)
            {
                if (!villagesByPosition.ContainsKey(position))
                {
                    villagesByPosition[position] = new Village(position.x, position.y);
                }
            }

            foreach (var position in townPositions)
            {
                if (!townsByPosition.ContainsKey(position))
                {
                    townsByPosition[position] = new Town(position.x, position.y);
                }
            }

            foreach (var position in castlePositions)
            {
                if (!castlesByPosition.ContainsKey(position))
                {
                    castlesByPosition[position] = new Castle(position.x, position.y, CreateDefaultLord(position));
                }
            }

            SettlementEconomySystem.InitializeEconomy(villagesByPosition.Values, townsByPosition.Values, castlesByPosition.Values);
            SyncWorldRepositorySettlements();
            AssignFactionsToWorld();
        }

        private static global::Characters CreateDefaultLord(Vector3Int position)
        {
            return new global::Characters($"Lord {position.x}-{position.y}", 250, 120, 10);
        }

        private void SyncWorldRepositorySettlements()
        {
            var repository = WorldRepository;
            if (repository == null)
            {
                return;
            }

            if (repository.Villages != null)
            {
                repository.Villages.Clear();
                foreach (var village in villagesByPosition.Values)
                {
                    repository.Villages.Add(village);
                }
            }

            if (repository.Towns != null)
            {
                repository.Towns.Clear();
                foreach (var town in townsByPosition.Values)
                {
                    repository.Towns.Add(town);
                }
            }

            if (repository.Castles != null)
            {
                repository.Castles.Clear();
                foreach (var castle in castlesByPosition.Values)
                {
                    repository.Castles.Add(castle);
                }
            }

            repository.RebuildSpatialCaches();
        }

        private bool IsHeightFertile(float value) =>
            value >= fertileHeightMin && value <= fertileHeightMax;

        private void AssignFactionsToWorld()
        {
            var repository = WorldRepository;
            if (repository == null)
            {
                return;
            }

            // Prefer dedicated FactionMaker if present
            if (factionMaker != null)
            {
                factionMaker.AssignFactions(castlesByPosition, townsByPosition, villagesByPosition);
                return;
            }

            if (repository.Factions == null)
            {
                return;
            }

            repository.Factions.Clear();
            int factionCount = Mathf.Max(1, numberOfFactions);
            for (int i = 0; i < factionCount; i++)
            {
                var faction = new Faction($"Faction {i + 1}", Vector2.zero, i)
                {
                    Color = UnityEngine.Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.7f, 1f)
                };
                repository.Factions.Add(faction);
            }

            // Round-robin assign settlements
            void AssignToFaction(ISettlement settlement)
            {
                if (settlement == null)
                {
                    return;
                }
                int fid = settlement.PosX + settlement.PosY;
                fid = Mathf.Abs(fid) % factionCount;
                settlement.FactionId = fid;
                var f = repository.Factions[fid];
                switch (settlement)
                {
                    case Castle c:
                        f.Castles.Add(c);
                        break;
                    case Town t:
                        f.Towns.Add(t);
                        break;
                    case Village v:
                        f.Villages.Add(v);
                        break;
                }
            }

            foreach (var c in castlesByPosition.Values) AssignToFaction(c);
            foreach (var t in townsByPosition.Values) AssignToFaction(t);
            foreach (var v in villagesByPosition.Values) AssignToFaction(v);
        }

        #region placement helpers
        private bool TryPlaceVillage(Vector3Int cell)
        {
            if (grid[cell.x, cell.y] != Cell.Floor)
                return false;
            villagePositions.Add(cell);
            return true;
        }

        private void PlaceTown(Vector3Int cell)
        {
            townPositions.Add(cell);
        }

        private void PlaceCastle(Vector3Int cell)
        {
            castlePositions.Add(cell);
        }

        private bool TryFindFloorCellNear(Vector3Int origin, out Vector3Int result, int radius)
        {
            if (IsValidFloor(origin))
            {
                result = origin;
                return true;
            }

            for (int r = 1; r <= radius; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        Vector3Int candidate = new Vector3Int(origin.x + dx, origin.y + dy, 0);
                        if (IsValidFloor(candidate))
                        {
                            result = candidate;
                            return true;
                        }
                    }
                }
            }

            result = origin;
            return false;
        }

        private bool IsValidFloor(Vector3Int cell)
        {
            return cell.x > 1 && cell.x < mapWidth - 1 && cell.y > 1 && cell.y < mapHeight - 1 && grid[cell.x, cell.y] == Cell.Floor;
        }

        private static void AddToBucket(Vector3Int cell, List<Vector3Int>[] buckets, int bucketsX, int bucketsY, float cellSize)
        {
            int bx = Mathf.Clamp(Mathf.FloorToInt(cell.x / cellSize), 0, bucketsX - 1);
            int by = Mathf.Clamp(Mathf.FloorToInt(cell.y / cellSize), 0, bucketsY - 1);
            int idx = bx + by * bucketsX;
            (buckets[idx] ??= new List<Vector3Int>()).Add(cell);
        }

        private bool IsValidVillageCandidate(Vector3Int cell, List<Vector3Int>[] buckets, int bucketsX, int bucketsY, float cellSize, float minDistSqr)
        {
            if (cell.x < 1 || cell.x >= mapWidth - 1 || cell.y < 1 || cell.y >= mapHeight - 1)
                return false;

            int bx = Mathf.Clamp(Mathf.FloorToInt(cell.x / cellSize), 0, bucketsX - 1);
            int by = Mathf.Clamp(Mathf.FloorToInt(cell.y / cellSize), 0, bucketsY - 1);
            for (int nx = bx - 1; nx <= bx + 1; nx++)
            {
                if (nx < 0 || nx >= bucketsX) continue;
                for (int ny = by - 1; ny <= by + 1; ny++)
                {
                    if (ny < 0 || ny >= bucketsY) continue;
                    var list = buckets[nx + ny * bucketsX];
                    if (list == null) continue;
                    foreach (var other in list)
                    {
                        if ((other - cell).sqrMagnitude < minDistSqr)
                            return false;
                    }
                }
            }
            return true;
        }

        private static List<Vector3> InitializeCentroids(List<Vector3Int> points, int target, System.Random rng)
        {
            target = Mathf.Max(1, target);
            List<Vector3> centroids = new List<Vector3>(target);
            if (points.Count == 0)
                return centroids;

            float step = points.Count / (float)target;
            for (int i = 0; i < target; i++)
            {
                int index = Mathf.Clamp(Mathf.RoundToInt(i * step + rng.Next(0, Mathf.Max(1, (int)step))), 0, points.Count - 1);
                centroids.Add(points[index]);
            }
            return centroids;
        }

        private static int FindClosestCentroid(List<Vector3> centroids, Vector3Int point)
        {
            int best = 0;
            float dist = float.MaxValue;
            for (int i = 0; i < centroids.Count; i++)
            {
                float sq = (centroids[i] - (Vector3)point).sqrMagnitude;
                if (sq < dist)
                {
                    dist = sq;
                    best = i;
                }
            }
            return best;
        }

        private static Vector3 ComputeAverage(List<Vector3Int> points)
        {
            Vector3 sum = Vector3.zero;
            for (int i = 0; i < points.Count; i++)
            {
                sum += (Vector3)points[i];
            }
            return sum / Mathf.Max(1, points.Count);
        }

        private static bool IsFarEnough(List<Vector3Int> existing, Vector3Int candidate, float minSqr)
        {
            for (int i = 0; i < existing.Count; i++)
            {
                if ((existing[i] - candidate).sqrMagnitude < minSqr)
                    return false;
            }
            return true;
        }

        private float DetermineThreshold(int totalCells)
        {
            int targetLandCells = Mathf.Clamp(Mathf.RoundToInt(totalCells * 0.65f), 1, totalCells);
            int cumulative = 0;
            for (int bucket = noiseHistogram.Length - 1; bucket >= 0; bucket--)
            {
                cumulative += noiseHistogram[bucket];
                if (cumulative >= targetLandCells)
                {
                    return bucket / (float)(noiseHistogram.Length - 1);
                }
            }
            return 0.5f;
        }
        #endregion

        #region jobs
        private struct ContinentNoiseJob : IJobParallelFor
        {
            public int width;
            public int height;
            public float continentScale;
            public float detailScale;
            public float detailBlend;
            public float falloffStrength;
            public float falloffPower;
            public float offsetX;
            public float offsetY;
            public float detailOffsetX;
            public float detailOffsetY;
            public NativeArray<float> result;

            public void Execute(int index)
            {
                int x = index % width;
                int y = index / width;
                float2 cSample = new float2((x + offsetX) * continentScale, (y + offsetY) * continentScale);
                float ridge = 1f - math.abs(Unity.Mathematics.noise.snoise(cSample));
                float2 dSample = new float2((x + detailOffsetX) * detailScale, (y + detailOffsetY) * detailScale);
                float detail = (Unity.Mathematics.noise.snoise(dSample) + 1f) * 0.5f;

                float nx = (x / (float)(width - 1)) * 2f - 1f;
                float ny = (y / (float)(height - 1)) * 2f - 1f;
                float dist = math.sqrt(nx * nx + ny * ny);
                float falloff = math.pow(math.saturate(dist), falloffPower) * falloffStrength;

                float value = math.clamp(ridge - falloff + detail * detailBlend, 0f, 1f);
                result[index] = value;
            }
        }

        private struct BlurHeightJob : IJobParallelFor
        {
            public int width;
            public int height;
            [ReadOnly] public NativeArray<float> source;
            public NativeArray<float> destination;

            public void Execute(int index)
            {
                int x = index % width;
                int y = index / width;
                float sum = 0f;
                int count = 0;
                for (int dy = -1; dy <= 1; dy++)
                {
                    int ny = y + dy;
                    if (ny < 0 || ny >= height) continue;
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx;
                        if (nx < 0 || nx >= width) continue;
                        sum += source[nx + ny * width];
                        count++;
                    }
                }
                destination[index] = count > 0 ? sum / count : source[index];
            }
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

                int neighbors = 0;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        neighbors += source[(x + dx) + (y + dy) * width];
                    }
                }
                destination[index] = neighbors >= 5 ? 1 : 0;
            }
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
        #endregion
    }
}
