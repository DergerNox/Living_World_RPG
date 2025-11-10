using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MonoBehaviour adapter that implements IQuestService using the existing static QuestManager logic.
/// </summary>
public class QuestService : MonoBehaviour, IQuestService
{
    [SerializeField]
    private AudioManager audioManagerFallback;

    private readonly List<Quest> _activeQuests = new List<Quest>();
    private IAudioService audioService;
    private IWorldStateRepository worldStateRepository;

    public static QuestService Instance { get; private set; }

    public IReadOnlyList<Quest> ActiveQuests => _activeQuests;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        ResolveAudioService();
    }

    public void StartQuest(Quest quest)
    {
        if (quest == null || _activeQuests.Contains(quest)) return;
        quest.status = QuestStatus.InProgress;
        _activeQuests.Add(quest);
    }

    public void CompleteQuest(Quest quest)
    {
        if (quest == null) return;

        quest.status = QuestStatus.Completed;
        GrantRewards(quest);
        _activeQuests.Remove(quest);
        ResolveAudioService()?.PlayQuestCompleteSound();
    }

    public void UpdateObjective(Quest quest, int objectiveIndex)
    {
        if (quest == null) return;
        if (objectiveIndex < 0 || objectiveIndex >= quest.objectives.Count) return;

        quest.objectives[objectiveIndex].completed = true;
        if (quest.IsCompleted())
        {
            CompleteQuest(quest);
        }
    }

    private void GrantRewards(Quest quest)
    {
        if (quest?.rewards == null) return;

        foreach (string reward in quest.rewards)
        {
            if (reward.StartsWith("Gold: "))
            {
                if (int.TryParse(reward.Substring(6), out int gold))
                {
                    var player = GetPlayer();
                    if (player != null)
                    {
                        player.Party.Gold += gold;
                    }
                }
            }
        }
    }

    private IAudioService ResolveAudioService()
    {
        if (audioService == null)
        {
            if (!ServiceLocator.TryResolve(out audioService))
            {
                audioService = audioManagerFallback ?? AudioManager.Instance;
            }
        }

        return audioService;
    }

    private Player GetPlayer()
    {
        if (worldStateRepository == null)
        {
            if (!ServiceLocator.TryResolve(out worldStateRepository))
            {
                worldStateRepository = WorldHandler.Instance;
            }
        }
        return worldStateRepository?.Player ?? WorldHandler.Player;
    }
}
