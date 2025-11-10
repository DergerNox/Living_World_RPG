using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;

    public List<NPC> npcs = new List<NPC>();
    private ITimeService timeService;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        var service = ResolveTimeService();
        if (service != null)
        {
            service.OnHourChanged += UpdateNPCActivities;
        }
    }

    private void OnDestroy()
    {
        var service = timeService;
        if (service != null)
        {
            service.OnHourChanged -= UpdateNPCActivities;
        }
    }

    private void UpdateNPCActivities()
    {
        var service = ResolveTimeService();
        int hour = service?.Hour ?? 0;
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
