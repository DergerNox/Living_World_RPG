using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Main menu controller that relies on a prefabbed Canvas.
/// Wire the buttons/text references in the inspector so artists
/// can tweak visuals without touching code.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "Game_World";

    [Header("UI References")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Text loadStatusText;

    [Header("Optional Events")]
    [SerializeField] private UnityEvent onMenuShown;
    [SerializeField] private UnityEvent onMenuHidden;

    private void Awake()
    {
        BindButton(newGameButton, StartNewGame, nameof(newGameButton));
        BindButton(loadGameButton, LoadSavedGame, nameof(loadGameButton));
        BindButton(exitButton, ExitGame, nameof(exitButton));
        RefreshLoadState();
    }

    private void OnEnable()
    {
        RefreshLoadState();
        onMenuShown?.Invoke();
    }

    private void OnDisable()
    {
        onMenuHidden?.Invoke();
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

    private static void BindButton(Button button, UnityAction callback, string fieldName)
    {
        if (button == null)
        {
            Debug.LogWarning($"MainMenuUI is missing a reference for '{fieldName}'.");
            return;
        }

        button.onClick.RemoveListener(callback);
        button.onClick.AddListener(callback);
    }
}
