using UnityEngine;

public static class QuestGenerator
{
    private static string[] questTitles = { "Retrieve the Artifact", "Defeat the Bandits", "Explore the Ruins", "Deliver the Message" };
    private static string[] objectiveDescriptions = { "Kill enemies", "Collect items", "Reach location", "Talk to NPC" };

    public static Quest GenerateQuest(int playerLevel)
    {
        QuestDifficulty diff = (QuestDifficulty)Random.Range(0, 3);
        int level = playerLevel + (diff == QuestDifficulty.Easy ? -1 : diff == QuestDifficulty.Normal ? 0 : 1);
        level = Mathf.Max(1, level);

        string title = questTitles[Random.Range(0, questTitles.Length)];
        Quest quest = new Quest("quest_" + Random.Range(0, 1000), title, "A generated quest.");
        quest.difficulty = diff;
        quest.level = level;

        // add objectives
        int objCount = Random.Range(1, 4);
        for (int i = 0; i < objCount; i++)
        {
            string objDesc = objectiveDescriptions[Random.Range(0, objectiveDescriptions.Length)];
            quest.AddObjective(objDesc);
        }

        // rewards
        int goldReward = level * 10 * ((int)diff + 1);
        quest.AddReward("Gold: " + goldReward);

        return quest;
    }
}
