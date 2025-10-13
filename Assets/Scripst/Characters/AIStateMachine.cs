using UnityEngine;

public enum AIState { Idle, Patrolling, Interacting, Working, Sleeping, Eating, Wandering }

public class AIStateMachine : MonoBehaviour
{
    public AIState currentState = AIState.Idle;
    private NPC npc;

    void Start()
    {
        npc = GetComponent<NPC>();
        if (npc == null)
        {
            Debug.LogError("AIStateMachine requires an NPC component.");
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case AIState.Idle:
                HandleIdle();
                break;
            case AIState.Patrolling:
                HandlePatrolling();
                break;
            case AIState.Interacting:
                HandleInteracting();
                break;
            case AIState.Working:
                HandleWorking();
                break;
            case AIState.Sleeping:
                HandleSleeping();
                break;
            case AIState.Eating:
                HandleEating();
                break;
            case AIState.Wandering:
                HandleWandering();
                break;
        }
    }

    private void HandleIdle()
    {
        // Transition to patrolling or wandering based on activity
        if (npc != null)
        {
            switch (npc.CurrentActivity)
            {
                case Activity.Wandering:
                    currentState = AIState.Wandering;
                    break;
                case Activity.Working:
                    currentState = AIState.Working;
                    break;
                case Activity.Sleeping:
                    currentState = AIState.Sleeping;
                    break;
                case Activity.Eating:
                    currentState = AIState.Eating;
                    break;
            }
        }
    }

    private void HandlePatrolling()
    {
        // Implement patrolling logic (use NPC_Controller if available)
        NPC2DController controller = GetComponent<NPC2DController>();
        if (controller != null)
        {
            // Controller handles movement
        }
    }

    private void HandleInteracting()
    {
        // Handle player interaction
        // This could trigger dialogue or quests
    }

    private void HandleWorking()
    {
        // Simulate working (perhaps move to a work location)
    }

    private void HandleSleeping()
    {
        // Simulate sleeping (reduce activity)
    }

    private void HandleEating()
    {
        // Simulate eating (perhaps move to a food location)
    }

    private void HandleWandering()
    {
        // Random wandering behavior
    }

    public void SetState(AIState newState)
    {
        currentState = newState;
    }
}
