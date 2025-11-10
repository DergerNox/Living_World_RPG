using UnityEngine;

namespace LivingWorldRPG.Camera
{
/// <summary>
/// Simple smooth camera follow for 2D or 3D setups.
/// Attach this to your Main Camera and assign the target (e.g. Player).
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Tooltip("The transform the camera will follow (e.g. the Player).")]
    [SerializeField] private Transform target;
    
    [Tooltip("Offset from the target's position.")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    
    [Tooltip("Lerp smoothing speed; 0 = instant, 1 = snappy.")]
    [SerializeField]
    [Range(0f, 1f)] private float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target == null)
            return;

        // Desired camera position based on target + offset
        Vector3 desiredPosition = target.position + offset;
        // Smoothly interpolate between current and desired
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
}
