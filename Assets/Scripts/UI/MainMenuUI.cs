using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "Game_World";
    [SerializeField] private Font fallbackFont;

    private Button loadGameButton;
    private Text loadStatusText;
    private bool uiBuilt;

    private void Awake()
    {
        if (fallbackFont == null)
        {
            fallbackFont = UnityEngine.Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        EnsureCameraExists();
        EnsureEventSystemExists();
        BuildUI();
        RefreshLoadState();
    }

    private void OnEnable()
    {
        RefreshLoadState();
    }

    private void EnsureCameraExists()
    {
        if (Camera.main != null)
        {
            return;
        }

        GameObject cameraGo = new GameObject("Main Camera");
        Camera cameraComponent = cameraGo.AddComponent<Camera>();
        cameraGo.tag = "MainCamera";
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = new Color(0.04f, 0.05f, 0.08f);
        cameraComponent.orthographic = true;
        cameraComponent.nearClipPlane = -10f;
        cameraComponent.farClipPlane = 10f;
        cameraGo.AddComponent<AudioListener>();
    }

    private void EnsureEventSystemExists()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        EventSystem existing = Object.FindFirstObjectByType<EventSystem>();
        if (existing != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private void BuildUI()
    {
        if (uiBuilt)
        {
            return;
        }

        GameObject canvasGo = new GameObject("MainMenuCanvas", typeof(RectTransform));
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.offsetMin = Vector2.zero;
        canvasRect.offsetMax = Vector2.zero;

        GameObject panel = new GameObject("Panel", typeof(RectTransform));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.SetParent(canvasRect, false);
        panelRect.sizeDelta = new Vector2(520f, 520f);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.02f, 0.02f, 0.03f, 0.85f);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 24f;
        layout.padding = new RectOffset(32, 32, 32, 32);

        ContentSizeFitter fitter = panel.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Text title = CreateTextElement(panel.transform, "Living World RPG", 48, FontStyle.Bold);
        LayoutElement titleLayout = title.gameObject.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 80f;

        Button newGameButton = CreateMenuButton(panel.transform, "New Game", StartNewGame);
        loadGameButton = CreateMenuButton(panel.transform, "Load Game", LoadSavedGame);
        loadStatusText = CreateInfoText(panel.transform, "No save data found.");
        loadStatusText.gameObject.SetActive(false);
        Button exitButton = CreateMenuButton(panel.transform, "Exit", ExitGame);

        // Keep button order but avoid unused variable warning
        _ = newGameButton;
        _ = exitButton;

        uiBuilt = true;
    }

    private Text CreateTextElement(Transform parent, string content, int fontSize, FontStyle fontStyle = FontStyle.Normal)
    {
        GameObject textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(parent, false);
        RectTransform rect = textGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, fontSize + 10);

        Text text = textGo.AddComponent<Text>();
        text.text = content;
        text.font = fallbackFont;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = Color.white;

        return text;
    }

    private Text CreateInfoText(Transform parent, string message)
    {
        Text text = CreateTextElement(parent, message, 22);
        text.color = new Color(1f, 0.8f, 0.4f, 1f);
        LayoutElement element = text.gameObject.AddComponent<LayoutElement>();
        element.preferredHeight = 32f;
        return text;
    }

    private Button CreateMenuButton(Transform parent, string label, UnityAction onClick)
    {
        GameObject buttonGo = new GameObject(label + " Button", typeof(RectTransform));
        buttonGo.transform.SetParent(parent, false);
        RectTransform rect = buttonGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 60f);

        Image image = buttonGo.AddComponent<Image>();
        image.color = new Color(0.17f, 0.25f, 0.36f, 0.9f);

        Button button = buttonGo.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.17f, 0.25f, 0.36f, 0.9f);
        colors.highlightedColor = new Color(0.26f, 0.38f, 0.55f, 0.95f);
        colors.pressedColor = new Color(0.1f, 0.16f, 0.24f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.17f, 0.25f, 0.36f, 0.3f);
        button.colors = colors;
        button.onClick.AddListener(onClick);

        LayoutElement layout = buttonGo.AddComponent<LayoutElement>();
        layout.preferredHeight = 70f;
        layout.minHeight = 60f;
        layout.flexibleWidth = 1f;

        GameObject labelGo = new GameObject("Label", typeof(RectTransform));
        labelGo.transform.SetParent(buttonGo.transform, false);
        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Text text = labelGo.AddComponent<Text>();
        text.text = label;
        text.font = fallbackFont;
        text.fontSize = 32;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        return button;
    }

    private void RefreshLoadState()
    {
        bool hasSave = SaveLoadManager.HasSaveData();
        if (loadGameButton != null)
        {
            loadGameButton.interactable = hasSave;
        }

        if (loadStatusText != null)
        {
            loadStatusText.gameObject.SetActive(!hasSave);
        }
    }

    private void StartNewGame()
    {
        GameSessionBootstrapper.StartNewGame(gameplaySceneName);
    }

    private void LoadSavedGame()
    {
        if (!SaveLoadManager.HasSaveData())
        {
            RefreshLoadState();
            return;
        }

        GameSessionBootstrapper.LoadSavedGame(gameplaySceneName);
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
