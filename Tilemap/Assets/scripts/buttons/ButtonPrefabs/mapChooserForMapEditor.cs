using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class mapChooserForMapEditor : MonoBehaviour
{
    LoadMapSelection MapList;
    public TextMeshProUGUI deckname;
    public Image ButtonImage;
    private saveManager SaveManager;
    // Start is called before the first frame update
    void Start()
    {
        MapList = GameObject.FindGameObjectWithTag("MapList").GetComponent<LoadMapSelection>();
        SaveManager = GameObject.FindGameObjectWithTag("saveManager").GetComponent<saveManager>();
    }

    public void onHover()
    {
        ButtonImage.color = new Color(.9f, .9f, .9f);
    }
    public void onPointerExit()
    {
        ButtonImage.color = new Color(1f, 1f, 1f);
    }

    public void onClick()
    {
        SaveManager.QuickLoadMap(deckname.text);
        MapList.Hide();
    }

    public IEnumerator wait(float waitingtime)
    {
        yield return new WaitForSeconds(waitingtime);
        ButtonImage.color = new Color(1f, 1f, 1f);
    }
}
