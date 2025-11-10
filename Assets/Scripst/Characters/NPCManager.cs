using System.Collections.Generic;
using UnityEngine;

namespace LivingWorldRPG.Characters
{
    public class NPCManager : MonoBehaviour
    {
        public static NPCManager Instance { get; private set; }

        [SerializeField] private List<NPC> npcs = new List<NPC>();
        private ITimeService timeService;

        public IReadOnlyList<NPC> NPCs => npcs;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            timeService = ResolveTimeService();
            if (timeService != null)
            {
                timeService.OnHourChanged += UpdateNPCActivities;
            }
        }

        private void OnDestroy()
        {
            if (timeService != null)
            {
                timeService.OnHourChanged -= UpdateNPCActivities;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void UpdateNPCActivities()
        {
            var service = timeService ?? ResolveTimeService();
            if (service == null)
            {
                return;
            }

            int hour = service.Hour;
            foreach (var npc in npcs)
            {
                if (npc == null)
                {
                    continue;
                }

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
            if (timeService != null)
            {
                return timeService;
            }

            if (!ServiceLocator.TryResolve(out timeService))
            {
                Debug.LogError("NPCManager: ITimeService is not registered in the ServiceLocator.");
            }

            return timeService;
        }

        public void RegisterNPC(NPC npc)
        {
            if (npc != null && !npcs.Contains(npc))
            {
                npcs.Add(npc);
            }
        }
    }
}
