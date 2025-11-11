using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Quest
{
    public string id;
    public string title;
    public string description;
    public QuestStatus status;
    public QuestDifficulty difficulty;
    public int level;
    public List<QuestObjective> objectives;
    public List<string> rewards;

    public Quest(string id, string title, string description)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.status = QuestStatus.NotStarted;
        this.difficulty = QuestDifficulty.Normal;
        this.level = 1;
        this.objectives = new List<QuestObjective>();
        this.rewards = new List<string>();
    }

    public void AddObjective(string description)
    {
        objectives.Add(new QuestObjective(description));
    }

    public void AddReward(string reward)
    {
        rewards.Add(reward);
    }

    public bool IsCompleted()
    {
        return objectives.All(o => o.completed);
    }
}

public enum QuestStatus { NotStarted, InProgress, Completed, Failed }

public enum QuestDifficulty { Easy, Normal, Hard }

[System.Serializable]
public class QuestObjective
{
    public string description;
    public bool completed;

    public QuestObjective(string description)
    {
        this.description = description;
        this.completed = false;
    }
}
