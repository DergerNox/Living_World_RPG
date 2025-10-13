using System.Collections.Generic;
using UnityEngine;

public enum Activity { Sleeping, Working, Wandering, Eating }

public class NPC : Characters
{
    public NPC() : base()
    {
        NameGenerator nameGen = new NameGenerator();
        System.Random random = new System.Random();
        Name = nameGen.GenerateName();
        Health = random.Next(100, 200); // Random health between 100 and 200
        MaxHealth = Health;
        Mana = random.Next(50, 100); // Random mana between 50 and 100
        MaxMana = Mana;
        Level = random.Next(1, 10); // Random level between 1 and 10
        Experience = 0;
        Gold = random.Next(1000, 2000); // Random gold between 1000 and 2000
        SkillBook = new SkillBook();
        CurrentActivity = Activity.Wandering;

    }
    public void Initialize(string name, int factionId, Vector3Int position)
    {
        this.npcName = name;
        this.factionId = factionId;
        this.gridPosition = position;

        // Optional: Update transform, visuals, etc.
        /*transform.position = new Vector3(position.x + 0.5f, position.y + 0.5f, 0);
        gameObject.name = name;*/
    }
    public Activity CurrentActivity { get; set; }

    private string npcName;
    private int factionId;
    private Vector3Int gridPosition;
}
