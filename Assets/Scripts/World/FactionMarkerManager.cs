using System.Collections.Generic;
using UnityEngine;

namespace LivingWorldRPG.World
{
    /// <summary>
    /// Spawns simple faction markers above settlements so faction ownership is visible on the map.
    /// </summary>
    public class FactionMarkerManager : MonoBehaviour
    {
        [SerializeField] private WalkerGenerator generator;
        [SerializeField] private GameObject markerPrefab;
        [SerializeField] private Vector3 markerOffset = new Vector3(0.5f, 0.9f, 0f);
        [SerializeField, Min(0.01f)] private float markerScale = 0.15f;

        private readonly Dictionary<ISettlement, GameObject> markers = new Dictionary<ISettlement, GameObject>();
        private IWorldSimulationService worldSimulation;

        private void Awake()
        {
            if (generator == null)
            {
                generator = FindFirstObjectByType<WalkerGenerator>();
            }

            ServiceLocator.TryResolve(out worldSimulation);
        }

        private void OnEnable()
        {
            if (generator != null && generator.IsGenerationComplete)
            {
                BuildMarkers();
            }
            else if (generator != null)
            {
                generator.GenerationCompleted += BuildMarkers;
            }

            if (worldSimulation != null)
            {
                worldSimulation.WorldGenerated += BuildMarkers;
            }
        }

        private void OnDisable()
        {
            if (generator != null)
            {
                generator.GenerationCompleted -= BuildMarkers;
            }

            if (worldSimulation != null)
            {
                worldSimulation.WorldGenerated -= BuildMarkers;
            }

            ClearMarkers();
        }

        private void BuildMarkers()
        {
            ClearMarkers();

            IWorldStateRepository state = worldSimulation?.State;
            if (state == null)
            {
                return;
            }

            SpawnForSettlements(state.Castles);
            SpawnForSettlements(state.Towns);
            SpawnForSettlements(state.Villages);
        }

        private void SpawnForSettlements<T>(IEnumerable<T> settlements) where T : ISettlement
        {
            if (settlements == null)
            {
                return;
            }

            foreach (var s in settlements)
            {
                if (s == null || markers.ContainsKey(s))
                {
                    continue;
                }
                SpawnMarker(s);
            }
        }

        private void SpawnMarker(ISettlement settlement)
        {
            GameObject marker = CreateMarkerInstance(settlement);
            markers[settlement] = marker;
        }

        private GameObject CreateMarkerInstance(ISettlement settlement)
        {
            Vector3 worldPos = new Vector3(settlement.PosX, settlement.PosY, 0f) + markerOffset;
            GameObject go = markerPrefab != null ? Instantiate(markerPrefab, worldPos, Quaternion.identity, transform) : CreateDefaultMarker(worldPos);

            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = ResolveFactionColor(settlement.FactionId);
                sr.sortingOrder = 50;
                sr.transform.localScale = Vector3.one * markerScale;
            }
            else
            {
                go.transform.localScale = Vector3.one * markerScale;
            }

            return go;
        }

        private GameObject CreateDefaultMarker(Vector3 position)
        {
            GameObject go = new GameObject("FactionMarker", typeof(SpriteRenderer));
            go.transform.SetParent(transform, false);
            go.transform.position = position;
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = GetDefaultSprite();
            sr.color = Color.white;
            return go;
        }

        private Sprite GetDefaultSprite()
        {
            // Try loading a common UI sprite; if not present, fall back to a tiny generated sprite.
            Sprite uiSprite = UnityEngine.Resources.Load<Sprite>("UI/Skin/UISprite");
            if (uiSprite != null)
            {
                return uiSprite;
            }

            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
        }

        private Color ResolveFactionColor(int factionId)
        {
            var repo = worldSimulation?.State;
            if (repo == null || factionId < 0 || factionId >= repo.Factions.Count)
            {
                return Color.white;
            }
            return repo.Factions[factionId].Color;
        }

        private void ClearMarkers()
        {
            foreach (var kvp in markers)
            {
                if (kvp.Value != null)
                {
                    DestroyImmediate(kvp.Value);
                }
            }
            markers.Clear();
        }
    }
}
