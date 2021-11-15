using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIInputWindowForDeckSave : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;
    CardPoolManager cardpoolmanager;
    void Start()
    {
        cardpoolmanager= GameObject.FindGameObjectWithTag("CardPoolManager").GetComponent<CardPoolManager>();
        gameObject.SetActive(false);
    }
    public void AcceptPressed()
    {
        cardpoolmanager.SaveDeck(inputField.text);
        gameObject.SetActive(false);
    }

    public void CancelPressed()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}
