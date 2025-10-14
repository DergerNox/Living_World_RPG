using System.Collections.Generic;
using UnityEngine;

public class Party
{
    //public variables
    public string Name { get => name; set => name = value; }
    public int Size { get => size; set => size = value; }
    public Characters Leader { get => leader; set => leader = value; }
    public List<Characters> Members { get => members; set => members = value; }
    public List<Troop> Troops { get => troops; set => troops = value; }
    public Formation CurrentFormation { get; set; } = new Formation(FormationType.Line);
    public List<Resources> Resources { get => resources; set => resources = value; }
    public List<AdvancedResources> AdvancedResources { get => advancedResources; set => advancedResources = value; }
    public PartyType PartyType { get => partyType; set => partyType = value; }
    public List<int> ResourcesAmountInInventory { get => resourcesAmountInInventory; set => resourcesAmountInInventory = value; }
    public int CarryWeight { get => carryWeight; set => carryWeight = value; }
    public int Gold { get; set; }
    public List<Item> TradeableItems { get; set; } = new List<Item>();
    public List<Quest> ActiveQuests { get; set; } = new List<Quest>();
    public Vector3Int Position { get; set; } = Vector3Int.zero;
    public int Morale { get; set; } = 100; // 0-100 scale
    public int Fatigue { get; set; } = 0; // 0-100 scale

    //Constructor
    public Party()
    {
        Gold = 1000;
        members = new List<Characters>();
        troops = new List<Troop>();
        resources = new List<Resources>();
        resourcesAmountInInventory = new List<int>();
        advancedResources = new List<AdvancedResources>();
        size = 0;
    }

    //Methods
    public void AddMember(Characters character)
    {
        if (!members.Contains(character))
        {
            members.Add(character);
            character.Party = this;
            size++;
        }
    }

    public void RemoveMember(Characters character)
    {
        if (members.Contains(character))
        {
            members.Remove(character);
            character.Party = null;
            size--;
        }
    }

    public void AddTroop(Troop troop)
    {
        troops.Add(troop);
        size++;
    }

    public void RemoveTroop(Troop troop)
    {
        if (troops.Contains(troop))
        {
            troops.Remove(troop);
            size--;
        }
    }

    public void MoveTo(Vector3Int newPosition)
    {
        Position = newPosition;
        ConsumeResourcesOnMove();
        UpdateFatigue();
        UpdateMorale();
    }

    private void ConsumeResourcesOnMove()
    {
        // Simple resource consumption: reduce food/water if available
        int foodIndex = resources.FindIndex(r => r.ResourceType == ResourceType.Food);
        if (foodIndex >= 0 && resourcesAmountInInventory[foodIndex] > 0)
        {
            resourcesAmountInInventory[foodIndex] -= Mathf.Min(1, resourcesAmountInInventory[foodIndex]);
        }
        int waterIndex = resources.FindIndex(r => r.ResourceType == ResourceType.Water);
        if (waterIndex >= 0 && resourcesAmountInInventory[waterIndex] > 0)
        {
            resourcesAmountInInventory[waterIndex] -= Mathf.Min(1, resourcesAmountInInventory[waterIndex]);
        }
    }

    private void UpdateFatigue()
    {
        Fatigue += 5; // Increase fatigue per move
        if (Fatigue > 100) Fatigue = 100;
    }

    private void UpdateMorale()
    {
        // Morale decreases with fatigue, increases with resources
        int resourceBonus = 0;
        if (resourcesAmountInInventory.Count > 0)
        {
            foreach (int amount in resourcesAmountInInventory)
            {
                if (amount > 10) resourceBonus += 5;
            }
        }
        Morale = Mathf.Clamp(Morale - Fatigue / 10 + resourceBonus, 0, 100);
    }

    public void Rest()
    {
        Fatigue = Mathf.Max(0, Fatigue - 20);
        UpdateMorale();
    }

    public void RestAtSettlement(ISettlement settlement)
    {
        Fatigue = Mathf.Max(0, Fatigue - 30); // Better rest in settlements
        UpdateMorale();
        // Optionally replenish some resources
    }

