using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controllable_script : MonoBehaviour
{
    public int owner = 0;
    public GameObject ownership; 
    public void ownerchange(int newowner)
    {
        ownership.transform.GetChild(owner).gameObject.SetActive(false);
        owner = newowner;
        ownership.transform.GetChild(newowner).gameObject.SetActive(true);
    }
}
