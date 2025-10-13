using UnityEngine;

public class NPCDecisionMaker : MonoBehaviour
{
    private NPC npc;
    private AIStateMachine stateMachine;

    void Start()
    {
        npc = GetComponent<NPC>();
        stateMachine = GetComponent<AIStateMachine>();
    }

    void Update()
    {
        MakeDecision();
    }

    private void MakeDecision()
    {
        if (npc == null || stateMachine == null) return;

        // Simple decision making based on activity and time
        int hour = TimeManager.Instance.hour;

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
}
