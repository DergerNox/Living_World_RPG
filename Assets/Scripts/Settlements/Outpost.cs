using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
public class Outpost
{
    //public variables
    public string Name{get{return name;}set{name = value;}}
    public int Level{get{return outpostLevel;}set{outpostLevel = value;}}
    public int ResourcesPerDay{get{return resourcesPerDay;}set{resourcesPerDay = value;}}
    public int ResourcesInStock{get{return resourcesInStock;}set{resourcesInStock = value;}}
    public int ResourcesCapacity{get{return resourcesCapacity;}set{resourcesCapacity = value;}}
    public int GarrisonCapacity{get{return garrisonCapacity;}set{garrisonCapacity = value;}}
    public int GarrisonSize{get{return garrisonSize;}set{garrisonSize = value;}}
    public Resources Resources{get{return resources;}set{resources = value;}}
    public List<Characters> Visitors{get{return visitors;}set{visitors = value;}}
    public int PosX{get{return posX;}set{posX = value;}}
    public int PosY{get{return posY;}set{posY = value;}}
    public bool ReceavedVillagers{get{return receavedVillagers;}set{receavedVillagers = value;}}
    public List<Party> PartiesInOutpost{get{return partiesInOutpost;}set{partiesInOutpost = value;}}
    public Party VillagersFromTheVillage{get{return villagersFromTheVillage;}set{villagersFromTheVillage = value;}}
    //Constructor
    public Outpost(int i_posX, int i_posY,Resources i_resources)
    {
        NameGenerator nameGenerator = new NameGenerator();
        name=nameGenerator.GenerateName();
        Random random = new Random();
        posX = i_posX;
        posY = i_posY;
        outpostLevel = 1;
        garrisonCapacity=100;
        garrisonSize=random.Next(25,50);
        resourcesCapacity=10000;
        resourcesInStock=0;
        resourcesPerDay=200;
        resources=i_resources;
        receavedVillagers=false;
        visitors = new List<Characters>();
        partiesInOutpost = new List<Party>();
        villagersFromTheVillage = null;
    }
    //Public methods
    public void LoadResources()
    {
       if (VillagersFromTheVillage!=null)
       {
        VillagersFromTheVillage.ResourcesAmountInInventory.Add(ResourcesInStock);
        VillagersFromTheVillage.Resources.Add(Resources);
       }
    }

    //private variables
    private string name;
    private int outpostLevel;
    private int resourcesPerDay;
    private int resourcesInStock;
    private int resourcesCapacity;
    private int garrisonCapacity;
    private int garrisonSize;
    private Resources resources;
    private List<Characters>visitors;
    private int posX;
    private int posY;
    private bool receavedVillagers;
    private List<Party> partiesInOutpost;
    private Party villagersFromTheVillage;
}
