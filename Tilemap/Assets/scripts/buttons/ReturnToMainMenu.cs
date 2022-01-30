using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToMainMenu : MonoBehaviour
{
    private NetworkManager networkManager;

    public void onClick()
    {
        networkManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkManager>();
        networkManager.ChangeScene("MainMenu");
        Destroy(networkManager.gameObject);
    }
}
