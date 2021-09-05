using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIInputWindowForSaveMap : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;
    public GameObject savemanager;
    private saveManager saveManager;
    private LoadLevel levelLoader;
    public string scenename;
    private void Awake()
    {
        levelLoader = GameObject.FindGameObjectWithTag("GameController").GetComponent<LoadLevel>();
        saveManager = savemanager.GetComponent<saveManager>();
        Hide();
    }
    public void Show(string inputString)
    {
        gameObject.SetActive(true);
        inputField.text = inputString;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    //we call this method when we click accept on the save manager. The button calls it
    public void SaveEnterPressed()
    {
        Debug.Log("hello, there");
        saveManager.QuickSaveMap(inputField.text);
    }
    //we call this method when we click accept on the load manager. The button calls it
    public void LoadEnterPressed()
    {
        Debug.Log("General Kenobi");
        saveManager.QuickLoadMap(inputField.text);
    }
    public void changetoplayscene()
    {
        PlayerPrefs.SetString("mapname", inputField.text);
        levelLoader.LoadScene(scenename);
    }

}
