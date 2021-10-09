using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class deckChooser : MonoBehaviour
{
    public TextMeshProUGUI deckname;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClick()
    {
        PlayerPrefs.SetString("CurrentDeck", deckname.text);
        PhotonNetwork.LoadLevel("unitSelectionMenu");
    }
}
