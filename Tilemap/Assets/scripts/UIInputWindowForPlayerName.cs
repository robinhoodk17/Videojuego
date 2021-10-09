using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class UIInputWindowForPlayerName : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;

    public void cancelPressed()
    {
        inputField.text = "come on, we don't have all day";
    }
    public void playerNameSaved()
    {
        PlayerPrefs.SetString("PlayerName", inputField.text);
        PhotonNetwork.LoadLevel("MainMenu");
    }
}
