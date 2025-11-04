using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class NPC2DController : MonoBehaviour
{
    public enum State { Patrol, Waiting, Interacting, Working, Sleeping, Eating, Wandering }
    private State currentState = State.Patrol;

    [Header("Patrol Settings")]
    public Transform[] waypoints;
    public float stoppingDistance = 0.1f;
    public float moveSpeed = 2f;
    public float waitMin = 1f, waitMax = 3f;

    [Header("Interaction Settings")]
    public float talkRadius = 1.5f;
    public string playerTag = "Player";

    // Pathfinding
    private List<Vector3Int> path = null;
    private int pathIndex = 0;

    private Rigidbody2D rb;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private Transform playerTransform;

	private static System.Collections.Generic.Dictionary<float, WaitForSeconds> _waitCache = new System.Collections.Generic.Dictionary<float, WaitForSeconds>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (waypoints == null || waypoints.Length == 0)
            Debug.LogWarning($"[{name}] No waypoints assigned!");
        // Ensure PathfindingManager is initialized
        if (PathfindingManager.Instance == null)
            Debug.LogError("PathfindingManager instance not found in scene.");
    }

    void Update()
    {
        if (currentState == State.Patrol)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, talkRadius);
            if (hit != null && hit.CompareTag(playerTag))
            {
                playerTransform = hit.transform;
                currentState = State.Interacting;
            }
        }
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case State.Patrol:
                if (!isWaiting && waypoints.Length > 0)
                    PatrolMove();
                break;
            case State.Waiting:
                // waiting
                break;
            case State.Interacting:
                InteractWithPlayer();
                break;
            case State.Working:
                HandleWorking();
                break;
            case State.Sleeping:
                HandleSleeping();
                break;
            case State.Eating:
                HandleEating();
                break;
            case State.Wandering:
                HandleWandering();
                break;
        }
    }

    void PatrolMove()
    {
        // Compute grid positions using PathfindingManager.nodeSize
        float nodeSize = PathfindingManager.Instance.nodeSize;
        Vector3Int startCell = Vector3Int.FloorToInt(transform.position / nodeSize);
        Vector3Int targetCell = Vector3Int.FloorToInt(waypoints[currentWaypointIndex].position / nodeSize);

        // Calculate path if needed
        if (path == null || pathIndex >= path.Count)
        {
            path = PathfindingManager.Instance.FindPath(startCell, targetCell);
            pathIndex = 0;
            if (path == null || path.Count == 0)
            {
                DirectMoveTo(waypoints[currentWaypointIndex].position);
                return;
            }
        }

        // Follow path
        Vector3 worldTarget = (Vector3)path[pathIndex] * nodeSize + new Vector3(nodeSize / 2f, nodeSize / 2f, 0);
        float dist = Vector2.Distance(rb.position, worldTarget);
        if (dist > stoppingDistance)
        {
            Vector2 dir = ((Vector2)worldTarget - rb.position).normalized;
            rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            pathIndex++;
            if (pathIndex >= path.Count)
            {
                StartCoroutine(WaitAtWaypoint(Random.Range(waitMin, waitMax)));
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                path = null;
            }
        }
    }

    void DirectMoveTo(Vector3 target)
    {
        Vector2 pos = rb.position;
        Vector2 dir = ((Vector2)target - pos).normalized;
        rb.MovePosition(pos + dir * moveSpeed * Time.fixedDeltaTime);
    }

    IEnumerator WaitAtWaypoint(float t)
    {
        isWaiting = true;
        currentState = State.Waiting;
        float key = Mathf.Round(t * 10f) / 10f; // 0.1s buckets to reuse
        if (!_waitCache.TryGetValue(key, out var wait))
        {
            wait = new WaitForSeconds(key);
            _waitCache[key] = wait;
        }
        yield return wait;
        isWaiting = false;
        currentState = State.Patrol;
    }

    void InteractWithPlayer()
    {
        rb.linearVelocity = Vector2.zero;
        Debug.Log($"Interacting with {playerTransform.name}");
        // TODO: trigger dialogue or event here
        playerTransform = null;
        currentState = State.Patrol;
        path = null;
    }

    void HandleWorking()
    {
        // Simulate working: stay in place or move to work location
        rb.linearVelocity = Vector2.zero;
    }

    void HandleSleeping()
    {
        // Simulate sleeping: stay in place
        rb.linearVelocity = Vector2.zero;
    }

    void HandleEating()
    {
        // Simulate eating: stay in place or move to food location
        rb.linearVelocity = Vector2.zero;
    }

    void HandleWandering()
    {
        // Random wandering: similar to patrolling but without fixed waypoints
        if (!isWaiting)
        {
            // Move to a random nearby location
            Vector3 randomTarget = transform.position + new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
            DirectMoveTo(randomTarget);
            StartCoroutine(WaitAtWaypoint(Random.Range(waitMin, waitMax)));
        }
    }

    void OnDrawGizmos()
    {
        if (waypoints != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++)
            {
                Gizmos.DrawWireSphere(waypoints[i].position, stoppingDistance);
                if (i + 1 < waypoints.Length)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
            }
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, talkRadius);

        // Draw path
        if (path != null)
        {
            Gizmos.color = Color.magenta;
            float nodeSize = PathfindingManager.Instance ? PathfindingManager.Instance.nodeSize : 1f;
            for (int i = pathIndex; i < path.Count; i++)
            {
                Vector3 p = (Vector3)path[i] * nodeSize + new Vector3(nodeSize/2f, nodeSize/2f, 0);
                Gizmos.DrawCube(p, Vector3.one * (nodeSize * 0.3f));
                if (i + 1 < path.Count)
                    Gizmos.DrawLine(p, (Vector3)path[i+1] * nodeSize + new Vector3(nodeSize/2f,nodeSize/2f,0));
            }
        }
    }
}
