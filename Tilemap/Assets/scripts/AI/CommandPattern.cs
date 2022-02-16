using System.Collections.Generic;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using UnityEngine.EventSystems;

public class CommandPattern : MonoBehaviour
{
    public MapManager mapmanager;
    public SelectionManager selectionmanager;
    private int activeplayer = 1;
    private int maxplayers = 2;
    public Tilemap map, units, conditions;
    public unitScript some;
    public TextMeshProUGUI unitname;
    private Vector3 PlayerCameraCoordinates;
    private float PlayerCameraZoom;
    public UIInputWindowForBarracksName accept;
    private Camera _mainCamera;
    Dictionary<unitScript, Dictionary<Vector3Int, (int,string)>> possibleMovesForAllUnits = new Dictionary<unitScript, Dictionary<Vector3Int, (int,string)>>();
    Dictionary<unitScript, (Vector3Int, String)> targetPositionsandActions = new Dictionary<unitScript, (Vector3Int, string)>();
    Queue<unitScript> unitsToMove = new Queue<unitScript>();
    private bool firstTurn = true;
    private List<GameObject> selectedbuildables;
    public Button endTurnButton;
    private gridCombat combatManager;
    public event Action<Vector3Int, Vector3Int> Oncombatstart;
    private string unitstate = "idle";
    private GameObject unitprefab;
    Stack<Vector3Int> path = new Stack<Vector3Int>();
    GameObject[] controllables;
    private bool movingaUnit = false;
    private bool finishedMovingAllUnits = false;

    void Update()
    {
        if(activeplayer != 2)
        {
            return;
        }
        if(unitsToMove.Count == 0 && movingaUnit)
        {
            Debug.Log("we entered here");
            movingaUnit = false;
            finishedMovingAllUnits = true;
        }
        if(movingaUnit)
        {
            unitScript currentMovingUnit = unitsToMove.Peek();
            moveUnit(currentMovingUnit, targetPositionsandActions[currentMovingUnit].Item1, targetPositionsandActions[currentMovingUnit].Item2);
            movingaUnit = false;
        }
        if(selectionmanager.unitstate == "thinking")
        {
            unitScript currentMovingUnit = unitsToMove.Dequeue();
            takeAction(currentMovingUnit, targetPositionsandActions[currentMovingUnit].Item1, targetPositionsandActions[currentMovingUnit].Item2);
        }
        if(finishedMovingAllUnits)
        {
            Debug.Log("we entered to the finished, for some reason");
            finishedMovingAllUnits = false;
            findOptimalBuild(controllables);
            endTurn();
            _mainCamera.transform.position = PlayerCameraCoordinates;
            _mainCamera.orthographicSize = PlayerCameraZoom;
        }
    }
    private void Start()
    {
        _mainCamera = Camera.main;
        combatManager = new gridCombat();
        combatManager.Start();
        combatManager.map = map;
        combatManager.conditions = conditions;
    }