    public Party MergeWith(Party otherParty)
    {
        Party mergedParty = new Party();
        mergedParty.Name = this.Name + " & " + otherParty.Name;
        mergedParty.Leader = this.Leader; // Keep original leader
        mergedParty.Members.AddRange(this.Members);
        mergedParty.Members.AddRange(otherParty.Members);
        mergedParty.Troops.AddRange(this.Troops);
        mergedParty.Troops.AddRange(otherParty.Troops);
        mergedParty.Gold = this.Gold + otherParty.Gold;
        mergedParty.Position = this.Position; // Keep position of merging party
        mergedParty.size = mergedParty.Members.Count + mergedParty.Troops.Count;

        // Merge resources
        mergedParty.Resources.AddRange(this.Resources);
        mergedParty.Resources.AddRange(otherParty.Resources);
        mergedParty.ResourcesAmountInInventory.AddRange(this.ResourcesAmountInInventory);
        mergedParty.ResourcesAmountInInventory.AddRange(otherParty.ResourcesAmountInInventory);

        // Merge advanced resources
        mergedParty.AdvancedResources.AddRange(this.AdvancedResources);
        mergedParty.AdvancedResources.AddRange(otherParty.AdvancedResources);

        // Merge other properties
        mergedParty.TradeableItems.AddRange(this.TradeableItems);
        mergedParty.TradeableItems.AddRange(otherParty.TradeableItems);
        mergedParty.ActiveQuests.AddRange(this.ActiveQuests);
        mergedParty.ActiveQuests.AddRange(otherParty.ActiveQuests);

        return mergedParty;
    }

    public List<Party> Split(int splitSize)
    {
        List<Party> splitParties = new List<Party>();
        Party party1 = new Party();
        Party party2 = new Party();

        party1.Name = this.Name + " Part 1";
        party2.Name = this.Name + " Part 2";

        // Split members
        for (int i = 0; i < members.Count; i++)
        {
            if (i < splitSize)
                party1.AddMember(members[i]);
            else
                party2.AddMember(members[i]);
        }

        // Split troops
        for (int i = 0; i < troops.Count; i++)
        {
            if (i < splitSize)
                party1.AddTroop(troops[i]);
            else
                party2.AddTroop(troops[i]);
        }

        party1.Gold = this.Gold / 2;
        party2.Gold = this.Gold / 2;
        party1.Position = this.Position;
        party2.Position = this.Position;

        // Split resources
        for (int i = 0; i < resources.Count; i++)
        {
            if (i < splitSize)
            {
                party1.Resources.Add(resources[i]);
                party1.ResourcesAmountInInventory.Add(resourcesAmountInInventory[i]);
            }
            else
            {
                party2.Resources.Add(resources[i]);
                party2.ResourcesAmountInInventory.Add(resourcesAmountInInventory[i]);
            }
        }

        // Split advanced resources
        for (int i = 0; i < advancedResources.Count; i++)
        {
            if (i < splitSize)
                party1.AdvancedResources.Add(advancedResources[i]);
            else
                party2.AdvancedResources.Add(advancedResources[i]);
        }

        // Split other properties
        for (int i = 0; i < TradeableItems.Count; i++)
        {
            if (i < splitSize)
                party1.TradeableItems.Add(TradeableItems[i]);
            else
                party2.TradeableItems.Add(TradeableItems[i]);
        }

        for (int i = 0; i < ActiveQuests.Count; i++)
        {
            if (i < splitSize)
                party1.ActiveQuests.Add(ActiveQuests[i]);
            else
                party2.ActiveQuests.Add(ActiveQuests[i]);
        }

        splitParties.Add(party1);
        splitParties.Add(party2);
        return splitParties;
    }

    //Private variables
    private string name;
    private int size;
    private Characters leader;
    private List<Characters> members;
    private List<Troop> troops;
    private List<Resources> resources;
    private List<int> resourcesAmountInInventory;
    private List<AdvancedResources> advancedResources;
    private PartyType partyType;
    private int carryWeight;
}
