using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerObject
{
    public Vector2 Posistion;
    public Vector2 Direction;
    public float ChanceToChange;

    public WalkerObject(Vector2 pos,Vector2 dir, float chanceToChange) 
    {
        Posistion = pos;
        Direction = dir;
        ChanceToChange = chanceToChange;
    }
}
