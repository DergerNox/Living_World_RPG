using UnityEngine;

namespace LivingWorldRPG.UI
{
    /// <summary>
    /// Lightweight replacement for a vertical layout group that keeps the settlement panel tidy
    /// without relying on Unity's automatic sizing (which often collapsed our elements).
    /// </summary>
    [ExecuteAlways]
    public sealed class SettlementUILayout : MonoBehaviour
    {
        [System.Serializable]
        public struct LayoutSection
        {
            public RectTransform rect;
            [Min(0f)] public float preferredHeight;
            [Min(0f)] public float minimumHeight;
            public bool flexibleHeight;
        }

        [SerializeField] private RectTransform container;
        [SerializeField] private float padding = 20f;
        [SerializeField] private float spacing = 12f;
        [SerializeField] private LayoutSection[] sections = System.Array.Empty<LayoutSection>();

        private void OnEnable() => ApplyLayout();
        private void OnValidate() => ApplyLayout();
        private void OnRectTransformDimensionsChange() => ApplyLayout();

        public void ApplyLayout()
        {
            if (container == null || sections == null || sections.Length == 0)
            {
                return;
            }

            float totalHeight = container.rect.height;
            float usableHeight = Mathf.Max(0f, totalHeight - padding * 2f - spacing * (sections.Length - 1));

            float fixedHeight = 0f;
            int flexibleCount = 0;
            foreach (var section in sections)
            {
                if (section.rect == null)
                {
                    continue;
                }

                if (section.flexibleHeight)
                {
                    flexibleCount++;
                }
                else
                {
                    fixedHeight += Mathf.Max(section.minimumHeight, section.preferredHeight);
                }
            }

            float flexibleHeight = 0f;
            if (flexibleCount > 0)
            {
                flexibleHeight = Mathf.Max(0f, (usableHeight - fixedHeight) / flexibleCount);
            }

            float currentY = padding;
            foreach (var section in sections)
            {
                if (section.rect == null)
                {
                    continue;
                }

                float targetHeight = section.flexibleHeight
                    ? Mathf.Max(section.minimumHeight, flexibleHeight)
                    : Mathf.Max(section.minimumHeight, section.preferredHeight);

                // Stretch horizontally within the container and position from the top.
                section.rect.anchorMin = new Vector2(0f, 1f);
                section.rect.anchorMax = new Vector2(1f, 1f);
                section.rect.pivot = new Vector2(0.5f, 1f);
                section.rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, padding, container.rect.width - padding * 2f);
                section.rect.anchoredPosition = new Vector2(0f, -currentY);
                section.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);

                currentY += targetHeight + spacing;
            }
        }
    }
}
