using System.Collections.Generic;

public static class QuestManager
{
    public static List<Quest> activeQuests => new List<Quest>(ResolveService()?.ActiveQuests ?? new List<Quest>());

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
        if (_questService == null)
        {
            if (!ServiceLocator.TryResolve(out _questService))
            {
                _questService = QuestService.Instance;
            }
        }

        return _questService;
    }

    private static IQuestService _questService;
}
