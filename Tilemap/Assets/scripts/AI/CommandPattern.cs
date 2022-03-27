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
    public TextMeshProUGUI unitname;
    private Vector3 PlayerCameraCoordinates;
    private float PlayerCameraZoom;
    public UIInputWindowForBarracksName accept;
    private Camera _mainCamera;
    Dictionary<unitScript, Dictionary<Vector3Int, (decimal,string, Vector3Int)>> possibleMovesForAllUnits = new Dictionary<unitScript, Dictionary<Vector3Int, (decimal,string, Vector3Int)>>();
    Dictionary<unitScript, (Vector3Int, String, Vector3Int)> targetPositionsandActions = new Dictionary<unitScript, (Vector3Int, string, Vector3Int)>();
    Dictionary<unitScript, Dictionary<Vector3Int, List<(decimal, Vector3Int, bool)>>> PossibleAttacksandScores = new Dictionary<unitScript, Dictionary<Vector3Int, List<(decimal, Vector3Int, bool)>>>();
    List<unitScript> unitsToMove = new List<unitScript>();
    int numberofunitmoved = 0;
    private bool firstTurn = true;
    private List<GameObject> selectedbuildables;
    public Button endTurnButton;
    //private gridCombat combatManager;
    GameObject[] controllables;
    private bool movingaUnit = false;
    private bool finishedMovingAllUnits = false;
    private int movementStep = 0;
    public float waitHowLong = .6f;
    private bool flagforFlow = false;
    //The scores for the tile defense go from -4 to +3
    public decimal cancapture = 12;
    public decimal movesIntoCaptureRange = 8;
    public int maxUnitCost = 7000;
    //while calculating the score, we multiply the value difference between the attacking unit and the defending unit by this number
    public decimal attackedByEnemy = 8;
    public decimal attacksAnEnemy = 8;
    public decimal attacksAnEnemyThatWasAlreadyAttacked = 1.5m;
    //This we multiply by the unit's total value
    public decimal attacksAndDestroysAnEnemy = 1.5m;
    public decimal AttackandGetDestroyed = 1.5m;

    void Update()
    {
        if(activeplayer != 2)
        {
            return;
        }
        if(numberofunitmoved <= 0 && movingaUnit)
        {
            movingaUnit = false;
            finishedMovingAllUnits = true;
        }
        #region MoveUnit
        //Here we move the unit. We move the movingaUnit counter when we perform the action.
        if(movingaUnit)
        {
            movingaUnit = false;
            StartCoroutine(waitSeconds(waitHowLong/2));
        }
        if(movementStep == 1 && flagforFlow)
        {
            flagforFlow = false;
            unitScript currentMovingUnit = unitsToMove[numberofunitmoved-1];
            CenterCameraonUnit(currentMovingUnit, targetPositionsandActions[currentMovingUnit].Item1, targetPositionsandActions[currentMovingUnit].Item2);

        }
        if(movementStep == 2 && flagforFlow)
        {
            flagforFlow = false;
            unitScript currentMovingUnit = unitsToMove[numberofunitmoved-1];
            SelectUnit(currentMovingUnit);
        }
        if(movementStep == 3 && flagforFlow)
        {
            flagforFlow = false;
            unitScript currentMovingUnit = unitsToMove[numberofunitmoved-1];
            moveUnit(currentMovingUnit, targetPositionsandActions[currentMovingUnit].Item1);
        }
        if(movementStep == 4 && flagforFlow && selectionmanager.unitstate == "thinking" )
        {
            flagforFlow = false;
            StartCoroutine(waitSeconds(waitHowLong * .8f));
        }
        if(movementStep == 5 && flagforFlow)
        {
            flagforFlow = false;
            movementStep = 0;
            unitScript currentMovingUnit = unitsToMove[numberofunitmoved-1];
            numberofunitmoved--;
            takeAction(currentMovingUnit, targetPositionsandActions[currentMovingUnit].Item1, targetPositionsandActions[currentMovingUnit].Item2, targetPositionsandActions[currentMovingUnit].Item3);
        }
        #endregion
        if(finishedMovingAllUnits)
        {
            finishedMovingAllUnits = false;
            findOptimalBuild(controllables);
            endTurn();
            _mainCamera.transform.position = PlayerCameraCoordinates;
            _mainCamera.orthographicSize = PlayerCameraZoom;
        }
    }
    private void Start()
    {
        Debug.Log("before normalization: " + AttackandGetDestroyed);
        _mainCamera = Camera.main;
        attackedByEnemy /= maxUnitCost;
        attacksAnEnemy /= maxUnitCost;
        attacksAnEnemyThatWasAlreadyAttacked /= maxUnitCost;
        attacksAndDestroysAnEnemy /= maxUnitCost;
        AttackandGetDestroyed /= maxUnitCost;
        Debug.Log("after normalization: " + AttackandGetDestroyed);
        /*combatManager = new gridCombat();
        combatManager.Start();
        combatManager.map = map;
        combatManager.conditions = conditions;*/
    }

    private void Reset()
    {
        targetPositionsandActions.Clear();
        possibleMovesForAllUnits.Clear();
        PossibleAttacksandScores.Clear();
        movingaUnit = false;
        finishedMovingAllUnits = false;
        unitsToMove.Clear();
        numberofunitmoved = 0;
        flagforFlow = false;
        movementStep = 0;
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
            {
                if(getunit(gridPosition(tile.transform.position)) == null)
                {
                    myBarracks.Add(tile);
                }
            }
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
                    (decimal, string, Vector3Int) scoreAndAction = findOptimalMove(currentMovingUnit, Key);
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
            Vector3Int targetTile = new Vector3Int(0, 0, 0);
            decimal MaxValue = 0;
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
                            targetTile = possibleMovesForAllUnits[currentMovingUnit][possibleTargetPosition].Item3;
                        }
                    }
                    else
                    {
                        MaxValue = possibleMovesForAllUnits[currentMovingUnit][possibleTargetPosition].Item1;
                        targetPosition = possibleTargetPosition;
                        takenAction = possibleMovesForAllUnits[currentMovingUnit][possibleTargetPosition].Item2;
                        targetTile = possibleMovesForAllUnits[currentMovingUnit][possibleTargetPosition].Item3;
                    }
                }
            }

            unitsToMove.Add(currentMovingUnit);
            numberofunitmoved++;
            Debug.Log("score of " + currentMovingUnit + " at " + gridPosition(currentMovingUnit.gameObject.transform.position) + " is " + MaxValue + " to " + targetPosition +" and" + possibleMovesForAllUnits[currentMovingUnit][targetPosition].Item2 + "to target " + possibleMovesForAllUnits[currentMovingUnit][targetPosition].Item3);
            visitedTiles.Add(targetPosition);
            targetPositionsandActions[currentMovingUnit] = (targetPosition, takenAction, targetTile);
        }
        movingaUnit = true;
    }


