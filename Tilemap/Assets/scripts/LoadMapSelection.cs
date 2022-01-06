using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class LoadMapSelection : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject MapButtons;
    public GameObject MapsArea;
    void Start()
    {
        string[] fileInfo = Directory.GetFiles(Application.persistentDataPath, "*.map");
        foreach (string file in fileInfo)
        {
            string filename = Path.GetFileNameWithoutExtension(file);
            GameObject currentMap = Instantiate(MapButtons, new Vector3(0, 0, 0), Quaternion.identity);
            currentMap.transform.SetParent(MapsArea.transform, false);
            currentMap.transform.GetChild(0).transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = filename;
        }

    }
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
