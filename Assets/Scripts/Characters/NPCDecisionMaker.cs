using UnityEngine;

namespace LivingWorldRPG.Characters
{
    [RequireComponent(typeof(NPC), typeof(AIStateMachine))]
    public class NPCDecisionMaker : MonoBehaviour
    {
        private NPC npc;
        private AIStateMachine stateMachine;
        private ITimeService timeService;

        private void Awake()
        {
            npc = GetComponent<NPC>();
            stateMachine = GetComponent<AIStateMachine>();
        }

        private void Start()
        {
            ResolveTimeService();
        }

        private void Update()
        {
            MakeDecision();
        }

        private void MakeDecision()
        {
            if (npc == null || stateMachine == null)
            {
                return;
            }

            var service = ResolveTimeService();
            if (service == null)
            {
                return;
            }

            int hour = service.Hour;

            if (hour >= 22 || hour < 6)
            {
                ApplyState(Activity.Sleeping, AIState.Sleeping);
            }
            else if (hour >= 8 && hour < 18)
            {
                ApplyState(Activity.Working, AIState.Working);
            }
            else if (hour >= 12 && hour < 14)
            {
                ApplyState(Activity.Eating, AIState.Eating);
            }
            else
            {
                ApplyState(Activity.Wandering, AIState.Wandering);
            }
        }

        private void ApplyState(Activity activity, AIState state)
        {
            if (npc.CurrentActivity == activity)
            {
                return;
            }

            npc.CurrentActivity = activity;
            stateMachine.SetState(state);
        }

        private ITimeService ResolveTimeService()
        {
            if (timeService != null)
            {
                return timeService;
            }

            if (!ServiceLocator.TryResolve(out timeService))
            {
                Debug.LogError("NPCDecisionMaker: ITimeService is not registered in the ServiceLocator.");
            }

            return timeService;
        }
    }
}
