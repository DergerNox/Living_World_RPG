using UnityEngine;

namespace LivingWorldRPG.Characters
{
    public enum AIState
    {
        Idle,
        Patrolling,
        Interacting,
        Working,
        Sleeping,
        Eating,
        Wandering
    }

    [RequireComponent(typeof(NPC))]
    public class AIStateMachine : MonoBehaviour
    {
        [SerializeField] private AIState currentState = AIState.Idle;

        private NPC npc;
        private NPC2DController controller;

        public AIState CurrentState => currentState;

        private void Awake()
        {
            npc = GetComponent<NPC>();
            controller = GetComponent<NPC2DController>();
        }

        private void Start()
        {
            if (npc == null)
            {
                Debug.LogError("AIStateMachine requires an NPC component.");
                enabled = false;
            }
        }

        private void Update()
        {
            if (!enabled || npc == null)
            {
                return;
            }

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
            // Transition to patrolling or wandering based on the NPC's current activity.
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
                default:
                    currentState = AIState.Patrolling;
                    break;
            }
        }

        private void HandlePatrolling()
        {
            if (controller == null)
            {
                return;
            }

            // Controller handles actual movement while we keep state transitions here.
            controller.enabled = true;
        }

        private void HandleInteracting()
        {
            // Placeholder hook for quest/dialogue interactions.
            if (npc == null)
            {
                return;
            }
        }

        private void HandleWorking()
        {
            if (npc == null)
            {
                return;
            }

            npc.CurrentActivity = Activity.Working;
        }

        private void HandleSleeping()
        {
            if (npc == null)
            {
                return;
            }

            npc.CurrentActivity = Activity.Sleeping;
        }

        private void HandleEating()
        {
            if (npc == null)
            {
                return;
            }

            npc.CurrentActivity = Activity.Eating;
        }

        private void HandleWandering()
        {
            if (npc == null)
            {
                return;
            }

            npc.CurrentActivity = Activity.Wandering;
        }

        public void SetState(AIState newState)
        {
            currentState = newState;
        }
    }
}