//This returns all the scores, actions and targets (values) for when the unit moves into each position
    public Dictionary<Vector3Int, (decimal,string, Vector3Int)> findPossibleMoves(Vector3Int position, unitScript unit)
    {
        Dictionary<Vector3Int, (decimal,string, Vector3Int)> possibleMoves = new Dictionary<Vector3Int, (decimal,string, Vector3Int)> ();
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
            possibleMoves[selectable] = ((decimal) (map.GetTile<levelTile>(selectable).defense / 5), "wait", selectable);
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
    public (decimal,string, Vector3Int) findOptimalMove(unitScript checkedUnit, Vector3Int startingposition)
    {
        decimal score = 0;
        decimal scoreforattacking = 0;
        string action = "wait";
        Vector3Int ActionTarget = startingposition;
        if(checkedUnit.attackandmove)
        {
            //Here we create a list with all the units that can be attacked from startingposition

            Dictionary<Vector3Int, List<(decimal, Vector3Int, bool)>> checkifempty = checkAttackables(checkedUnit, startingposition);
            if(checkifempty.Any())
            {
                PossibleAttacksandScores[checkedUnit] = checkifempty;
            }
            (decimal, Vector3Int, string) ScoreTargetAction = FindOptimalAttack(checkedUnit, startingposition);
            score = ScoreTargetAction.Item1;
            ActionTarget = ScoreTargetAction.Item2;
            action = ScoreTargetAction.Item3;

            if (checkedUnit.typeOfUnit == TypeOfUnit.infantry)
            {
                //Here we check if there is a capturable property at the checked tile
                if(map.GetTile<levelTile>(startingposition).controllable)
                {
                    if(map.GetInstantiatedObject(startingposition).GetComponent<controllable_script>().owner == 0)
                    {
                        if(cancapture >= scoreforattacking)
                        {
                            score = cancapture;
                            action = "capture";
                            ActionTarget = startingposition;
                        }
                    }
                }


                //Here, if the unit is infantry, we check if it can capture a tile on the next turn
                if (TrueIfwithinCaptureDistance(checkedUnit, startingposition))
                    score += movesIntoCaptureRange;
            }
        }


        ///////still have to add for units that cant attack and move
       return(score, action, ActionTarget);

    }
    
    //this method returns  the score for the attack, and the target for the attack
    public Dictionary<Vector3Int, List<(decimal, Vector3Int, bool)>> checkAttackables(unitScript unit, Vector3Int attackingPosition)
    {
        Vector3Int target = new Vector3Int(0,0,0);
        decimal score = 0;
        bool destroys = false;
        Dictionary<Vector3Int, List<(decimal, Vector3Int, bool)>> ScoreTargetDestoys = new Dictionary<Vector3Int, List<(decimal, Vector3Int, bool)>>();
        List<(decimal, Vector3Int, bool)> temporalList = new List<(decimal, Vector3Int, bool)>();
        foreach (var posi in map.cellBounds.allPositionsWithin)
        {
            score = 0;
            destroys = false;
            int distanceFromAttackingUnit = Math.Abs(posi.x - attackingPosition.x) + Math.Abs(posi.y - attackingPosition.y);
            if(distanceFromAttackingUnit > unit.attackrange || getunit(posi) == null)
            {
                continue;
            }
            if(getunit(posi).owner == unit.owner)
            {
                continue;
            }
            //Here we calculate the score for attacking the unit at position posi from attackingPosition
            
            target = posi;
            unitScript attackerScript = unit;
            unitScript defenderScript = getunit(posi);
            string temporalstatus = defenderScript.status;
            int defenderhealthchange = calculateDamage(attackerScript, defenderScript, attackingPosition, posi);
            defenderScript.status = changeStatus(attackerScript, defenderScript);
            defenderScript.HP -= defenderhealthchange;
            int attackingUnitHPchange  = calculateDamage(defenderScript, attackerScript, posi, attackingPosition);
            defenderScript.HP += defenderhealthchange;
            defenderScript.status = temporalstatus;
            if(attackingUnitHPchange >= unit.HP)
            {
                score -= unit.foodCost * AttackandGetDestroyed;
            }
            if(defenderhealthchange >= defenderScript.HP)
            {
                score += defenderScript.foodCost * attacksAndDestroysAnEnemy;
                destroys = true;
            }
            score += (unit.foodCost-defenderScript.foodCost) * attacksAnEnemy;
            score +=  + possibleMovesForAllUnits[unit][attackingPosition].Item1;
            temporalList.Add((score,target,destroys));
        }
        ScoreTargetDestoys[attackingPosition] = temporalList;
        return(ScoreTargetDestoys);
    }
    public (decimal, Vector3Int, string) FindOptimalAttack(unitScript checkedUnit, Vector3Int startingposition)
    {
        decimal score = 0;
        string action = "wait";
        Vector3Int target = startingposition;

        //and here we check what scores there are for attacking units, and add the highest
        if(PossibleAttacksandScores.ContainsKey(checkedUnit))
        {
            if(PossibleAttacksandScores[checkedUnit].ContainsKey(startingposition))
            {
                foreach((decimal, Vector3Int, bool) scoreandTarget in PossibleAttacksandScores[checkedUnit][startingposition])
                {
                    if(scoreandTarget.Item3)
                    {
                        //here, if the attackingunit can destroy the defending unit, we give it extra points for its score.
                        if(scoreandTarget.Item1 + attacksAndDestroysAnEnemy * getunit(scoreandTarget.Item2).foodCost > score)
                        {
                            score = scoreandTarget.Item1 + attacksAndDestroysAnEnemy * getunit(scoreandTarget.Item2).foodCost;
                            action = "destroy";
                            target = scoreandTarget.Item2;
                        }
                    }
                    else
                    {
                        if(scoreandTarget.Item1 > score)
                        {
                            score = scoreandTarget.Item1;
                            action = "attack";
                            target = scoreandTarget.Item2;
                        }
                    }
                }
            }
        }
        return(score,target,action);
    }
    public List<(Vector3Int, int)> attackableUnits(Vector3Int attackposition, unitScript unit)
    {
        List<(Vector3Int, int)> TargetandScore = new List<(Vector3Int, int)>();
        return(TargetandScore);
    }
    public void CenterCameraonUnit(unitScript unit, Vector3Int newPosition, string  Action)
    {
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
        StartCoroutine(waitSeconds(waitHowLong * .8f));
    }
    public void SelectUnit(unitScript unit)
    {
        Vector3Int currentPosition = gridPosition(unit);
        selectionmanager.selectUnit(unit, currentPosition);
        StartCoroutine(waitSeconds(waitHowLong));
    }
    public void moveUnit(unitScript unit, Vector3Int newPosition)
    {
        flagforFlow = true;
        movementStep++;
        Vector3Int currentPosition = gridPosition(unit);
        selectionmanager.getPath(currentPosition, newPosition);
    }

    public void takeAction(unitScript unit, Vector3Int newPosition, string Action, Vector3Int target)
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
            #region attack/destroy
            case "attack":
            case "destroy":
            Debug.Log("attacking " + target);
                selectionmanager.InvokeCombat(newPosition,target);
                break;
            #endregion
            default:
                break;
        }
        movingaUnit = true;
    }
    //we check if there is a capturable location within range and increase the score by 8, which is the difference between the minimum and maximum defenses (-3 and +4)
    private bool TrueIfwithinCaptureDistance(unitScript checkedUnit, Vector3Int startingposition)
    {
        Dictionary<Vector3Int, (decimal,string, Vector3Int)> moveswithinthissquare = findPossibleMoves(startingposition, checkedUnit);
        List<Vector3Int> Positions = new List<Vector3Int>(moveswithinthissquare.Keys);
        foreach(Vector3Int currentTile in Positions)
        {
            if(map.GetTile<levelTile>(currentTile) != null)
            {
                if (map.GetTile<levelTile>(currentTile).controllable)
                {
                    if (map.GetInstantiatedObject(currentTile).GetComponent<controllable_script>().owner == 0)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
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
    public unitScript getunit(Vector3Int position)
        {
            return getunit(_mainCamera.WorldToScreenPoint(map.GetCellCenterWorld(position)));
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
        movementStep++;
        flagforFlow = true;
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
    public int calculateDamage(unitScript attackingunit, unitScript defendingunit, Vector3Int attackPosition, Vector3Int defendposition)
    {
        int distanceFromAttackingUnit = Math.Abs(attackPosition.x - defendposition.x) + Math.Abs(attackPosition.y - defendposition.y);
        if (attackingunit.status == "stunned" || attackingunit.status == "recovered" || attackingunit.HP <= 0)
        return 0;
        if(distanceFromAttackingUnit > attackingunit.attackrange)
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
            if (adv == defendingunit.typeOfUnit.ToString()|| adv == defendingunit.movementtype.ToString() || adv == defendingunit._attacktype || adv == (defendingunit._attacktype + defendingunit.movementtype) || adv == (defendingunit._attacktype + defendingunit.typeOfUnit.ToString()) || adv == (defendingunit.movementtype.ToString() + defendingunit.typeOfUnit.ToString()))
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

}
