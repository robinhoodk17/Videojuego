using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuDisconnector : MonoBehaviour
{
    private NetworkManager levelLoader;
    // Start is called before the first frame update
    void Start()
    {
        levelLoader = GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkManager>();
        levelLoader.Disconnect();
    }
}
