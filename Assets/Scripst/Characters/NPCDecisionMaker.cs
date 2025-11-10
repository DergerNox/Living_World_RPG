using UnityEngine;

public class NPCDecisionMaker : MonoBehaviour
{
    private NPC npc;
    private AIStateMachine stateMachine;
    private ITimeService timeService;

    void Start()
    {
        npc = GetComponent<NPC>();
        stateMachine = GetComponent<AIStateMachine>();
        ResolveTimeService();
    }

    void Update()
    {
        MakeDecision();
    }

    private void MakeDecision()
    {
        if (npc == null || stateMachine == null) return;

        var service = ResolveTimeService();
        int hour = service?.Hour ?? 0;

        if (hour >= 22 || hour < 6)
        {
            if (npc.CurrentActivity != Activity.Sleeping)
            {
                npc.CurrentActivity = Activity.Sleeping;
                stateMachine.SetState(AIState.Sleeping);
            }
        }
        else if (hour >= 8 && hour < 18)
        {
            if (npc.CurrentActivity != Activity.Working)
            {
                npc.CurrentActivity = Activity.Working;
                stateMachine.SetState(AIState.Working);
            }
        }
        else if (hour >= 12 && hour < 14)
        {
            if (npc.CurrentActivity != Activity.Eating)
            {
                npc.CurrentActivity = Activity.Eating;
                stateMachine.SetState(AIState.Eating);
            }
        }
        else
        {
            if (npc.CurrentActivity != Activity.Wandering)
            {
                npc.CurrentActivity = Activity.Wandering;
                stateMachine.SetState(AIState.Wandering);
            }
        }

        // Add more complex decisions here, like reacting to player proximity, faction relations, etc.
    }

    private ITimeService ResolveTimeService()
    {
        if (timeService == null)
        {
            if (!ServiceLocator.TryResolve(out timeService))
            {
                timeService = TimeManager.Instance;
            }
        }
        return timeService;
    }
}
