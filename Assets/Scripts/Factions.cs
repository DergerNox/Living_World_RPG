using System.Collections.Generic;
using UnityEngine;

public class Faction
{
    //Public Variables
    public string Name { get { return name; } set { name = value; } }
    public Vector2 Center { get { return center; } set { center = value; } }
    public List<Castle> Castles { get { return castles; } set { castles = value; } }
    public List<Town> Towns { get { return towns; } set { towns = value; } }
    public List<Village> Villages { get { return villages; } set { villages = value; } }
    public int Id { get { return id; } set { id = value; } }
    public Color Color { get; set; } = Color.white;
    public FactionEconomySnapshot Economy { get; private set; } = new FactionEconomySnapshot();
    //Constructor
    public Faction(string name, Vector2 center,int id)
    {
        this.name = name;
        this.center = center;
        this.id = id;
    }
    //Private Variables
    private string name;
    private int id;
    private Vector2 center;
    private List<Castle> castles = new List<Castle>();
    private List<Town> towns = new List<Town>();
    private List<Village> villages = new List<Village>();

    internal void ApplyEconomySnapshot(FactionEconomySnapshot snapshot)
    {
        Economy = snapshot ?? new FactionEconomySnapshot();
    }
}
