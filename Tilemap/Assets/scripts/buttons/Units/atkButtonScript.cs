using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class atkButtonScript : MonoBehaviour
{
    public eventsScript combat;
    public void OnClick()
    {
        combat.Raise();
    }
}
