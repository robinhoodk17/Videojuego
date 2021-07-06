using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Level Tile", menuName = "2D/Custom tile/Level Tile")]

public class levelTile : RuleTile
{
    public tileType type;

    public string status;
    public int owner;

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
    farm = 5,
    bonfire = 6,

    //units
    warrior = 1000,

    //conditions
    clear = 2000
}
