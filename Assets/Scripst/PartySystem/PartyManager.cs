using System.Collections.Generic;
using UnityEngine;
using LivingWorldRPG.World;

namespace LivingWorldRPG.PartySystem
{
    public class PartyManager : MonoBehaviour, IPartyService
    {
        public static PartyManager Instance { get; private set; }

        public Party PlayerParty { get; private set; }
        public List<Party> AllParties { get; private set; } = new List<Party>();
        IReadOnlyList<Party> IPartyService.AllParties => AllParties;
        private IWorldSimulationService worldSimulation;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            ServiceLocator.TryResolve(out worldSimulation);
        }

        private void Start()
        {
            var player = worldSimulation?.State.Player;
            if (player != null)
            {
                PlayerParty = player.Party;
                if (!AllParties.Contains(PlayerParty))
                {
                    AllParties.Add(PlayerParty);
                }
            }
            else
            {
                Debug.LogWarning("PartyManager could not find a player party. AI party generation will continue without the player.");
            }

            GenerateAIParties();
        }

	void OnEnable()
	{
		TickSystem.OnSecond += OnSecondTick;
	}

	void OnDisable()
	{
		TickSystem.OnSecond -= OnSecondTick;
	}

    private void GenerateAIParties()
    {
        // Generate some AI parties for the world
        for (int i = 0; i < 5; i++)
        {
            Party aiParty = new Party();
            aiParty.Name = "AI Party " + (i + 1);
            aiParty.PartyType = (PartyType)Random.Range(0, System.Enum.GetValues(typeof(PartyType)).Length);
            aiParty.Position = new Vector3Int(Random.Range(0, 100), Random.Range(0, 100), 0);
            AllParties.Add(aiParty);
        }
    }

    public void AddParty(Party party)
    {
        if (!AllParties.Contains(party))
        {
            AllParties.Add(party);
        }
    }

    public void RemoveParty(Party party)
    {
        if (AllParties.Contains(party))
        {
            AllParties.Remove(party);
        }
    }

    public List<Party> GetPartiesAtPosition(Vector3Int position)
    {
        return AllParties.FindAll(p => p.Position == position);
    }

    public void UpdatePartyPositions()
    {
        // Update AI party movements (simple random movement for demo)
        foreach (Party party in AllParties)
        {
            if (party != PlayerParty)
            {
                Vector3Int newPos = party.Position + new Vector3Int(Random.Range(-1, 2), Random.Range(-1, 2), 0);
                party.MoveTo(newPos);
            }
        }
    }

	private void OnSecondTick()
	{
		UpdatePartyPositions();
	}

        private IWorldSimulationService ResolveWorldSimulation()
        {
            if (worldSimulation == null && !ServiceLocator.TryResolve(out worldSimulation))
            {
                Debug.LogError("PartyManager requires an IWorldSimulationService.");
            }

            return worldSimulation;
        }
    }
}
