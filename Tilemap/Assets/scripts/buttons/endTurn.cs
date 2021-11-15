using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class endTurn : MonoBehaviour
{
    public eventsScript OnTurnEnd;
    public void Buttonclicked()
    {
        OnTurnEnd.Raise();
    }
}
