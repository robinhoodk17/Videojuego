using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitCards : MonoBehaviour
{

    public Image sprite;
    private unitScript unit;
    public TextMeshProUGUI Cost;
    public TextMeshProUGUI SUPCost;
    public TextMeshProUGUI unitName;
    public TextMeshProUGUI type;
    public GameObject attack;
    public GameObject movement;
    public GameObject unitprefab;
    public TextMeshProUGUI text;
    private CardPoolManager manager;
    public MapEditorManager mapeditormanager;
    public bool cardpool = true;
    void Awake()
    {
        cardpool = true;
        if(GameObject.FindGameObjectWithTag("CardPoolManager") != null)
        {
            manager = GameObject.FindGameObjectWithTag("CardPoolManager").GetComponent<CardPoolManager>();
        }
        else
        {
            cardpool = false;
            mapeditormanager = GameObject.FindGameObjectWithTag("MapEditorManager").GetComponent<MapEditorManager>();
        }
        unit = unitprefab.GetComponent<unitScript>();
        text.text = gameObject.transform.GetChild(8).gameObject.transform.GetChild(4).gameObject.transform.GetChild(0).gameObject.transform.GetChild(10).GetComponent<TextMeshProUGUI>().text;

        sprite.sprite = gameObject.transform.GetChild(8).gameObject.transform.GetChild(3).GetComponent<SpriteRenderer>().sprite;
        Cost.text = unit.foodCost.ToString();
        SUPCost.text = unit.SUPCost.ToString();
        unitName.text = unit.unitname;
        type.text = unit.typeOfUnit;

        //attack range and damage
        if (unit.attackrange == 1)
        {
            attack.transform.GetChild(0).gameObject.SetActive(true);
            attack.GetComponentInChildren<TextMeshProUGUI>().text = ((int)(unit.attackdamage * unit.HP / unit.maxHP)).ToString();
        }
        else
        {
            attack.transform.GetChild(1).gameObject.SetActive(true);
            string attdamage = ((int)(unit.attackdamage * unit.HP / unit.maxHP)).ToString();
            attack.GetComponentInChildren<TextMeshProUGUI>().text = attdamage + ": " + unit.attackrange;
        }

        //movement type and distance
        movement.GetComponentInChildren<TextMeshProUGUI>().text = unit.movement.ToString();
        switch (unit.movementtype)
        {
            case ("foot"):
                movement.transform.GetChild(0).gameObject.SetActive(true);
                break;
            case ("wheels"):
                movement.transform.GetChild(1).gameObject.SetActive(true);
                break;
            case ("treads"):
                movement.transform.GetChild(2).gameObject.SetActive(true);
                break;
            case ("flying"):
                movement.transform.GetChild(3).gameObject.SetActive(true);
                break;


        }

        
    }


    public void onClick()
    {
        if(cardpool)
        {
            if (manager.selectedunits.Count < manager.decklimit)
            {
                manager.onClick(unitName.text, this.gameObject);
                this.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("you reached the deck limit");
            }
        }
        else
        {
            mapeditormanager.onClick(unit);
        }
    }
}
