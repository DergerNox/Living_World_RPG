using System.Collections.Generic;

/// <summary>
/// Contract for starting, completing, and tracking quests via inversion of control.
/// </summary>
public interface IQuestService
{
    IReadOnlyList<Quest> ActiveQuests { get; }

    void StartQuest(Quest quest);
    void CompleteQuest(Quest quest);
    void UpdateObjective(Quest quest, int objectiveIndex);
}
