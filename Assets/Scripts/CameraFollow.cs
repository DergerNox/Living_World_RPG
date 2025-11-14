using UnityEngine;

namespace LivingWorldRPG.Camera
{
/// <summary>
/// Smooth camera follow behaviour with optional auto-discovery of the player.
/// Attach this to your Main Camera; it will latch onto the configured target or the first Player it finds.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Tooltip("The transform the camera will follow (e.g. the Player).")]
    [SerializeField] private Transform target;

    [Tooltip("Automatically search for a PlayerController or object tagged 'Player' if no target is assigned.")]
    [SerializeField] private bool autoFindPlayer = true;

    [Tooltip("Offset from the target's position.")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Tooltip("Lerp smoothing speed; 0 = instant, 1 = snappy.")]
    [SerializeField, Range(0f, 1f)] private float smoothSpeed = 0.125f;

    [Header("Zoom")]
    [SerializeField] private bool enableZoom = true;
    [SerializeField] private bool requireMouseButtonForZoom = false;
    [SerializeField] private int zoomMouseButton = 2; // 0=LMB,1=RMB,2=MMB,3/4 extra buttons
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minOrthographicSize = 2f;
    [SerializeField] private float maxOrthographicSize = 20f;
    [SerializeField] private float minFieldOfView = 30f;
    [SerializeField] private float maxFieldOfView = 80f;

    [Tooltip("Minimum seconds between automatic target searches.")]
    [SerializeField, Min(0.1f)] private float searchCooldown = 0.5f;

    private float nextSearchTime;
    private UnityEngine.Camera cachedCamera;

    private void Awake()
    {
        cachedCamera = GetComponent<UnityEngine.Camera>();
        TryAssignTarget();
    }

    private void OnEnable()
    {
        TryAssignTarget();
    }

    private void Update()
    {
        if (enableZoom)
        {
            HandleZoomInput();
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            if (autoFindPlayer && Time.time >= nextSearchTime)
            {
                nextSearchTime = Time.time + searchCooldown;
                TryAssignTarget();
            }

            return;
        }

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    private void HandleZoomInput()
    {
        if (cachedCamera == null)
        {
            cachedCamera = GetComponent<UnityEngine.Camera>();
            if (cachedCamera == null)
            {
                return;
            }
        }

        if (requireMouseButtonForZoom && !Input.GetMouseButton(zoomMouseButton))
        {
            return;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Approximately(scroll, 0f))
        {
            return;
        }

        if (cachedCamera.orthographic)
        {
            float size = cachedCamera.orthographicSize - scroll * zoomSpeed;
            cachedCamera.orthographicSize = Mathf.Clamp(size, minOrthographicSize, maxOrthographicSize);
        }
        else
        {
            float fov = cachedCamera.fieldOfView - scroll * zoomSpeed;
            cachedCamera.fieldOfView = Mathf.Clamp(fov, minFieldOfView, maxFieldOfView);
        }
    }

    private void TryAssignTarget()
    {
        if (target != null || !autoFindPlayer)
        {
            return;
        }

        var playerController = FindFirstObjectByType<LivingWorldRPG.Characters.PlayerController>();
        if (playerController != null)
        {
            target = playerController.transform;
            return;
        }

        var taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            target = taggedPlayer.transform;
        }
    }
}
}
