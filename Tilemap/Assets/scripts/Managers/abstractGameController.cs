using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class abstractGameController : MonoBehaviour
{
    public abstract void selectedUnitWaits(unitScript selectedunit, Vector3Int gridposition);
}
