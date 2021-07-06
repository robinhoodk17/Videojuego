using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BattleSystem : MonoBehaviour
{
    public int numberOfPlayers;
    public BattleState state;
    public int activeplayer;
    void Start()
    {
        activeplayer = 1;
        state = BattleState.PLAYERTURN;
        endTurn endTurnButtonBlicked = GetComponent<endTurn>();
    }

    public void turnEnd()
    {
        state = BattleState.ENDTURN;
        if (activeplayer < numberOfPlayers)
        { activeplayer++; }
        else
        { activeplayer = 1; }
        state = BattleState.START;
        Debug.Log("it works!");
    }
}
