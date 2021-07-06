using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Level Tile", menuName = "2D/Custom tile/Unit Tile")]
public class unitTile : levelTile
{
    //To access this unit position, you have to do it from the Mapmanager
    public int HP;
    public int MP;
    public int maxHP;
    public int movement;
    public string tag;

    public void turnEnd()
    {
        if(status == "stunned")
        {
            status = "recovered";
        }
    }

    public void turnStart()
    {
        if(status == "recovered")
        {
            status = "clear";
        }
    }
}
