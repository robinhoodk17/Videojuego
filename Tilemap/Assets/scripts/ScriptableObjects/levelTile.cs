using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Level Tile", menuName = "2D/Custom tile/Level Tile")]

public class levelTile : RuleTile
{
    public tileType type;

    
    public int movecost(string movetype)
    {
        int cost = 1;
        switch (movetype)
        {
            case ("foot"):
                cost = footcost;
                break;
            case ("tread"):
                cost = treadcost;
                break;
            case ("flying"):
                cost = flyingcost;
                break;
            case ("wheel"):
                cost = wheelcost;
                break;
        }
        return cost;
    }
    public int treadcost;
    public int footcost;
    public int wheelcost;
    public int flyingcost;
    public int defense;
    public bool controllable = false;
}

[Serializable]

public enum tileType
{
    //ground tiles
    forest = 0,
    mountain = 1,
    planes = 2,
    river = 3,
    road = 4,
    market = 5,
    bonfire = 6,
    barracks = 7,
    HQ = 8,

    //units
    warrior = 1000,

    //conditions
    clear = 2000,

    //UI
    movement = 3000,
    attack = 3001,
    attackandmove = 3002
}
