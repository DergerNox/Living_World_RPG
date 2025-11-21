#if UNITY_EDITOR
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LivingWorldRPG.UI
{
    /// <summary>
    /// Editor helper that spawns a fully wired settlement UI under this object.
    /// Use the context menu "Rebuild Settlement UI" to create the panel from scratch.
    /// </summary>
    [ExecuteInEditMode]
    public sealed class SettlementUIBuilder : MonoBehaviour
    {
        [ContextMenu("Rebuild Settlement UI")]
        private void RebuildUI()
        {
            if (!Application.isPlaying)
            {
                Undo.RegisterFullObjectHierarchyUndo(gameObject, "Rebuild Settlement UI");
            }

            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }

            CreateUI();
        }

        private void CreateUI()
        {
            var panel = CreatePanel("SettlementUI", new Vector2(520f, 720f), new Color(0f, 0f, 0f, 0.9f));
            var manager = panel.AddComponent<SettlementUIManager>();

            var title = CreateTMP("Title", panel.transform, "Village", 34, TextAlignmentOptions.Left);
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30f), new Vector2(480f, 48f));

            var exitButton = CreateButton("ExitButton", panel.transform, "X", new Vector2(220f, -30f));
            exitButton.GetComponent<RectTransform>().sizeDelta = new Vector2(50f, 40f);

            var factionName = CreateTMP("FactionName", panel.transform, "Faction: None", 24, TextAlignmentOptions.Left);
            SetRect(factionName.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -70f), new Vector2(480f, 36f));

            var tradeButton = CreateButton("TradeButton", panel.transform, "Trade", new Vector2(0, -90f));
            var recruitButton = CreateButton("RecruitButton", panel.transform, "Recruit", new Vector2(0, -150f));

            var economyText = CreateTMP("EconomySummary", panel.transform, "Level 1 village\nStock: --", 28, TextAlignmentOptions.Left);
            SetRect(economyText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -230f), new Vector2(480f, 140f));

            var factionText = CreateTMP("FactionNeed", panel.transform, "No faction data", 26, TextAlignmentOptions.Left);
            SetRect(factionText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -380f), new Vector2(480f, 80f));

            var productionScroll = CreateScrollRect(panel.transform, "ProductionScroll", new Vector2(0, -540f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(480f, 220f), out var productionTMP);

            var recruitmentPanel = CreatePanel("RecruitmentPanel", new Vector2(520f, 620f), new Color(0f, 0f, 0f, 0.85f));
            recruitmentPanel.transform.SetParent(transform, false);
            recruitmentPanel.SetActive(false);

            var recruitmentTitle = CreateTMP("RecruitmentTitle", recruitmentPanel.transform, "Recruitment", 32, TextAlignmentOptions.Left);
            SetRect(recruitmentTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30f), new Vector2(460f, 48f));

            var troopScroll = CreateScrollRect(recruitmentPanel.transform, "TroopScroll", new Vector2(0, -320f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(480f, 500f), out _);
            troopScroll.content.gameObject.AddComponent<VerticalLayoutGroup>().childForceExpandHeight = false;
            var troopCardTemplate = CreateButton("TroopCardTemplate", troopScroll.content, "Troop", Vector2.zero);
            troopCardTemplate.GetComponent<RectTransform>().sizeDelta = new Vector2(420f, 60f);
            troopCardTemplate.gameObject.SetActive(false);

            var cancelRecruitBtn = CreateButton("CancelRecruitButton", recruitmentPanel.transform, "Cancel", new Vector2(0, -560f));
            cancelRecruitBtn.onClick.AddListener(() => manager.OnCloseRecruitment());

            var selectionPanel = CreatePanel("TroopSelectionPanel", new Vector2(540f, 520f), new Color(0f, 0f, 0f, 0.92f));
            selectionPanel.transform.SetParent(transform, false);
            selectionPanel.SetActive(false);

            var selectionScript = selectionPanel.AddComponent<TroopSelectionPanel>();
            selectionScript.settlementPanel = panel;
            selectionScript.recruitmentPanel = recruitmentPanel;

            selectionScript.troopNameText = CreateTMP("TroopName", selectionPanel.transform, "Troop", 32, TextAlignmentOptions.Left);
            SetRect(selectionScript.troopNameText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30f), new Vector2(480f, 48f));

            selectionScript.troopImage = CreateImage("TroopPortrait", selectionPanel.transform, new Vector2(120f, 120f), new Vector2(-180f, -150f));

            selectionScript.unitCostText = CreateTMP("UnitCost", selectionPanel.transform, "Cost per unit: 0", 26, TextAlignmentOptions.Left);
            SetRect(selectionScript.unitCostText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(100f, -160f), new Vector2(320f, 60f));

            selectionScript.quantitySlider = CreateSlider("QuantitySlider", selectionPanel.transform, new Vector2(0, -260f), new Vector2(400f, 32f));
            selectionScript.quantityInput = CreateTMPInput("QuantityInput", selectionPanel.transform, new Vector2(0, -320f), new Vector2(200f, 50f));
            selectionScript.totalCostText = CreateTMP("TotalCost", selectionPanel.transform, "Total Cost: 0", 28, TextAlignmentOptions.Left);
            SetRect(selectionScript.totalCostText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -380f), new Vector2(400f, 40f));

            selectionScript.confirmButton = CreateButton("ConfirmButton", selectionPanel.transform, "Confirm", new Vector2(-100f, -450f));
            selectionScript.cancelButton = CreateButton("CancelButton", selectionPanel.transform, "Cancel", new Vector2(100f, -450f));

            selectionScript.troopImage.color = Color.white;
            selectionScript.gameObject.SetActive(false);

            // Add listeners
            selectionScript.quantitySlider.onValueChanged.AddListener(selectionScript.OnSliderChanged);
            selectionScript.quantityInput.onSubmit.AddListener(selectionScript.OnInputFieldChanged);
            selectionScript.quantityInput.onEndEdit.AddListener(selectionScript.OnInputFieldChanged);
            selectionScript.confirmButton.onClick.AddListener(selectionScript.OnConfirm);
            selectionScript.cancelButton.onClick.AddListener(selectionScript.Close);

            var recruitmentLayout = recruitmentPanel.AddComponent<VerticalLayoutGroup>();
            recruitmentLayout.childControlHeight = false;
            recruitmentLayout.childControlWidth = false;
            recruitmentLayout.enabled = false; // we position manually.

            // Assign to manager via reflection
            Assign(manager, "uiPanel", panel);
            Assign(manager, "recruitmentPanel", recruitmentPanel);
            Assign(manager, "troopSelectionPanel", selectionPanel);
            Assign(manager, "troopSelectionPanelScript", selectionScript);
            Assign(manager, "settlementNameText", title);
            Assign(manager, "recruitButton", recruitButton);
            Assign(manager, "tradeButton", tradeButton);
            Assign(manager, "exitButton", exitButton);
            Assign(manager, "economySummaryText", economyText);
            Assign(manager, "factionNameText", factionName);
            Assign(manager, "factionNeedText", factionText);
            Assign(manager, "productionDetailsText", productionTMP);
            Assign(manager, "troopContainer", troopScroll.content);
            Assign(manager, "troopCardPrefab", troopCardTemplate.gameObject);

            tradeButton.onClick.AddListener(manager.OnTradeButton);
            recruitButton.onClick.AddListener(manager.OnRecruitButton);
            exitButton.onClick.AddListener(manager.OnExitButton);

            panel.SetActive(false);
            recruitmentPanel.SetActive(false);
            selectionPanel.SetActive(false);
        }

        private GameObject CreatePanel(string name, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = color;
            return go;
        }

        private TextMeshProUGUI CreateTMP(string name, Transform parent, string text, int size, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.color = Color.white;
            return tmp;
        }

        private Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(320f, 48f);
            rect.anchoredPosition = anchoredPos;

            var image = go.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.9f);
            var button = go.GetComponent<Button>();

            var text = CreateTMP("Label", go.transform, label, 26, TextAlignmentOptions.Center);
            SetRect(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300f, 36f));

            return button;
        }

        private Image CreateImage(string name, Transform parent, Vector2 size, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;
            var img = go.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.25f);
            return img;
        }

        private TMP_InputField CreateTMPInput(string name, Transform parent, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TMP_InputField));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;
            var img = go.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.9f);

            var textArea = new GameObject("Text Area", typeof(RectTransform));
            textArea.transform.SetParent(go.transform, false);
            var textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10f, 6f);
            textAreaRect.offsetMax = new Vector2(-10f, -6f);

            var text = CreateTMP("Text", textArea.transform, "", 26, TextAlignmentOptions.Left);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;

            var placeholder = CreateTMP("Placeholder", textArea.transform, "Amount", 24, TextAlignmentOptions.Left);
            placeholder.color = new Color(1f, 1f, 1f, 0.5f);
            placeholder.rectTransform.anchorMin = Vector2.zero;
            placeholder.rectTransform.anchorMax = Vector2.one;
            placeholder.rectTransform.offsetMin = Vector2.zero;
            placeholder.rectTransform.offsetMax = Vector2.zero;

            var input = go.GetComponent<TMP_InputField>();
            input.textViewport = textAreaRect;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.contentType = TMP_InputField.ContentType.IntegerNumber;
            return input;
        }

        private Slider CreateSlider(string name, Transform parent, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Slider));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;

            var background = go.GetComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.4f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.offsetMin = new Vector2(10f, 0f);
            fillAreaRect.offsetMax = new Vector2(-10f, 0f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fill.GetComponent<Image>().color = new Color(0.2f, 0.6f, 1f, 0.8f);

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10f, 0f);
            handleAreaRect.offsetMax = new Vector2(-10f, 0f);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20f, 50f);
            var handleImage = handle.GetComponent<Image>();
            handleImage.color = Color.white;

            var slider = go.GetComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 1f;
            slider.maxValue = 100f;
            slider.wholeNumbers = true;

            return slider;
        }

        private ScrollRect CreateScrollRect(Transform parent, string name, Vector2 anchoredPos, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, out TextMeshProUGUI textContent)
        {
            var scrollGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
            scrollGO.transform.SetParent(parent, false);
            var rect = scrollGO.GetComponent<RectTransform>();
            SetRect(rect, anchorMin, anchorMax, new Vector2(0.5f, 1f), anchoredPos, size);
            var bg = scrollGO.GetComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0.08f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(scrollGO.transform, false);
            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(4f, 4f);
            viewportRect.offsetMax = new Vector2(-4f, -4f);

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0f, 1f);
            contentRect.offsetMin = new Vector2(0f, 0f);
            contentRect.offsetMax = new Vector2(0f, 0f);

            textContent = content.AddComponent<TextMeshProUGUI>();
            textContent.text = "";
            textContent.fontSize = 26;
            textContent.color = Color.white;
            textContent.alignment = TextAlignmentOptions.TopLeft;
            textContent.textWrappingMode = TextWrappingModes.Normal;
            
            var scrollRect = scrollGO.GetComponent<ScrollRect>();
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            return scrollRect;
        }

        private void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;
        }

        private void SetRect(RectTransform rect, Vector2 anchoredPos, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;
        }

        private void Assign(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"SettlementUIBuilder: Could not find field '{fieldName}' on {target.GetType().Name}.");
            }
        }
    }
}
#endif
