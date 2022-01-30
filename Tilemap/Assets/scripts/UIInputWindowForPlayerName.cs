using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class UIInputWindowForPlayerName : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;
    public NetworkManager networkManager;

    public void cancelPressed()
    {
        inputField.text = "come on, we don't have all day";
    }
    public void playerNameSaved()
    {
        PlayerPrefs.SetString("PlayerName", inputField.text);
        networkManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkManager>();
        networkManager.ChangeScene("MainMenu");
        Destroy(networkManager.gameObject);
    }
}
