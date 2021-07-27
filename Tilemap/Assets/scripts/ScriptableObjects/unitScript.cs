using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unitScript : MonoBehaviour
{
    //To access this unit position, you have to do it from the Mapmanager
    public int owner = 1;
    public string status;
    public int HP;
    public int MP;
    public int maxHP;
    public int movement;
    public string typeOfUnit;
    public string movementtype;
    public int attackrange = 1;
    public bool attackandmove = true;
    public bool exhausted = false;
    public int level = 0;
    public int levelcounter = 0;
    public int maxlevel = 10;
    public string state = "idle";
    public float movespeedanimation = 20;
    //public Transform movepoint;
    //public float moveSpeed = 5f;

    public void turnEnd()
    {
        if (status == "stunned")
        {
            status = "recovered";
        }
    }

    public void turnStart()
    {
        if (status == "recovered")
        {
            status = "clear";
        }
        exhausted = false;

        levelcounter++;
        if (levelcounter >= 3)
            if (level < maxlevel) { level++; levelcounter = 0; }
    }


    // Update is called once per frame
    void Update()
    {
    }
}
