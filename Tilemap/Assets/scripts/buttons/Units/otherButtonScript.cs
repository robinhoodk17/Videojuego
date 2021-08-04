using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class otherButtonScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
         
    }

    public eventsScript ability;
    public void OnClick()
    {
        ability.Raise();
    }
}
