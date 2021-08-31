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
    public void onClick(string buttonname, GameObject card)
    {
        GameObject button = Instantiate(buttons, new Vector3(0, 0, 0), Quaternion.identity);
        button.transform.SetParent(deck.transform, false);
        button.GetComponent<deckCards>().card = card;
        button.SetActive(true);
        button.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = buttonname;
        button.GetComponent<deckCards>().customAwake();
    }
}
