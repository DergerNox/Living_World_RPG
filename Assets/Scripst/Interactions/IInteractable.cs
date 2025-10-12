// IInteractable.cs
using UnityEngine;

/// <summary>
/// Interface for any object the player (or NPC) can interact with.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Called when an interaction is triggered (e.g. player presses E).
    /// </summary>
    void Interact(GameObject interactor);
}