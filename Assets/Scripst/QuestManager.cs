using System.Collections.Generic;

public static class QuestManager
{
    public static List<Quest> activeQuests = new List<Quest>();

    public static void StartQuest(Quest quest)
    {
        quest.status = QuestStatus.InProgress;
        activeQuests.Add(quest);
    }

    public static void CompleteQuest(Quest quest)
    {
        quest.status = QuestStatus.Completed;
        // give rewards
        foreach (string reward in quest.rewards)
        {
            if (reward.StartsWith("Gold: "))
            {
                int gold = int.Parse(reward.Substring(6));
                WorldHandler.Player.Party.Gold += gold;
            }
        }
        activeQuests.Remove(quest);
        // Play quest complete sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayQuestCompleteSound();
        }
    }

    public static void UpdateObjective(Quest quest, int objectiveIndex)
    {
        if (objectiveIndex < quest.objectives.Count)
        {
            quest.objectives[objectiveIndex].completed = true;
            if (quest.IsCompleted())
            {
                CompleteQuest(quest);
            }
        }
    }
}
