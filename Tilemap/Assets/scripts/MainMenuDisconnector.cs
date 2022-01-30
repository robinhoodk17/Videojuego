using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuDisconnector : MonoBehaviour
{
    public NetworkManager levelLoader;
    // Start is called before the first frame update
    void Start()
    {
        levelLoader.Disconnect();
    }
}
