using System.Collections.Generic;
using UnityEngine;
using LivingWorldRPG.World;

namespace LivingWorldRPG.Core
{
    /// <summary>
    /// MonoBehaviour adapter that implements IQuestService using the existing static QuestManager logic.
    /// </summary>
    public class QuestService : MonoBehaviour, IQuestService
    {
        [SerializeField] private AudioManager audioManagerFallback;

        private readonly List<Quest> activeQuests = new List<Quest>();
        private IAudioService audioService;
        private IWorldSimulationService worldSimulation;

        public static QuestService Instance { get; private set; }

        public IReadOnlyList<Quest> ActiveQuests => activeQuests;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            ResolveAudioService();
            ServiceLocator.TryResolve(out worldSimulation);
        }

        public void StartQuest(Quest quest)
        {
            if (quest == null || activeQuests.Contains(quest))
            {
                return;
            }

            quest.status = QuestStatus.InProgress;
            activeQuests.Add(quest);
        }

        public void CompleteQuest(Quest quest)
        {
            if (quest == null)
            {
                return;
            }

            quest.status = QuestStatus.Completed;
            GrantRewards(quest);
            activeQuests.Remove(quest);
            ResolveAudioService()?.PlayQuestCompleteSound();
        }

        public void UpdateObjective(Quest quest, int objectiveIndex)
        {
            if (quest == null || objectiveIndex < 0 || objectiveIndex >= quest.objectives.Count)
            {
                return;
            }

            quest.objectives[objectiveIndex].completed = true;
            if (quest.IsCompleted())
            {
                CompleteQuest(quest);
            }
        }

        private void GrantRewards(Quest quest)
        {
            if (quest?.rewards == null)
            {
                return;
            }

            var player = worldSimulation?.State.Player;
            if (player == null)
            {
                Debug.LogError("QuestService cannot grant rewards without a player reference.");
                return;
            }

            foreach (string reward in quest.rewards)
            {
                if (reward.StartsWith("Gold: ") && int.TryParse(reward.Substring(6), out int gold))
                {
                    player.Party.Gold += gold;
                }
            }
        }

        private IAudioService ResolveAudioService()
        {
            if (audioService == null && !ServiceLocator.TryResolve(out audioService))
            {
                audioService = audioManagerFallback ?? AudioManager.Instance;
            }

            return audioService;
        }
    }
}
