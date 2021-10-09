using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class deckCards : MonoBehaviour
{
    public GameObject card;
    public CardPoolManager manager;
    public Image sprite;
    public void customAwake()
    {
        manager = GameObject.FindGameObjectWithTag("CardPoolManager").GetComponent<CardPoolManager>();
        sprite.sprite = card.GetComponent<UnitCards>().sprite.sprite;
    }
    public void onClick()
    {
        manager.selectedunits.Remove(card.GetComponent<UnitCards>().unitName.text);
        card.SetActive(true);
        Destroy(this.gameObject);
    }
}
