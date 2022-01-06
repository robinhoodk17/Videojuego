using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class mapChooser : MonoBehaviour
{
    public TextMeshProUGUI mapName;
    public Image ButtonImage;
    public saveManager savemanager;
    public NetworkManager levelLoader;

    public void Start()
    {
        levelLoader = GameObject.FindGameObjectWithTag("GameController").GetComponent<NetworkManager>();
        savemanager = GameObject.FindGameObjectWithTag("saveManager").GetComponent<saveManager>();
    }
    public void onHover()
    {
        ButtonImage.color = new Color(.9f, .9f, .9f);
    }

    public void onPointerExit()
    {
        ButtonImage.color = new Color(1f, 1f, 1f);
    }

    public IEnumerator wait(float waitingtime)
    {
        yield return new WaitForSeconds(waitingtime);
        ButtonImage.color = new Color(1f, 1f, 1f);
    }

    public void onClick()
    {
        PlayerPrefs.SetString("mapname", mapName.text);
        if (PlayerPrefs.GetString("AIorHuman") == "Human")
            levelLoader.Connect();
        else
            levelLoader.ChangeScene("AI");
    }
}
