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
    public List<Resources> Resources { get => resources; set => resources = value; }
    public List<AdvancedResources> AdvancedResources { get => advancedResources; set => advancedResources = value; }
    public PartyType PartyType { get => partyType; set => partyType = value; }
    public List<int> ResourcesAmountInInventory { get => resourcesAmountInInventory; set => resourcesAmountInInventory = value; }
    public int CarryWeight { get => carryWeight; set => carryWeight = value; }
    public int Gold { get; set; }
    public List<Item> TradeableItems { get; set; } = new List<Item>();
    public List<Quest> ActiveQuests { get; set; } = new List<Quest>();
    
    //Constructor
    public Party()
    {
        Gold = 1000;
    }
    //Methods
    //Private variables
    private string name;
    private int size;
    private Characters leader;
    private List<Characters> members;
    private List<Troop> troops;
    private List<Resources> resources;
    private List<int>resourcesAmountInInventory;
    private List<AdvancedResources> advancedResources;
    private PartyType partyType;
    private int carryWeight;
}
