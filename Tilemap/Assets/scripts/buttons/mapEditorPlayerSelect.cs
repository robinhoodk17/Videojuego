using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapEditorPlayerSelect : MonoBehaviour
{
    public int ID;
    public bool Clicked = false;
    private MapEditorManager manager;

    // This script controls the buttons in the mapmanager
    void Awake()
    {
        Clicked = false;
        manager = GameObject.FindGameObjectWithTag("MapEditorManager").GetComponent<MapEditorManager>();
    }

    public void ButtonClicked()
    {
        Clicked = true;
        manager.activeplayer = ID;
    }
}
