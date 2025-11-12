using UnityEngine.SceneManagement;

/// <summary>
/// Simple static helper that remembers whether the next gameplay scene load
/// should restore a save file.
/// </summary>
public static class GameSessionBootstrapper
{
    private static bool loadFromSave;

    public static void StartNewGame(string sceneName)
    {
        loadFromSave = false;
        SceneManager.LoadScene(sceneName);
    }

    public static void LoadSavedGame(string sceneName)
    {
        loadFromSave = true;
        SceneManager.LoadScene(sceneName);
    }

    public static bool ConsumeLoadRequest()
    {
        bool shouldLoad = loadFromSave;
        loadFromSave = false;
        return shouldLoad;
    }
}
