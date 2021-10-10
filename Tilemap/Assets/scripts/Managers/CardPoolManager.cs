using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class CardPoolManager : MonoBehaviour
{
    public GameObject SaveDeckDialogue;
    public GameObject[] humans;
    public GameObject humanarea;
    public GameObject[] avatars;
    public GameObject avatararea;
    public GameObject deck;
    public GameObject buttons;
    public List<string> selectedunits;
    public string deckname = "deck";
    public int decklimit = 10;
    private string cardseparator = "#CARD-NAME#";
    // Start is called before the first frame update
    void Start()
    {
        //we knoe the CurrentDeck name because it was set in PlayerPrefs in the DeckSelector scene by the deckButton prefab's deckChooser script
        deckname = PlayerPrefs.GetString("CurrentDeck");
        string saveString = File.ReadAllText(Application.persistentDataPath + "/" + deckname + ".deck");
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
    }

    //we call this method when we select a unitcard
    public void onClick(string buttonname, GameObject card)
    {
        //buttons is a prefab where we store the selected units (which we save in the pool of humans and avatars)
        if(selectedunits.Count < decklimit)
        {
            GameObject button = Instantiate(buttons, new Vector3(0, 0, 0), Quaternion.identity);
            button.transform.SetParent(deck.transform, false);
            button.GetComponent<deckCards>().card = card;
            button.SetActive(true);
            button.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = buttonname;
            button.GetComponent<deckCards>().customAwake();
            string cardname = card.GetComponent<UnitCards>().unitName.text;
            selectedunits.Add(cardname);
        }
    }
    //this method is called when we change scenes (or whenever else the manager is destroyed)
    private void OnDestroy()
    {
        PlayerPrefs.SetInt("decklimit", decklimit);
        int i = 0;
        foreach(string cardname in selectedunits)
        {
            string unitSaveString = deckname + i.ToString();
            PlayerPrefs.SetString(unitSaveString, cardname);
            i++;
        }
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
        File.WriteAllText(Application.persistentDataPath + "/" + savename + ".deck", savestring);

    }
}
