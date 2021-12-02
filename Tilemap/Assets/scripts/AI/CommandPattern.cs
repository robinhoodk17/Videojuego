using System.Collections.Generic;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine;
using TMPro;

public class CommandPattern : MonoBehaviour
{
    public MapManager mapmanager;
    public SelectionManager selectionmanager;
    private int activeplayer = 1;
    private int maxplayers = 2;
    public Tilemap map;
    public unitScript some;
    public TextMeshProUGUI unitname;
    public UIInputWindowForBarracksName accept;

    private bool firstTurn = true;
    private Dictionary<string, GameObject> selectedbuildables;


    void takeTurn()
    {
        GameObject[] controllables = GameObject.FindGameObjectsWithTag("Controllable");
        findOptimalBuild(controllables);

    }

    public void onTurnEnd()
    {
        if (firstTurn)
        {
            selectedbuildables = mapmanager.selectedbuildables;
        }

        if (activeplayer < maxplayers)
            activeplayer++;
        else
            activeplayer = 1;
        if (activeplayer == 2)
            takeTurn();
    }

    public void findOptimalBuild(GameObject[] controllables)
    {
        int currentfood = mapmanager.food[1];
        int currentSUP = mapmanager.SUP[1];
        List<GameObject> myBarracks = new List<GameObject>();
        foreach(GameObject tile in controllables)
        {
            if (tile.GetComponent<controllable_script>().owner == 2 && map.GetTile<levelTile>(gridPosition(tile.transform.position)).type == tileType.barracks)
                myBarracks.Add(tile);
        }
        foreach(GameObject tile in myBarracks)
        {
            mapmanager.currentposition = gridPosition(tile.transform.position);
            mapmanager.buildUnit(some);
            unitname.text = "Jane";
            accept.acceptPressed();
        }

    }

    public Vector3Int gridPosition(Vector3 position, bool screen = false)
    {
        if (screen)
        {
            position = Camera.main.ScreenToWorldPoint(position);
        }

        Vector3Int gridposition = map.WorldToCell(position);
        return gridposition;
    }
}
