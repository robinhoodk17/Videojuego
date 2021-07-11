using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unitScript : MonoBehaviour
{
    //To access this unit position, you have to do it from the Mapmanager
    public string status;
    public int HP;
    public int MP;
    public int maxHP;
    public int movement;
    public string typeOfUnit;
    public string movementtype;
    public float moveSpeed = 5f;
    public int owner = 1;
    public Transform movepoint;

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
