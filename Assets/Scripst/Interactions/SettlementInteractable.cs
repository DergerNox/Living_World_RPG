// SettlementInteractable.cs
using UnityEngine;

/// <summary>
/// Attach this to your Castle, Town, or Village prefab.
/// Requires a Collider2D set as trigger.
/// Implements IInteractable to respond when the player interacts.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SettlementInteractable : MonoBehaviour, IInteractable
{
    public enum SettlementType { Castle, Town, Village }
    public SettlementType Type;
    public string SettlementName;

    private void Reset()
    {
        // ensure the collider is a trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    public void Interact(GameObject interactor)
    {
        // Replace with your own UI logic: open dialogs, trade screens, etc.
        Debug.Log($"{interactor.name} interacted with {Type} '{SettlementName}' at {transform.position}.");
        // Example: open a UI panel
        // UIManager.Instance.ShowSettlementPanel(Type, SettlementName);
    }
}