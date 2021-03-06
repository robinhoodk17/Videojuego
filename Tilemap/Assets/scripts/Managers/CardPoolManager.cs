using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Linq;

public class CardPoolManager : MonoBehaviour
{
    public GameObject SaveDeckDialogue;
    private List<GameObject> humans = new List<GameObject>();
    public GameObject humanarea;
    private List<GameObject> avatars = new List<GameObject>();
    public GameObject avatararea;
    public GameObject deck;
    public GameObject buttons;
    public List<string> selectedunits;
    public List<int> selectedunitCosts;
    public string deckname = "deck";
    public int decklimit = 10;
    private string cardseparator = "#";
    public TextMeshProUGUI cardcount;
    // Start is called before the first frame update
    void Start()
    {
        List<GameObject> temporal = GameObject.FindGameObjectWithTag("BuildableUnits").GetComponent<BuildableUnits>().Buildables;
        foreach(GameObject card in temporal)
        {
            unitScript unit = card.GetComponent<UnitCards>().unitprefab.GetComponent<unitScript>();
            if(unit.typeOfUnit == TypeOfUnit.avatar)
            {
                avatars.Add(card);
            }
            else
            {
                humans.Add(card);
            }
        }
        //we knoe the CurrentDeck name because it was set in PlayerPrefs in the DeckSelector scene by the deckButton prefab's deckChooser script
        deckname = PlayerPrefs.GetString("CurrentDeck");
        string saveString = File.ReadAllText(Application.streamingAssetsPath + "/" + deckname + ".deck");
        string[] cardsindeck = saveString.Split(new[] { cardseparator }, System.StringSplitOptions.None);
        //Here we check if the instantiated card is part of the deck we loaded
        foreach (GameObject i in humans)
        {

            GameObject card = Instantiate(i, new Vector3(0, 0, 0), Quaternion.identity);
            card.transform.SetParent(humanarea.transform, false);
            foreach(string cardname in cardsindeck)
            {
                if (card.GetComponent<UnitCards>().unitName.text == cardname)
                {
                    card.GetComponent<UnitCards>().onClick();
                }

            }
        }


        foreach (GameObject i in avatars)
        {
            GameObject card = Instantiate(i, new Vector3(0, 0, 0), Quaternion.identity);
            card.transform.SetParent(avatararea.transform, false);
            //Here we check if the instantiated card is part of the deck we loaded
            foreach (string cardname in cardsindeck)
            {
                if (card.GetComponent<UnitCards>().unitName.text == cardname)
                {
                    card.GetComponent<UnitCards>().onClick();
                }

            }
        }
        cardcount.text = selectedunits.Count.ToString() + "/" + decklimit.ToString();
    }

    //we call this method when we select a unitcard
    public void onClick(string buttonname, GameObject card, int Cost)
    {
        //buttons is a prefab where we store the selected units (which we save in the pool of humans and avatars)
        int index = 0;
        if(selectedunits.Count < decklimit)
        {
            foreach(int i in selectedunitCosts)
            {
                if(Cost < i)
                {
                    break;
                }
                index++;
            }
            GameObject button = Instantiate(buttons, new Vector3(0, 0, 0), Quaternion.identity);
            button.transform.SetParent(deck.transform, false);
            button.transform.SetSiblingIndex(index);
            button.GetComponent<deckCards>().card = card;
            button.SetActive(true);
            button.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = buttonname;
            button.GetComponent<deckCards>().customAwake();
            string cardname = card.GetComponent<UnitCards>().unitName.text;
            selectedunits.Insert(index, cardname);
            selectedunitCosts.Insert(index, Cost);
        }
    }
    //this method is called when we change scenes (or whenever else the manager is destroyed)
    private void OnDestroy()
    {
        PlayerPrefs.SetInt("decklimit", decklimit);
        int i = 0;
        foreach(string cardname in selectedunits)
        {
            string unitSaveString = "selecteddeck" + i.ToString();
            PlayerPrefs.SetString(unitSaveString, cardname);
            i++;
        }
        PlayerPrefs.SetInt("decksize", i);
    }

    //Here we actually save the deck
    public void SaveDeck(string savename)
    {
        string savestring = "";
        List<string> cardnames = new List<string>();
        foreach (string cardname in selectedunits)
        {
            cardnames.Add(cardname);
        }
        savestring = string.Join(cardseparator, cardnames);
        File.WriteAllText(Application.streamingAssetsPath + "/" + savename + ".deck", savestring);

    }

    public void Remove(string cardName)
    {
        int index = selectedunits.IndexOf(cardName);
        selectedunits.Remove(cardName);
        selectedunitCosts.RemoveAt(index);
        cardcount.text = selectedunits.Count.ToString() + "/" + decklimit.ToString();
    }

}
