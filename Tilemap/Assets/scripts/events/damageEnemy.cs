using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class damageEnemy : MonoBehaviour
{
    public void damage()
    {
        gameObject.GetComponentInParent<unitScript>().damageEnemy();
    }
}
