using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardPoolManager : MonoBehaviour
{
    public GameObject[] humans;
    public GameObject humanarea;
    public GameObject[] avatars;
    public GameObject avatararea;
    public GameObject deck;
    public GameObject buttons;
    public List<string> selectedunits;
    public string deckname = "deck";
    public int decklimit = 10;
    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject i in humans)
        {
            GameObject card = Instantiate(i, new Vector3(0, 0, 0), Quaternion.identity);
            card.transform.SetParent(humanarea.transform, false);
        }


        foreach (GameObject i in avatars)
        {
            GameObject card = Instantiate(i, new Vector3(0, 0, 0), Quaternion.identity);
            card.transform.SetParent(avatararea.transform, false);
        }
    }

    // Update is called once per frame
    void Update()
    {
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
}
