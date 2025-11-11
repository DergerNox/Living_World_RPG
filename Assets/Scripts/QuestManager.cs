using System.Collections.Generic;
using LivingWorldRPG.Core;

namespace LivingWorldRPG.Core
{
    public static class QuestManager
    {
        public static List<Quest> ActiveQuests => new List<Quest>(ResolveService()?.ActiveQuests ?? new List<Quest>());

        public static void StartQuest(Quest quest)
        {
            ResolveService()?.StartQuest(quest);
        }

        public static void CompleteQuest(Quest quest)
        {
            ResolveService()?.CompleteQuest(quest);
        }

        public static void UpdateObjective(Quest quest, int objectiveIndex)
        {
            ResolveService()?.UpdateObjective(quest, objectiveIndex);
        }

        private static IQuestService ResolveService()
        {
            if (questService == null && !ServiceLocator.TryResolve(out questService))
            {
                questService = QuestService.Instance;
            }

            return questService;
        }

        private static IQuestService questService;
    }
}
