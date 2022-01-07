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
    Dictionary<unitScript, Dictionary<Vector3Int, int>> possibleMovesForAllUnits = new Dictionary<unitScript, Dictionary<Vector3Int, int>>();
    private bool firstTurn = true;
    private List<GameObject> selectedbuildables;

    private void Start()
    {
        _mainCamera = Camera.main;
    }
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

    public void findOptimalBuild(GameObject[] controllables)
    {
        int currentfood = mapmanager.food[1];
        int currentSUP = mapmanager.SUP[1];
        int foodcost = 0;
        int SUPcost = 0;
        List<GameObject> myBarracks = new List<GameObject>();
        //we find all the barracks owned by player 2
        foreach (GameObject tile in controllables)
        {
            if (tile.GetComponent<controllable_script>().owner == 2 && map.GetTile<levelTile>(gridPosition(tile.transform.position)).type == tileType.barracks)
                myBarracks.Add(tile);
        }
        int numberofBarracks = myBarracks.Count;
        unitScript[] unitstoBuild = new unitScript[numberofBarracks];
        int j = 0;
        //we assign the cheapest unit to build to each barrack so that all barracks build at least 1 unit.
        for (int i = 0; i < numberofBarracks; i++)
        {
            if (selectedbuildables[0].GetComponent<unitScript>().foodCost + foodcost <= currentfood && selectedbuildables[0].GetComponent<unitScript>().SUPCost + SUPcost <= currentSUP)
            {
                unitstoBuild[i] = selectedbuildables[0].GetComponent<unitScript>();
                foodcost += selectedbuildables[0].GetComponent<unitScript>().foodCost;
                SUPcost += selectedbuildables[0].GetComponent<unitScript>().SUPCost;
                j++;
            }
        }
        for (int i = 0; i < j; i++)
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
        foreach (GameObject tile in myBarracks)
        {
            if (j > 0)
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

    //Here we first find all possible moves, then optimize the movement, and then move all units
    public void moveUnits()
    {
        GameObject[] allunits = GameObject.FindGameObjectsWithTag("Unit");
        List<Vector3Int> visitedTiles = new List<Vector3Int>();
        //Here we get all the possible moves for each unit the AI controls and then call the function FindOptimalMove, which calculates the score for each possible movement
        foreach (GameObject unitPrefab in allunits)
        {
            if (unitPrefab.GetComponent<unitScript>().owner == 2)
            {
                unitScript currentMovingUnit = unitPrefab.GetComponent<unitScript>();
                possibleMovesForAllUnits[currentMovingUnit] = findPossibleMoves(gridPosition(unitPrefab.transform.position), currentMovingUnit);
                List<Vector3Int> Keys = new List<Vector3Int>(possibleMovesForAllUnits[currentMovingUnit].Keys);
                foreach (Vector3Int Key in Keys)
                {
                    //Here we add the score calculated for each space (Key) of the currentmovingunit
                    possibleMovesForAllUnits[currentMovingUnit][Key] += findOptimalMove(currentMovingUnit, Key);
                }
            }
        }
        //Here we assume that we already established the score for each movement, and take the best possible move
        foreach (unitScript currentMovingUnit in possibleMovesForAllUnits.Keys)
        {
            //Vector3Int currentPosition = gridPosition(currentMovingUnit.gameObject.transform.position);
            Vector3Int targetPosition = new Vector3Int(0, 0, 0);
            int MaxValue = -10;
            //possibleMovesForAllUnits[currentMovingUnit].Keys is a list that Vector3Int values and contains all the possible tiles that the currentmovingunit can move to
            foreach (Vector3Int possibleTargetPosition in possibleMovesForAllUnits[currentMovingUnit].Keys)
            {
                //Here we check that our current value is the max value we have and that it does not have a unit on it already
                if(possibleMovesForAllUnits[currentMovingUnit][possibleTargetPosition] > MaxValue && !visitedTiles.Contains(possibleTargetPosition))
                {
                    if(map.GetTile<levelTile>(possibleTargetPosition).controllable)
                    {
                        //Here we just check that we are not moving to our own barracks
                        if(!(map.GetTile<levelTile>(possibleTargetPosition).type == tileType.barracks && map.GetInstantiatedObject(possibleTargetPosition).GetComponent<controllable_script>().owner == 2))
                        {
                            MaxValue = possibleMovesForAllUnits[currentMovingUnit][possibleTargetPosition];
                            targetPosition = possibleTargetPosition;
                        }
                    }
                    else
                    {
                        MaxValue = possibleMovesForAllUnits[currentMovingUnit][possibleTargetPosition];
                        targetPosition = possibleTargetPosition;
                    }
                }
            }
            visitedTiles.Add(targetPosition);
            moveUnit(currentMovingUnit, targetPosition);
        }
    }


    public Dictionary<Vector3Int, int> findPossibleMoves(Vector3Int position, unitScript unit)
    {
        Dictionary<Vector3Int, int> possibleMoves = new Dictionary<Vector3Int, int> ();
        Dictionary<Vector3Int, List<Vector3Int>> neighborlist = new Dictionary<Vector3Int, List<Vector3Int>>();
        Dictionary<Vector3Int, Vector3Int> parentlist = new Dictionary<Vector3Int, Vector3Int>();
        Dictionary<Vector3Int, int> distancelist = new Dictionary<Vector3Int, int>();
        Dictionary<Vector3Int, bool> visitlist = new Dictionary<Vector3Int, bool>();
        List<Vector3Int> selectableTiles = new List<Vector3Int>();

        //in this foreach we calculate the neighbors of all tiles and assign the movement cost to move to them
        foreach (var posi in map.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(posi.x, posi.y, posi.z);
            if (map.HasTile(localPlace))
            {
                levelTile Tile = map.GetTile<levelTile>(localPlace);
                findneighbors(unit, localPlace);
                visitlist[localPlace] = false;
                parentlist[localPlace] = Vector3Int.zero;
                distancelist[localPlace] = Tile.movecost(unit.movementtype.ToString());
            }
        }

        //here we initialize the queue where we will get all the selectable tiles
        Queue<Vector3Int> process = new Queue<Vector3Int>();
        visitlist[position] = true;
        distancelist[position] = 0;
        process.Enqueue(position);
        selectableTiles.Add(position);

        //in this while we add all the possible tiles that the unit might move to
        while (process.Count > 0)
        {
            //this is the original position where the unit starts
            Vector3Int pos = process.Dequeue();
            //here we add the tile to the movable tiles if there are no units or there is an available transport or the moving unit can attach to an empty vehicle
            if (getunit(pos) == null)
            { selectableTiles.Add(pos); }
            else
            {
                if ((getunit(pos).istransport || (unit.ability == "attach" && getunit(pos).typeOfUnit == TypeOfUnit.vehicle)) && getunit(pos).transportedUnits.Count < getunit(pos).unitCarryingCapacity && unit.typeOfUnit == TypeOfUnit.infantry)
                { selectableTiles.Add(pos); }
            }
            foreach (Vector3Int vector in neighborlist[pos])
            {
                if (!visitlist[vector] && (distancelist[pos] + distancelist[vector]) <= unit.movement)
                {
                    parentlist[vector] = pos;
                    visitlist[vector] = true;
                    distancelist[vector] = distancelist[pos] + distancelist[vector];
                    process.Enqueue(vector);
                }
                if (visitlist[vector])
                {
                    int tempdistance = map.GetTile<levelTile>(vector).movecost(unit.movementtype.ToString());
                    if (tempdistance + distancelist[pos] < distancelist[vector])
                    {
                        parentlist[vector] = pos;
                        distancelist[vector] = tempdistance + distancelist[pos];
                    }
                }
            }
        }
        //here we add between -3 to +4 depending on the defense of each tile.
        foreach (Vector3Int selectable in selectableTiles)
        {
            possibleMoves[selectable] = map.GetTile<levelTile>(selectable).defense/5;
        }
        List<Vector3Int> positions = new List<Vector3Int>(possibleMoves.Keys);
        return possibleMoves;

        void findneighbors(unitScript unit, Vector3Int position)
        {
            List<Vector3Int> adjacencyList = new List<Vector3Int>();
            string movementType = unit.movementtype.ToString();
            Vector3Int left = new Vector3Int(-1, 0, 0);
            Vector3Int right = new Vector3Int(1, 0, 0);
            Vector3Int up = new Vector3Int(0, 1, 0);
            Vector3Int down = new Vector3Int(0, -1, 0);
            switch (movementType)
            {
                #region case flying
                case "flying":
                    if (map.HasTile(position + left))
                    {
                        levelTile checkedTile = map.GetTile<levelTile>(position + left);
                        int tileowner = 0;
                        if (checkedTile.controllable)
                        {
                            tileowner = map.GetInstantiatedObject(position + left).GetComponent<controllable_script>().owner;
                        }
                        if ((getunit(position + left) == null || getunit(position + left).owner == unit.owner || getunit(position + left).status == "downed") && (tileowner == 0 || tileowner == unit.owner))
                        {
                            adjacencyList.Add(position + left);
                        }
                    }
                    if (map.HasTile(position + up))
                    {
                        Vector3Int checkedposition = position + up;
                        levelTile checkedTile = map.GetTile<levelTile>(checkedposition);
                        int tileowner = 0;
                        if (checkedTile.controllable)
                        {
                            tileowner = map.GetInstantiatedObject(checkedposition).GetComponent<controllable_script>().owner;
                        }
                        if ((getunit(checkedposition) == null || getunit(checkedposition).owner == unit.owner || getunit(checkedposition).status == "downed") && (tileowner == 0 || tileowner == unit.owner))
                        {
                            adjacencyList.Add(checkedposition);
                        }
                    }
                    if (map.HasTile(position + down))
                    {
                        Vector3Int checkedposition = position + down;
                        levelTile checkedTile = map.GetTile<levelTile>(checkedposition);
                        int tileowner = 0;
                        if (checkedTile.controllable)
                        {
                            tileowner = map.GetInstantiatedObject(checkedposition).GetComponent<controllable_script>().owner;
                        }
                        if ((getunit(checkedposition) == null || getunit(checkedposition).owner == unit.owner || getunit(checkedposition).status == "downed") && (tileowner == 0 || tileowner == unit.owner))
                        {
                            adjacencyList.Add(checkedposition);
                        }
                    }
                    if (map.HasTile(position + right))
                    {
                        Vector3Int checkedposition = position + right;
                        levelTile checkedTile = map.GetTile<levelTile>(checkedposition);
                        int tileowner = 0;
                        if (checkedTile.controllable)
                        {
                            tileowner = map.GetInstantiatedObject(checkedposition).GetComponent<controllable_script>().owner;
                        }
                        if ((getunit(checkedposition) == null || getunit(checkedposition).owner == unit.owner || getunit(checkedposition).status == "downed") && (tileowner == 0 || tileowner == unit.owner))
                        {
                            adjacencyList.Add(checkedposition);
                        }
                    }
                    neighborlist[position] = adjacencyList;
                    break;
                #endregion
                #region case foot
                case "foot":
                    if (map.HasTile(position + left) && map.GetTile<levelTile>(position + left).type.ToString() != "lava")
                    {
                        Vector3Int checkedposition = position + left;
                        levelTile checkedTile = map.GetTile<levelTile>(checkedposition);
                        int tileowner = 0;
                        if (checkedTile.controllable)
                        {
                            tileowner = map.GetInstantiatedObject(checkedposition).GetComponent<controllable_script>().owner;
                        }
                        if ((getunit(checkedposition) == null || getunit(checkedposition).owner == unit.owner || getunit(checkedposition).status == "downed") && (tileowner == 0 || tileowner == unit.owner))
                        {
                            adjacencyList.Add(checkedposition);
                        }
                    }
                    if (map.HasTile(position + up) && map.GetTile<levelTile>(position + up).type.ToString() != "lava")
                    {
                        Vector3Int checkedposition = position + up;
                        levelTile checkedTile = map.GetTile<levelTile>(checkedposition);
                        int tileowner = 0;
                        if (checkedTile.controllable)
                        {
                            tileowner = map.GetInstantiatedObject(checkedposition).GetComponent<controllable_script>().owner;
                        }
                        if ((getunit(checkedposition) == null || getunit(checkedposition).owner == unit.owner || getunit(checkedposition).status == "downed") && (tileowner == 0 || tileowner == unit.owner))
                        {
                            adjacencyList.Add(checkedposition);
                        }
                    }
                    if (map.HasTile(position + down) && map.GetTile<levelTile>(position + down).type.ToString() != "lava")
                    {
                        Vector3Int checkedposition = position + down;
                        levelTile checkedTile = map.GetTile<levelTile>(checkedposition);
                        int tileowner = 0;
                        if (checkedTile.controllable)
                        {
                            tileowner = map.GetInstantiatedObject(checkedposition).GetComponent<controllable_script>().owner;
                        }
                        if ((getunit(checkedposition) == null || getunit(checkedposition).owner == unit.owner || getunit(checkedposition).status == "downed") && (tileowner == 0 || tileowner == unit.owner))
                        {
                            adjacencyList.Add(checkedposition);
                        }
                    }
                    if (map.HasTile(position + right) && map.GetTile<levelTile>(position + right).type.ToString() != "lava")
                    {
                        Vector3Int checkedposition = position + right;
                        levelTile checkedTile = map.GetTile<levelTile>(checkedposition);
                        int tileowner = 0;
                        if (checkedTile.controllable)
                        {
                            tileowner = map.GetInstantiatedObject(checkedposition).GetComponent<controllable_script>().owner;
                        }
                        if ((getunit(checkedposition) == null || getunit(checkedposition).owner == unit.owner || getunit(checkedposition).status == "downed") && (tileowner == 0 || tileowner == unit.owner))
                        {
                            adjacencyList.Add(checkedposition);
                        }
                    }

                    neighborlist[position] = adjacencyList;
                    break;
                #endregion
                #region case treads
                case "treads":
                    if (map.HasTile(position + left))
                    {
                        Vector3Int checkedposition = position + left;
                        levelTile checkedTile = map.GetTile<levelTile>(checkedposition);
                        int tileowner = 0;
                        if (checkedTile.controllable)
                        {
                            tileowner = map.GetInstantiatedObject(checkedposition).GetComponent<controllable_script>().owner;
                        }
                        if ((getunit(checkedposition) == null || getunit(checkedposition).owner == unit.owner || getunit(checkedposition).status == "downed") && (tileowner == 0 || tileowner == unit.owner))
                        {
                            adjacencyList.Add(checkedposition);
                        }
                    }
                    if (map.HasTile(position + up))
                    {
                        Vector3Int checkedposition = position + up;
                        levelTile checkedTile = map.GetTile<levelTile>(checkedposition);
                        int tileowner = 0;
                        if (checkedTile.controllable)
                        {
                            tileowner = map.GetInstantiatedObject(checkedposition).GetComponent<controllable_script>().owner;
                        }
                        if ((getunit(checkedposition) == null || getunit(checkedposition).owner == unit.owner || getunit(checkedposition).status == "downed") && (tileowner == 0 || tileowner == unit.owner))
                        {
                            adjacencyList.Add(checkedposition);
                        }
                    }
                    if (map.HasTile(position + down))
                    {
                        Vector3Int checkedposition = position + down;
                        levelTile checkedTile = map.GetTile<levelTile>(checkedposition);
                        int tileowner = 0;
                        if (checkedTile.controllable)
                        {
                            tileowner = map.GetInstantiatedObject(checkedposition).GetComponent<controllable_script>().owner;
                        }
                        if ((getunit(checkedposition) == null || getunit(checkedposition).owner == unit.owner || getunit(checkedposition).status == "downed") && (tileowner == 0 || tileowner == unit.owner))
                        {
                            adjacencyList.Add(checkedposition);
                        }
                    }
                    if (map.HasTile(position + right))
                    {
                        Vector3Int checkedposition = position + right;
                        levelTile checkedTile = map.GetTile<levelTile>(checkedposition);
                        int tileowner = 0;
                        if (checkedTile.controllable)
                        {
                            tileowner = map.GetInstantiatedObject(checkedposition).GetComponent<controllable_script>().owner;
                        }
                        if ((getunit(checkedposition) == null || getunit(checkedposition).owner == unit.owner || getunit(checkedposition).status == "downed") && (tileowner == 0 || tileowner == unit.owner))
                        {
                            adjacencyList.Add(checkedposition);
                        }
                    }

                    neighborlist[position] = adjacencyList;
                    break;
                #endregion
                #region case wheels
                case "wheels":
                    if (map.HasTile(position + left))
                    {
                        Vector3Int checkedposition = position + left;
                        levelTile checkedTile = map.GetTile<levelTile>(checkedposition);
                        int tileowner = 0;
                        if (checkedTile.controllable)
                        {
                            tileowner = map.GetInstantiatedObject(checkedposition).GetComponent<controllable_script>().owner;
                        }
                        if ((getunit(checkedposition) == null || getunit(checkedposition).owner == unit.owner || getunit(checkedposition).status == "downed") && (tileowner == 0 || tileowner == unit.owner))
                        {
                            adjacencyList.Add(checkedposition);
                        }
                    }
                    if (map.HasTile(position + up))
                    {
                        Vector3Int checkedposition = position + up;
                        levelTile checkedTile = map.GetTile<levelTile>(checkedposition);
                        int tileowner = 0;
                        if (checkedTile.controllable)
                        {
                            tileowner = map.GetInstantiatedObject(checkedposition).GetComponent<controllable_script>().owner;
                        }
                        if ((getunit(checkedposition) == null || getunit(checkedposition).owner == unit.owner || getunit(checkedposition).status == "downed") && (tileowner == 0 || tileowner == unit.owner))
                        {
                            adjacencyList.Add(checkedposition);
                        }
                    }
                    if (map.HasTile(position + down))
                    {
                        Vector3Int checkedposition = position + down;
                        levelTile checkedTile = map.GetTile<levelTile>(checkedposition);
                        int tileowner = 0;
                        if (checkedTile.controllable)
                        {
                            tileowner = map.GetInstantiatedObject(checkedposition).GetComponent<controllable_script>().owner;
                        }
                        if ((getunit(checkedposition) == null || getunit(checkedposition).owner == unit.owner || getunit(checkedposition).status == "downed") && (tileowner == 0 || tileowner == unit.owner))
                        {
                            adjacencyList.Add(checkedposition);
                        }
                    }
                    if (map.HasTile(position + right))
                    {
                        Vector3Int checkedposition = position + right;
                        levelTile checkedTile = map.GetTile<levelTile>(checkedposition);
                        int tileowner = 0;
                        if (checkedTile.controllable)
                        {
                            tileowner = map.GetInstantiatedObject(checkedposition).GetComponent<controllable_script>().owner;
                        }
                        if ((getunit(checkedposition) == null || getunit(checkedposition).owner == unit.owner || getunit(checkedposition).status == "downed") && (tileowner == 0 || tileowner == unit.owner))
                        {
                            adjacencyList.Add(checkedposition);
                        }
                    }

                    neighborlist[position] = adjacencyList;
                    break;
                    #endregion
            }
        }
    }

    //We call this function for every possible tile the unit can move to
    public int findOptimalMove(unitScript checkedUnit, Vector3Int startingposition)
    {
        int score = 0;
        if (checkedUnit.typeOfUnit == TypeOfUnit.infantry)
        {
            score += TrueIfwithinCaptureDistance(checkedUnit, startingposition);
            //Here we check if there is a capturable property at the checked tile
            if(map.GetTile<levelTile>(startingposition).controllable)
            {
                if(map.GetInstantiatedObject(startingposition).GetComponent<controllable_script>().owner == 0)
                {
                    score += 12;
                }
            }
        }

        return score;
    }
    public void moveUnit(unitScript unit, Vector3Int newPosition)
    {
        unit.gameObject.transform.position = map.GetCellCenterWorld(newPosition) + new Vector3(0, 0, 5);
        //GameObject unitprefab = getunitprefab(originalPosition);
        //unitprefab.transform.position = map.GetCellCenterWorld(newPosition) + new Vector3(0, 0, 5);
    }
    //we check if there is a capturable location within range and increase the score by 8, which is the difference between the minimum and maximum defenses (-3 and +4)
    private int TrueIfwithinCaptureDistance(unitScript checkedUnit, Vector3Int startingposition)
    {
        Dictionary<Vector3Int, int> moveswithinthissquare = findPossibleMoves(startingposition, checkedUnit);
        List<Vector3Int> Positions = new List<Vector3Int>(moveswithinthissquare.Keys);
        foreach(Vector3Int currentTile in Positions)
        {
            if(map.GetTile<levelTile>(currentTile) != null)
            {
                if (map.GetTile<levelTile>(currentTile).controllable)
                {
                    if (map.GetInstantiatedObject(currentTile).GetComponent<controllable_script>().owner == 0)
                    {
                        return 8;
                    }
                }
            }
        }
        return 0;
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
    public GameObject getunitprefab(Vector3 position, bool screen = true)
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
            return selection;
        }
        //returns null if it did not find a unit
        else
        {
            return null;
        }
    }
}
