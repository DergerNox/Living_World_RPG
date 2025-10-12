using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;

    public List<NPC> npcs = new List<NPC>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        TimeManager.Instance.OnHourChanged += UpdateNPCActivities;
    }

    private void UpdateNPCActivities()
    {
        int hour = TimeManager.Instance.hour;
        foreach (NPC npc in npcs)
        {
            if (hour >= 22 || hour < 6)
            {
                npc.CurrentActivity = Activity.Sleeping;
            }
            else if (hour >= 8 && hour < 18)
            {
                npc.CurrentActivity = Activity.Working;
            }
            else if (hour >= 12 && hour < 14)
            {
                npc.CurrentActivity = Activity.Eating;
            }
            else
            {
                npc.CurrentActivity = Activity.Wandering;
            }
        }
    }
}
