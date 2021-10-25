using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class deckChooser : MonoBehaviour
{
    public TextMeshProUGUI deckname;
    public Image ButtonImage;

    public void onHover()
    {
        ButtonImage.color = new Color(.9f, .9f, .9f);
    }
    public void onPointerExit()
    {
        ButtonImage.color = new Color(1f, 1f, 1f);
    }

    public void onClick()
    {
        PlayerPrefs.SetString("CurrentDeck", deckname.text);
        PhotonNetwork.LoadLevel("unitSelectionMenu");
        ButtonImage.color = new Color(.5f, .5f, .5f);
        StartCoroutine(wait(.3f));
    }

    public IEnumerator wait(float waitingtime)
    {
        yield return new WaitForSeconds(waitingtime);
        ButtonImage.color = new Color(1f, 1f, 1f);
    }
}
