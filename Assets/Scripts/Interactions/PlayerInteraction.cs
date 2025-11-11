// PlayerInteraction.cs
using UnityEngine;

/// <summary>
/// Attach this to your Player GameObject. Detects IInteractable in range and triggers interaction on key press.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Tooltip("Key used to interact with objects.")]
    public KeyCode interactKey = KeyCode.E;

    [Tooltip("Radius in world units to search for interactables.")]
    public float interactRadius = 1f;

    [Tooltip("LayerMask for interactable objects (e.g. assign a layer 'Interactable').")]
    public LayerMask interactableLayer;

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            // find all colliders within radius
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactableLayer);
            foreach (var hit in hits)
            {
                var interactable = hit.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(gameObject);
                    break; // only interact with the first one
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // visualize interaction radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
