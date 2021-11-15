using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class capButtonScript : MonoBehaviour
{
    public eventsScript Capture;
    public void OnClick()
    {
        Capture.Raise();
    }
}
