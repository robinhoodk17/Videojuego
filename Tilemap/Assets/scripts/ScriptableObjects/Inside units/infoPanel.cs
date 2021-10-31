using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class infoPanel : MonoBehaviour
{
    public GameObject panel;
    public Image sprite;
    private unitScript unit;
    public TextMeshProUGUI Cost;
    public TextMeshProUGUI SUPCost;
    public TextMeshProUGUI BarracksName;
    public TextMeshProUGUI Level;
    public TextMeshProUGUI XP;
    public TextMeshProUGUI type;
    public TextMeshProUGUI Advantages;
    public TextMeshProUGUI Resistances;
    public TextMeshProUGUI vulnerabilities;
    public TextMeshProUGUI statustext; 
    public GameObject status;
    public GameObject attack;
    public GameObject movement;

    // Start is called before the first frame update
    public void showPanel()
    {
        unit = gameObject.GetComponent<unitScript>();
        sprite.sprite = gameObject.transform.GetChild(3).GetComponent<SpriteRenderer>().sprite;
        Cost.text = unit.foodCost.ToString();
        SUPCost.text = unit.SUPCost.ToString();
        BarracksName.text = unit.barracksname;
        Level.text = "LV" + unit.level.ToString();
        XP.text = "XP" + unit.xp.ToString() + "/5";
        type.text = unit.typeOfUnit.ToString();

        #region advantages etc
        string temporal = "";
        foreach(string member in unit.advantages)
        {
            temporal += " " + member;
        }
        Advantages.text = "Adv. vs: " + temporal;

        temporal = "";
        foreach (string member in unit.resistances)
        {
            temporal += " " + member;
        }
        Resistances.text = "Res. vs: " + temporal;

        temporal = "";
        foreach (string member in unit.vulnerabilities)
        {
            temporal += " " + member;
        }
        vulnerabilities.text = "Vul. vs: " + temporal;
        #endregion

        //status
        if (unit.status != "clear")
        {
            statustext.text = unit.status;
            if (unit.status == "recovered" || unit.status == "stunned")
            {
                statustext.text = "stunned";
                status.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
        else
        {
            status.transform.GetChild(0).gameObject.SetActive(false);
            statustext.text = "clear";
        }

        //attack range and damage
        if(unit.attackrange == 1)
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
        switch (unit.movementtype.ToString())
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

        panel.SetActive(true);
    }

    public void hidePanel()
    {
        panel.SetActive(false);
    }
}
