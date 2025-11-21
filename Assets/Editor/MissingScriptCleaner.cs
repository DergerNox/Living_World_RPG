using UnityEditor;
using UnityEngine;

/// <summary>
/// Small editor helpers to find and remove missing script references in scenes/assets.
/// Use the Tools menu to clean the active scene or current selection.
/// </summary>
public static class MissingScriptCleaner
{
    [MenuItem("Tools/Cleanup/Remove Missing Scripts In Scene")]
    public static void RemoveMissingInScene()
    {
        int total = 0;
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (go.scene.isLoaded == false)
            {
                continue;
            }
            total += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        }
        Debug.Log($"Removed {total} missing script references in the active scene.");
    }

    [MenuItem("Tools/Cleanup/Remove Missing Scripts In Selection")]
    public static void RemoveMissingInSelection()
    {
        int total = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            total += RemoveMissingRecursive(go);
        }
        Debug.Log($"Removed {total} missing script references from selection.");
    }

    private static int RemoveMissingRecursive(GameObject go)
    {
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        foreach (Transform child in go.transform)
        {
            removed += RemoveMissingRecursive(child.gameObject);
        }
        return removed;
    }
}
