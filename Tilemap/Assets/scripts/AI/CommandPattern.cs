using System.Collections.Generic;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine;
using TMPro;
using System.Linq;

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
    private Camera _mainCamera;

    private bool firstTurn = true;
    private List<GameObject> selectedbuildables;


    void takeTurn()
    {
        GameObject[] controllables = GameObject.FindGameObjectsWithTag("Controllable");
        moveUnits();
        findOptimalBuild(controllables);

    }

    public void onTurnEnd()
    {
        //we create a list of the buildable units and sort it from cheapest to most expensive
        if (firstTurn)
        {
            selectedbuildables = new List<GameObject>(mapmanager.selectedbuildables.Values);
            selectedbuildables = selectedbuildables.OrderBy(go => go.GetComponent<unitScript>().foodCost).ToList<GameObject>();
        }


        if (activeplayer < maxplayers)
            activeplayer++;
        else
            activeplayer = 1;
        if (activeplayer == 2)
            takeTurn();
    }

    public void moveUnits()
    {
        GameObject[] allunits = GameObject.FindGameObjectsWithTag("Unit");
    }
    public void findOptimalBuild(GameObject[] controllables)
    {
        int currentfood = mapmanager.food[1];
        int currentSUP = mapmanager.SUP[1];
        int foodcost = 0;
        int SUPcost = 0;
        List<GameObject> myBarracks = new List<GameObject>();
        //we find all the barracks owned by player 2
        foreach(GameObject tile in controllables)
        {
            if (tile.GetComponent<controllable_script>().owner == 2 && map.GetTile<levelTile>(gridPosition(tile.transform.position)).type == tileType.barracks)
                myBarracks.Add(tile);
        }
        int numberofBarracks = myBarracks.Count;
        unitScript[] unitstoBuild = new unitScript[numberofBarracks];
        int j = 0;
        //we assign the cheapest unit to build to each barrack so that all barracks build at least 1 unit.
        for(int i = 0; i < numberofBarracks; i++)
        {
            if (selectedbuildables[0].GetComponent<unitScript>().foodCost + foodcost <= currentfood && selectedbuildables[0].GetComponent<unitScript>().SUPCost + SUPcost <= currentSUP)
            {
                unitstoBuild[i] = selectedbuildables[0].GetComponent<unitScript>();
                foodcost += selectedbuildables[0].GetComponent<unitScript>().foodCost;
                SUPcost += selectedbuildables[0].GetComponent<unitScript>().SUPCost;
                j++;
            }
        }
        for(int i = 0; i < j; i++)
        {
            foodcost -= selectedbuildables[0].GetComponent<unitScript>().foodCost;
            SUPcost -= selectedbuildables[0].GetComponent<unitScript>().SUPCost;
            int maxfoodcost = selectedbuildables[0].GetComponent<unitScript>().foodCost;
            for (int k = 0; k < selectedbuildables.Count; k++)
            {
                if (selectedbuildables[k].GetComponent<unitScript>().foodCost + foodcost <= currentfood && selectedbuildables[k].GetComponent<unitScript>().SUPCost + SUPcost <= currentSUP)
                {
                    unitstoBuild[i] = selectedbuildables[k].GetComponent<unitScript>();
                    foodcost += selectedbuildables[0].GetComponent<unitScript>().foodCost;
                    SUPcost += selectedbuildables[0].GetComponent<unitScript>().SUPCost;
                }
            }
        }
        foreach(GameObject tile in myBarracks)
        {
            if(j>0)
            {
                j--;
                mapmanager.currentposition = gridPosition(tile.transform.position);
                mapmanager.CurrentButtonPressed = unitstoBuild[j].name;
                mapmanager.buildUnit(unitstoBuild[j]);
                unitname.text = unitstoBuild[j].barracksname;
                accept.acceptPressed();
            }
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
    public unitScript getunit(Vector3 position, bool screen = true)
    {

        if (!screen)
        {
            position = _mainCamera.WorldToScreenPoint(position);
        }
        var ray = _mainCamera.ScreenPointToRay(position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var selection = hit.transform.gameObject;
            unitScript unit = selection.GetComponent<unitScript>();
            return unit;
        }
        //returns null if it did not find a unit
        else
        {
            return null;
        }
    }
}
