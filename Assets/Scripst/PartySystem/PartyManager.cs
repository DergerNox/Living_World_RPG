using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour, IPartyService
{
    public static PartyManager Instance;

    public Party PlayerParty { get; private set; }
    public List<Party> AllParties { get; private set; } = new List<Party>();
    IReadOnlyList<Party> IPartyService.AllParties => AllParties;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayerParty = WorldHandler.Player.Party;
        AllParties.Add(PlayerParty);
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
}