    private void Reset()
    {
        targetPositionsandActions.Clear();
        possibleMovesForAllUnits.Clear();
        movingaUnit = false;
        finishedMovingAllUnits = false;
        unitsToMove.Clear();
    }
    private void takeTurn()
    {
        controllables = GameObject.FindGameObjectsWithTag("Controllable");
        moveUnits();
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
        {
            Reset();
            PlayerCameraCoordinates = _mainCamera.transform.position;
            PlayerCameraZoom = _mainCamera.orthographicSize;
            takeTurn();
        }
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

    //Here we first find all possible moves, then optimize the movement, and then set the movingaUnit flag to true. Then, on Update we trigger the movement of 
    //the next unit in the selectionManager. After that unit moves and takes its action, we move the next one, and so forth.
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
                    //Here we add the score calculated for each space (Key) of the currentmovingunit. The Item1 has the score and Item2 has the action
                    (int, string) scoreAndAction = findOptimalMove(currentMovingUnit, Key);
                    scoreAndAction.Item1 += possibleMovesForAllUnits[currentMovingUnit][Key].Item1;
                    possibleMovesForAllUnits[currentMovingUnit][Key] = scoreAndAction;
                }
            }
        }
        //Here we assume that we already established the score for each movement, and take the best possible move
        foreach (unitScript currentMovingUnit in possibleMovesForAllUnits.Keys)
        {
            //Vector3Int currentPosition = gridPosition(currentMovingUnit.gameObject.transform.position);
            Vector3Int targetPosition = new Vector3Int(0, 0, 0);
            int MaxValue = -10;
            string takenAction = "wait";
            //possibleMovesForAllUnits[currentMovingUnit].Keys is a list that Vector3Int values and contains all the possible tiles that the currentmovingunit can move to
            foreach (Vector3Int possibleTargetPosition in possibleMovesForAllUnits[currentMovingUnit].Keys)
            {
                //Here we check that our current value is the max value we have and that it does not have a unit on it already
                if(possibleMovesForAllUnits[currentMovingUnit][possibleTargetPosition].Item1 > MaxValue && !visitedTiles.Contains(possibleTargetPosition))
                {
                    if(map.GetTile<levelTile>(possibleTargetPosition).controllable)
                    {
                        //Here we just check that we are not moving to our own barracks
                        if(!(map.GetTile<levelTile>(possibleTargetPosition).type == tileType.barracks && map.GetInstantiatedObject(possibleTargetPosition).GetComponent<controllable_script>().owner == 2))
                        {
                            MaxValue = possibleMovesForAllUnits[currentMovingUnit][possibleTargetPosition].Item1;
                            targetPosition = possibleTargetPosition;
                            takenAction = possibleMovesForAllUnits[currentMovingUnit][possibleTargetPosition].Item2;
                            unitsToMove.Enqueue(currentMovingUnit);
                        }
                    }
                    else
                    {
                        MaxValue = possibleMovesForAllUnits[currentMovingUnit][possibleTargetPosition].Item1;
                        targetPosition = possibleTargetPosition;
                        takenAction = possibleMovesForAllUnits[currentMovingUnit][possibleTargetPosition].Item2;
                    }
                }
            }
            visitedTiles.Add(targetPosition);
            targetPositionsandActions[currentMovingUnit] = (targetPosition, takenAction);

        }
        movingaUnit = true;
    }


    public Dictionary<Vector3Int, (int,string)> findPossibleMoves(Vector3Int position, unitScript unit)
    {
        Dictionary<Vector3Int, (int,string)> possibleMoves = new Dictionary<Vector3Int, (int,string)> ();
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
            possibleMoves[selectable] = (map.GetTile<levelTile>(selectable).defense/5,"wait");
        }
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

    //We call this function for every possible tile the unit can move to. We return the score and the desired action.
    public (int,string) findOptimalMove(unitScript checkedUnit, Vector3Int startingposition)
    {
        int score = 0;
        string action = "wait";
        if (checkedUnit.typeOfUnit == TypeOfUnit.infantry)
        {
            score += TrueIfwithinCaptureDistance(checkedUnit, startingposition);
            //Here we check if there is a capturable property at the checked tile
            if(map.GetTile<levelTile>(startingposition).controllable)
            {
                if(map.GetInstantiatedObject(startingposition).GetComponent<controllable_script>().owner == 0)
                {
                    score += 12;
                    action = "capture";
                }
            }
        }

        return (score, action);
    }
    
    public void moveUnit(unitScript unit, Vector3Int newPosition, string  Action)
    {
        Debug.Log(unit + "moved to " + newPosition + " and " + Action);

        GameObject unitprefab = unit.gameObject;
        selectionmanager.unit = unit;
        selectionmanager.unitprefab = unitprefab;
        selectionmanager.newposition = newPosition;
        Vector3Int currentPosition = gridPosition(unit);
        selectionmanager.currentposition = currentPosition; 
        //Setting the camera to the unit we are about to move
        float cam_x = unit.gameObject.transform.position.x;
        float cam_y = unit.gameObject.transform.position.y;
        float cam_z = _mainCamera.transform.position.z;
        _mainCamera.transform.position = new Vector3(cam_x, cam_y, cam_z);
        _mainCamera.orthographicSize = 5.5f;
        StartCoroutine(waitSeconds(1600));
        selectionmanager.selectUnit(unit, currentPosition);
        StartCoroutine(waitSeconds(1600));
        selectionmanager.getPath(currentPosition, newPosition);
        /*
        GameObject unitprefab = unit.gameObject;
        selectionmanager.unit = unit;
        selectionmanager.unitprefab = unitprefab;
        selectionmanager.newposition = newPosition;
        Vector3Int currentPosition = gridPosition(unit);
        selectionmanager.currentposition = currentPosition;

        float cam_x = unit.gameObject.transform.position.x;
        float cam_y = unit.gameObject.transform.position.y;
        float cam_z = _mainCamera.transform.position.z;
        _mainCamera.transform.position = new Vector3(cam_x, cam_y, cam_z);
        StartCoroutine(waitSeconds(.7f));
        selectionmanager.selectUnit(unit, currentPosition);
        StartCoroutine(waitSeconds(.7f));
        selectionmanager.getPath(currentPosition, newPosition);
        //GameObject unitprefab = getunitprefab(originalPosition);
        //unitprefab.transform.position = map.GetCellCenterWorld(newPosition) + new Vector3(0, 0, 5);
        */
    }

    public void takeAction(unitScript unit, Vector3Int newPosition, string Action)
    {
        switch (Action)
        {
            #region wait
            case "wait":
                selectionmanager.onWait();
                break;
            #endregion
            #region capture
            case "capture":
                selectionmanager.onCap();
                break;
            #endregion
            default:
                Debug.Log(Action);
                break;
        }
        movingaUnit = true;
    }
    //we check if there is a capturable location within range and increase the score by 8, which is the difference between the minimum and maximum defenses (-3 and +4)
    private int TrueIfwithinCaptureDistance(unitScript checkedUnit, Vector3Int startingposition)
    {
        Dictionary<Vector3Int, (int,string)> moveswithinthissquare = findPossibleMoves(startingposition, checkedUnit);
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
    public Vector3Int gridPosition(unitScript unit)
    {
        return map.WorldToCell(unit.gameObject.transform.position);
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
    private void endTurn()
    {
        possibleMovesForAllUnits.Clear();
        Reset();
        endTurnButton.onClick.Invoke();
    }
    public IEnumerator waitSeconds(float waitingTime)
    {
        yield return new WaitForSeconds(waitingTime);

    }

    //these two method cooperate to return true if the targetposition is within the unit's aura range (position is the unit with aura's position)
    public bool findifwithinrange(Vector3Int targetposition, unitScript unit, Vector3Int position)
    {
        Dictionary<Vector3Int, List<Vector3Int>> neighborlist = new Dictionary<Vector3Int, List<Vector3Int>>();
        Dictionary<Vector3Int, Vector3Int> parentlist = new Dictionary<Vector3Int, Vector3Int>();
        Dictionary<Vector3Int, int> distancelist = new Dictionary<Vector3Int, int>();
        Dictionary<Vector3Int, bool> visitlist = new Dictionary<Vector3Int, bool>();
        List<Vector3Int> selectableTiles = new List<Vector3Int>();
        Stack<Vector3Int> path = new Stack<Vector3Int>();
        units.ClearAllTiles();
        if(targetposition == position)
        {
            return false;
        }

        //getting the attackable tiles
        foreach (var posi in map.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(posi.x, posi.y, posi.z);
            if (map.HasTile(localPlace))
            {
                List<Vector3Int> adjacencyList = new List<Vector3Int>();
                Vector3Int left = new Vector3Int(-1, 0, 0);
                Vector3Int right = new Vector3Int(1, 0, 0);
                Vector3Int up = new Vector3Int(0, 1, 0);
                Vector3Int down = new Vector3Int(0, -1, 0);
                if (map.HasTile(localPlace + left))
                {
                    adjacencyList.Add(localPlace + left);
                }
                if (map.HasTile(localPlace + up))
                {
                    adjacencyList.Add(localPlace + up);
                }
                if (map.HasTile(localPlace + down))
                {
                    adjacencyList.Add(localPlace + down);
                }
                if (map.HasTile(localPlace + right))
                {
                    adjacencyList.Add(localPlace + right);
                }
                neighborlist[localPlace] = adjacencyList;
                visitlist[localPlace] = false;
                parentlist[localPlace] = Vector3Int.zero;
                distancelist[localPlace] = 1;
            }
        }

        //here we initialize the queue where we will get all the selectable tiles
        Queue<Vector3Int> process = new Queue<Vector3Int>();
        visitlist[position] = true;
        distancelist[position] = 0;
        process.Enqueue(position);
        while (process.Count > 0)
        {
            Vector3Int pos = process.Dequeue();
            selectableTiles.Add(pos);
            foreach (Vector3Int vector in neighborlist[pos])
            {
                if (!visitlist[vector] && (distancelist[pos] + distancelist[vector]) <= unit.aurarange)
                {
                    parentlist[vector] = pos;
                    visitlist[vector] = true;
                    distancelist[vector] = distancelist[pos] + distancelist[vector];
                    process.Enqueue(vector);
                }
            }
        }

        foreach (Vector3Int selectable in selectableTiles)
        {
            if (selectable == targetposition)
            {
                Debug.Log(getunit(targetposition));
                if (unit.auracheck(getunit(targetposition)))
                    return true;
            }
        }
        return false;
    }

    public List<Vector3Int> findifwithinrange(unitScript unit, Vector3Int position)
    {
        Dictionary<Vector3Int, List<Vector3Int>> neighborlist = new Dictionary<Vector3Int, List<Vector3Int>>();
        Dictionary<Vector3Int, Vector3Int> parentlist = new Dictionary<Vector3Int, Vector3Int>();
        Dictionary<Vector3Int, int> distancelist = new Dictionary<Vector3Int, int>();
        Dictionary<Vector3Int, bool> visitlist = new Dictionary<Vector3Int, bool>();
        List<Vector3Int> selectableTiles = new List<Vector3Int>();
        Stack<Vector3Int> path = new Stack<Vector3Int>();
        List<Vector3Int> affectedUnits = new List<Vector3Int>();
        //creating the neighborlist for attackable tiles
        neighborlist.Clear();
        selectableTiles.Clear();
        path.Clear();
        distancelist.Clear();
        units.ClearAllTiles();


        //getting the attackable tiles
        foreach (var posi in map.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(posi.x, posi.y, posi.z);
            if (map.HasTile(localPlace))
            {
                List<Vector3Int> adjacencyList = new List<Vector3Int>();
                Vector3Int left = new Vector3Int(-1, 0, 0);
                Vector3Int right = new Vector3Int(1, 0, 0);
                Vector3Int up = new Vector3Int(0, 1, 0);
                Vector3Int down = new Vector3Int(0, -1, 0);
                if (map.HasTile(localPlace + left))
                {
                    adjacencyList.Add(localPlace + left);
                }
                if (map.HasTile(localPlace + up))
                {
                    adjacencyList.Add(localPlace + up);
                }
                if (map.HasTile(localPlace + down))
                {
                    adjacencyList.Add(localPlace + down);
                }
                if (map.HasTile(localPlace + right))
                {
                    adjacencyList.Add(localPlace + right);
                }
                neighborlist[localPlace] = adjacencyList;
                visitlist[localPlace] = false;
                parentlist[localPlace] = Vector3Int.zero;
                distancelist[localPlace] = 1;
            }
        }

        //here we initialize the queue where we will get all the selectable tiles
        Queue<Vector3Int> process = new Queue<Vector3Int>();
        visitlist[position] = true;
        distancelist[position] = 0;
        process.Enqueue(position);
        while (process.Count > 0)
        {
            Vector3Int pos = process.Dequeue();
            selectableTiles.Add(pos);
            foreach (Vector3Int vector in neighborlist[pos])
            {
                if (!visitlist[vector] && (distancelist[pos] + distancelist[vector]) <= unit.aurarange)
                {
                    parentlist[vector] = pos;
                    visitlist[vector] = true;
                    distancelist[vector] = distancelist[pos] + distancelist[vector];
                    process.Enqueue(vector);
                }
            }
        }

        foreach (Vector3Int selectable in selectableTiles)
        {
            if (getunit(selectable) != null)
            {
                if (unit.auracheck(getunit(selectable)))
                {
                    affectedUnits.Add(selectable);
                }
            }
        }
        return affectedUnits;
    }


    public class gridCombat : MonoBehaviour
    {
        public Tilemap map, conditions;

        GameObject attacker;
        unitScript attackerScript;
        GameObject defender;
        unitScript defenderScript;

        public void Start()
        {
            //here we register this object as a listener to the combat initiation in the selection manager
            GameObject.FindGameObjectWithTag("SelectionManager").GetComponent<SelectionManager>().Oncombatstart += OncombatHappening;
            GameObject.FindGameObjectWithTag("SelectionManager").GetComponent<SelectionManager>().Oncombathover += OnCombatHover;
        }
        public void OncombatHappening(Vector3Int attackposition, Vector3Int defendposition)
        {
            attackerScript = getunit(attackposition);
            switch (attackerScript.ability)
            {
                //this is the god of small thing's ability, where all units gain 2 levels
                case "demonstrate":
                    foreach (GameObject unitObject in GameObject.FindGameObjectsWithTag("Unit"))
                    {
                        if (unitObject.GetComponent<unitScript>().owner == attackerScript.owner)
                        {
                            unitObject.GetComponent<unitScript>().gainXP();
                            unitObject.GetComponent<unitScript>().gainXP();
                        }
                    }
                    break;
                default:
                    break;
            }
            //This if happens if the unit is attacking a tile
            if (getunit(defendposition) == null)
            {
                int damage = attackerScript.attackdamage;
                damage = (int)(damage * attackerScript.HP / attackerScript.maxHP * (1 + attackerScript.level / 10) * (1 + GlobalModifiers(attackerScript.owner)[0]));
                controllable_script attackedTile = map.GetInstantiatedObject(defendposition).GetComponent<controllable_script>();
                if (attackerScript.ability == "siege")
                {
                    damage = 500;
                }
                attackedTile.HP -= damage;
                if (attackedTile.HP <= 0)
                {
                    attackedTile.ownerloss();
                }
                else
                {
                    attackedTile.healthChanged();
                    if (checkifneighbors(attackposition, defendposition))
                    {
                        attackerScript.HP -= 30;
                        attackerScript.healthChanged();
                    }
                }
            }

            ////////////////////
            //This if happens if the unit is attacking another unit
            else
            {
                defenderScript = getunit(defendposition);
                bool canCounterAttack = false;

                //we check if the units have first strike. If the defender does, it deals damage first. Also, we check if the 
                //defender can counterattack in case it survives.

                if ((defenderScript._attacktype == "melee" || defenderScript.firstStrike) && checkifneighbors(attackposition, defendposition))
                {
                    canCounterAttack = true;
                    if (defenderScript.firstStrike && !attackerScript.firstStrike)
                    {
                        attackerScript = getunit(defendposition);
                        defenderScript = getunit(attackposition);
                        Vector3Int temporal = attackposition;
                        attackposition = defendposition;
                        defendposition = temporal;
                    }
                }

                //the attacker deals damage to the defender (status changes also happen here)
                defenderScript.HP -= calculateDamage(attackerScript, defenderScript, defendposition);
                defenderScript.status = changeStatus(attackerScript, defenderScript);


                //if the defender survives, and can counterattack, here we add a counterattack on the combat
                if (defenderScript.HP > 0 && canCounterAttack && defenderScript.status != "stunned")
                {
                    attackerScript.HP -= calculateDamage(defenderScript, attackerScript, defendposition);
                    defenderScript.status = changeStatus(defenderScript, attackerScript);
                    //The rest of the function adds the animation of the shooting from the attacker, who then calls the counterattack on its enemy.
                    //The counterattack plays and then calls the damage animation on the attackerscript
                    attackerScript.onCombat(defenderScript);

                    if (attackerScript.HP <= 0)
                    {
                        if (defenderScript.cankill && defenderScript.ability != "capture")
                        {
                            attackerScript.Destroyed();
                            defenderScript.downedanotherUnit();
                        }
                        else
                        {
                            if (!defenderScript.cankill)
                            {
                                attackerScript.Downed();
                                defenderScript.downedanotherUnit();
                            }
                            if (defenderScript.ability == "capture")
                            {
                                attackerScript.HP = 10;
                                attackerScript.ownerChange(defenderScript.owner);
                                attackerScript.healthChanged();
                                attackerScript.exhausted = true;
                                attackerScript.sprite.color = new Color(.6f, .6f, .6f);
                            }
                        }
                    }
                }
                //no counterattack
                else
                {
                    attackerScript.onCombatWOCA(defenderScript);
                }
                if (defenderScript.HP <= 0)
                {
                    if (attackerScript.cankill && attackerScript.ability != "capture")
                    {
                        defenderScript.Destroyed();
                        attackerScript.downedanotherUnit();
                    }
                    else
                    {
                        if (!attackerScript.cankill)
                        {
                            defenderScript.Downed();
                            attackerScript.downedanotherUnit();
                        }
                        if (attackerScript.ability == "capture")
                        {
                            defenderScript.HP = 10;
                            defenderScript.ownerChange(attackerScript.owner);
                            defenderScript.healthChanged();
                            defenderScript.exhausted = true;
                            defenderScript.sprite.color = new Color(.6f, .6f, .6f);
                        }
                    }
                }
            }
        }
        public void OnCombatHover(Vector3Int attackposition, Vector3Int defendposition, GameObject preview)
        {
            attackerScript = getunit(attackposition);
            defenderScript = getunit(defendposition);
            string temporalstatus = defenderScript.status;
            preview = preview.transform.GetChild(0).gameObject;
            int defenderhealthchange = calculateDamage(attackerScript, defenderScript, defendposition);
            defenderScript.status = changeStatus(attackerScript, defenderScript);
            preview.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = defenderhealthchange.ToString();
            defenderScript.HP -= defenderhealthchange;
            preview.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = calculateDamage(defenderScript, attackerScript, attackposition).ToString();
            defenderScript.HP += defenderhealthchange;
            defenderScript.status = temporalstatus;
        }
        public unitScript getunit(Vector3 position, bool screen = true)
        {

            if (!screen)
            {
                position = Camera.main.WorldToScreenPoint(position);
            }
            var ray = Camera.main.ScreenPointToRay(position);
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
        //tries to get a unit given a gridposition
        public unitScript getunit(Vector3Int position)
        {
            return getunit(Camera.main.WorldToScreenPoint(map.GetCellCenterWorld(position)));
        }
        public Vector3 worldPosition(Vector3Int gridposition)
        {
            return map.CellToWorld(gridposition);
        }
        public GameObject getunitprefab(Vector3 position, bool screen = true)
        {
            if (!screen)
            {
                position = Camera.main.WorldToScreenPoint(position);
            }
            var ray = Camera.main.ScreenPointToRay(position);
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
        private bool checkifneighbors(Vector3Int pos1, Vector3Int pos2)
        {
            Vector3Int left = new Vector3Int(-1, 0, 0);
            Vector3Int right = new Vector3Int(1, 0, 0);
            Vector3Int up = new Vector3Int(0, 1, 0);
            Vector3Int down = new Vector3Int(0, -1, 0);

            if (pos1 + left == pos2 || pos1 + right == pos2 || pos1 + up == pos2 || pos1 + down == pos2)
                return true;
            else
                return false;
        }
        public int calculateDamage(unitScript attackingunit, unitScript defendingunit, Vector3Int defendposition)
        {

            if (attackingunit.status == "stunned" || attackingunit.status == "recovered")
                return 0;

            List<string> attackingadv = new List<string>();
            List<string> defendingresist = new List<string>();
            List<string> defendingvul = new List<string>();
            int damage = attackingunit.attackdamage;
            if (attackingunit.advantages != null)
            {
                attackingadv = attackingunit.advantages;
            }
            if (defendingunit.resistances != null)
            {
                defendingresist = defendingunit.resistances;
            }
            if (defendingunit.vulnerabilities != null)
            {
                defendingvul = defendingunit.vulnerabilities;
            }
            foreach (string adv in attackingadv)
            {
                if (adv == defendingunit.typeOfUnit.ToString() || adv == defendingunit.movementtype.ToString() || adv == defendingunit._attacktype || adv == (defendingunit._attacktype + defendingunit.movementtype) || adv == (defendingunit._attacktype + defendingunit.typeOfUnit.ToString()) || adv == (defendingunit.movementtype.ToString() + defendingunit.typeOfUnit.ToString()))
                {
                    damage *= 2;
                }
            }
            foreach (string vul in defendingvul)
            {
                if (vul == attackingunit.typeOfUnit.ToString() || vul == attackingunit.movementtype.ToString() || vul == attackingunit._attacktype || vul == (attackingunit._attacktype + attackingunit.movementtype) || vul == (attackingunit._attacktype + attackingunit.typeOfUnit.ToString()) || vul == (attackingunit.movementtype.ToString() + attackingunit.typeOfUnit.ToString()))
                {
                    damage *= 2;
                }
            }
            foreach (string res in defendingresist)
            {
                if (attackingunit.typeOfUnit.ToString() == res || attackingunit.movementtype.ToString() == res || attackingunit._attacktype == res || res == (attackingunit._attacktype + attackingunit.movementtype) || res == (attackingunit._attacktype + attackingunit.typeOfUnit.ToString()) || res == (attackingunit.movementtype.ToString() + attackingunit.typeOfUnit.ToString()))
                    damage /= 2;
            }
            levelTile Tile = map.GetTile<levelTile>(defendposition);
            int tiledefense = Tile.defense;
            damage = (int)(damage * attackingunit.HP / attackingunit.maxHP * (1 + attackingunit.level / 10) * (1 + GlobalModifiers(attackingunit.owner)[0]) * (1 - GlobalModifiers(defendingunit.owner)[1]));
            damage -= tiledefense;

            if (damage < 0)
                damage = 0;
            return damage;
        }

        public string changeStatus(unitScript attackingunit, unitScript defendingunit)
        {
            string newStatus = null;
            foreach (string stun in attackingunit.stuns)
            {
                if (stun == defendingunit.typeOfUnit.ToString() || stun == defendingunit.movementtype.ToString() || stun == defendingunit._attacktype)
                {
                    newStatus = "stunned";
                }
            }
            return newStatus;
        }

        //on [0] returns global damage (bonfires, etc), and on [1] returns defense. For now, only bonfires are implemented.
        public double[] GlobalModifiers(int owner)
        {
            double[] modifiers = new double[2];
            modifiers[0] = 0;
            modifiers[1] = 0;
            foreach (var posi in map.cellBounds.allPositionsWithin)
            {
                Vector3Int localPlace = new Vector3Int(posi.x, posi.y, posi.z);
                if (map.HasTile(localPlace))
                {
                    levelTile Tile = map.GetTile<levelTile>(localPlace);
                    if (Tile.controllable)
                    {
                        int tileowner = map.GetInstantiatedObject(localPlace).GetComponent<controllable_script>().owner;
                        if (owner == tileowner)
                        {
                            if (Tile.type == tileType.bonfire)
                            {
                                modifiers[0] += .1;
                            }
                        }
                    }
                }
            }
            return modifiers;
        }
    }
}
