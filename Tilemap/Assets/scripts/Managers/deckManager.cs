using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System.Text.RegularExpressions;

public class deckManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject DeckButtons;
    public GameObject DeckArea;
    void Start()
    {
        string[] fileInfo = Directory.GetFiles(Application.streamingAssetsPath, "*.deck");
        foreach (string file in fileInfo)
        {
            string filename = Path.GetFileNameWithoutExtension(file);
            GameObject currentDeck = Instantiate(DeckButtons, new Vector3(0, 0, 0), Quaternion.identity);
            currentDeck.transform.SetParent(DeckArea.transform, false);
            currentDeck.transform.GetChild(0).transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = filename;
        }

    }
}
