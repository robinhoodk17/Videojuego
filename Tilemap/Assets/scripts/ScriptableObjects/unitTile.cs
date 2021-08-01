using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Level Tile", menuName = "2D/Custom tile/Unit Tile")]
public class unitTile : levelTile
{
    //To access this unit position, you have to do it from the Mapmanager
    public string status = "clear";
    public int HP;
    public int MP;
    public int maxHP;
    public int movement;
    public string typeOfUnit;
    public string movementtype;
    public float moveSpeed = 5f;
    public int attackrange = 1;
    public bool attackandmove = true;
    public int level = 0;
    public int levelcounter = 0;
    public int maxlevel = 10;
    public Transform movepoint;

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
            status = "clear";

        levelcounter++;
        if(levelcounter >= 3)
            if(level < maxlevel) { level++;levelcounter = 0; }

    }
}
