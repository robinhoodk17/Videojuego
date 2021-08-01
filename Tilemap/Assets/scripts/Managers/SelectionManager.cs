using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectionManager : MonoBehaviour
{
    //the tilemaps (units has the UI elements for the units)
    [SerializeField]
    private Tilemap map, conditions, units;
    int activeplayer = 1;
    public int playernumber = 2;
    //this list holds the tile types with the UI elements of the units for easy accesibility
    /*
    movement: 0
    attack: 1
    attackandmove: 2
    */
    public List<levelTile> movementUI;

    bool unitselected = false;

    //for selecting the tiles in the tilemap
    [SerializeField]
    private LayerMask mask;

    //lists and dictionaries with the data each tile needs for movement
    private Dictionary<Vector3Int, List<Vector3Int>> neighborlist = new Dictionary<Vector3Int, List<Vector3Int>>();
    private Dictionary<Vector3Int, Vector3Int> parentlist = new Dictionary<Vector3Int, Vector3Int>();
    private Dictionary<Vector3Int, int> distancelist = new Dictionary<Vector3Int, int>();
    private Dictionary<Vector3Int, bool> visitlist = new Dictionary<Vector3Int, bool>();
    List<Vector3Int> selectableTiles = new List<Vector3Int>();
    Stack<Vector3Int> path = new Stack<Vector3Int>();

    unitScript unit;
    GameObject unitprefab;
    Vector3Int currentposition = new Vector3Int();
    Vector3Int newposition = new Vector3Int();
    int turnoff = 0;

    void Update()
    {

        if (unitselected)
        {
            if (unit.state == "moving")
            {
                if (path.Count > 0)
                {
                    Move(unitprefab.transform.position, path.Peek());
                }
                else
                {
                    clearUnitsTiles();
                    unit.state = "thinking";
                    if (newposition == currentposition || unit.attackandmove)
                    {
                        findattackables(newposition,unit);
                    }

                    turnoff = showUnitPanel(unitprefab, unit, newposition);
                }
            }
        }
        //this is the click to move the unit
        if (Input.GetMouseButtonUp(0) && unitselected)
        {
            if (unit.owner != activeplayer) { Reset(); }
            else
            {
                //these ifs set the unit in motion (works even if you press its own position) only if you are clicking on a movement UI tile while the
                //unit is not exhausted an while the target position has no units.
                newposition = gridPosition(Input.mousePosition, true);
                if (((getunit(newposition) != null && newposition != currentposition) && unit.state != "thinking") || (unit.exhausted || !units.HasTile(newposition)))
                {
                    Reset();
                }
                else
                {
                    if (!unit.exhausted && units.HasTile(newposition) && unit.state == "idle")
                    {
                        getPath(currentposition, newposition, unit);
                        unit.state = "moving";
                        path.Pop();
                    }
                    else
                    {
                        if(unit.state != "thinking")
                            Reset(); 
                    }
                }
            }
        }
        //this is the click to select a unit (can also select enemy units and see their movement)
        if (Input.GetMouseButtonUp(0) && !unitselected)
        {
            currentposition = gridPosition(Input.mousePosition, true);
            unitprefab = getunitprefab(Input.mousePosition, true);
            unit = getunit(currentposition);
            if (unit != null)
            {
                findSelectabletiles(unit, currentposition);
                unitselected = true;
            }
        }
        //the right click resets the selection
        if (Input.GetMouseButtonDown(1))
        {
            Reset();
        }

    }

    
    public void onWait ()
    {
        currentposition = newposition;
        unit.exhausted = true;
        Reset();
    }
    public void OnTurnEnd()
    {
        Reset();
        if (activeplayer < playernumber)
        {
            activeplayer++;
        }
        else
        {
            activeplayer = 1;
        }
    }
    private void clearUnitsTiles()
    {
        foreach (var pos in units.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (units.HasTile(localPlace))
            {
                units.SetTile(localPlace, null);
            }
        }
    }
    private void Reset()
    {
        neighborlist.Clear();
        selectableTiles.Clear();
        path.Clear();
        distancelist.Clear();
        clearUnitsTiles();
        if (unitselected)
        {
            unit.state = "idle";
            unitprefab.transform.SetPositionAndRotation(map.GetCellCenterWorld(currentposition), Quaternion.identity);
            turnpanel(unitprefab, false, turnoff);
            neighborlist.Clear();
            selectableTiles.Clear();
            path.Clear();
            distancelist.Clear();
            unitselected = false;
            clearUnitsTiles();
        }
        unitselected = false;
    }

    //finds the neighbors of a tile in gridposition "position" using the unit's movement
    void findneighbors(unitScript unit, Vector3Int position)
    {
        List<Vector3Int> adjacencyList = new List<Vector3Int>();
        string movementType = unit.movementtype;
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
                    if (getunit(position + left) == null || getunit(position + left).owner == unit.owner)
                    {
                        adjacencyList.Add(position + left);
                    }
                }
                if (map.HasTile(position + up))
                {
                    if (getunit(position + up) == null || getunit(position + up).owner == unit.owner)
                    {
                        adjacencyList.Add(position + up);
                    }
                }
                if (map.HasTile(position + down))
                {
                    if (getunit(position + down) == null || getunit(position + down).owner == unit.owner)
                    {
                        adjacencyList.Add(position + down);
                    }
                }
                if (map.HasTile(position + right))
                {
                    if (getunit(position + right) == null || getunit(position + right).owner == unit.owner)
                    {
                        adjacencyList.Add(position + right);
                    }
                }
                neighborlist[position] = adjacencyList;
                break;
            #endregion
            #region case foot
            case "foot":
                if (map.HasTile(position + left) && map.GetTile<levelTile>(position + left).type.ToString() != "lava")
                {
                    if (getunit(position + left) == null || getunit(position + left).owner == unit.owner)
                        adjacencyList.Add(position + left);
                }
                if (map.HasTile(position + up) && map.GetTile<levelTile>(position + up).type.ToString() != "lava")
                {
                    if (getunit(position + up) == null || getunit(position + up).owner == unit.owner)
                        adjacencyList.Add(position + up);
                }
                if (map.HasTile(position + down) && map.GetTile<levelTile>(position + down).type.ToString() != "lava")
                {
                    if (getunit(position + down) == null || getunit(position + down).owner == unit.owner)
                        adjacencyList.Add(position + down);
                }
                if (map.HasTile(position + right) && map.GetTile<levelTile>(position + right).type.ToString() != "lava")
                {
                    if (getunit(position + right) == null || getunit(position + right).owner == unit.owner)
                        adjacencyList.Add(position + right);
                }

                neighborlist[position] = adjacencyList;
                break;
            #endregion
            #region case treads
            case "treads":
                break;
            #endregion
            #region case wheels
            case "wheels":
                break;
                #endregion
        }
    }
    //tries to get the unit at screenposition "position" if screen = true
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
    //get the gridposition given a world position (if screen = true, it calculates given a screen position instead)
    public Vector3Int gridPosition(Vector3 position, bool screen = false)
    {
        if (screen)
        {
            position = Camera.main.ScreenToWorldPoint(position);
        }

        Vector3Int gridposition = map.WorldToCell(position);
        return gridposition;
    }
    public Vector3 worldPosition(Vector3Int gridposition)
    {
        return map.CellToWorld(gridposition);
    }
    void findSelectabletiles(unitScript unit, Vector3Int position)
    {
        Reset();
        //in this foreach we calculate the neighbors of all tiles
        foreach (var posi in map.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(posi.x, posi.y, posi.z);
            if (map.HasTile(localPlace))
            {
                levelTile Tile = map.GetTile<levelTile>(localPlace);
                findneighbors(unit, localPlace);
                visitlist[localPlace] = false;
                parentlist[localPlace] = Vector3Int.zero;
                distancelist[localPlace] = Tile.movecost(unit.movementtype);
            }
        }

        //here we initialize the queue where we will get all the selectable tiles
        Queue<Vector3Int> process = new Queue<Vector3Int>();
        visitlist[position] = true;
        distancelist[position] = 0;
        process.Enqueue(position);
        selectableTiles.Add(position);
        while (process.Count > 0)
        {
            Vector3Int pos = process.Dequeue();
            if (getunit(pos) == null)
            { selectableTiles.Add(pos); }
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
                    int tempdistance = map.GetTile<levelTile>(vector).movecost(unit.movementtype);
                    if (tempdistance + distancelist[pos] < distancelist[vector])
                    {
                        parentlist[vector] = pos;
                        distancelist[vector] = tempdistance + distancelist[pos];
                    }
                }
            }
        }

        //this foreach sets the UI elements to show where the unit can move
        foreach (Vector3Int selectable in selectableTiles)
        {
            if (unit.attackandmove)
            {
                units.SetTile(selectable, movementUI[2]);
            }
            else
            {
                units.SetTile(selectable, movementUI[0]);
            }
        }
    }
    public void getPath(Vector3Int previous, Vector3Int newposition, unitScript unit)
    {
        path.Clear();
        Vector3Int next = newposition;
        while (next != Vector3Int.zero)
        {
            path.Push(next);
            next = parentlist[next];
        }
    }

    public void Move(Vector3 startposition, Vector3Int targetposition)
    {
        Vector3 target = map.GetCellCenterLocal(targetposition) + new Vector3(0, 0, 5);
        if (Vector3.Distance(startposition, target) <= .1f)
        {
            path.Pop();
            unitprefab.transform.position = map.GetCellCenterWorld(targetposition) + new Vector3(0, 0, 5);
        }
        else
        {
            Vector3 heading = target - startposition;
            heading.Normalize();
            Vector3 velocity = heading * unit.movespeedanimation;
            unitprefab.transform.forward = heading;
            unitprefab.transform.SetPositionAndRotation(unitprefab.transform.position + velocity * Time.fixedDeltaTime, Quaternion.identity);
        }
    }

    public void findattackables(Vector3Int position, unitScript unit)
    {

        //creating the neighborlist for attackable tiles
        neighborlist.Clear();
        selectableTiles.Clear();
        path.Clear();
        distancelist.Clear();
        clearUnitsTiles();


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
                if (!visitlist[vector] && (distancelist[pos] + distancelist[vector]) <= unit.attackrange)
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
            units.SetTile(selectable, movementUI[1]);
        }
    }

    public int showUnitPanel(GameObject unitobject, unitScript unitscript, Vector3Int gridposition)
    {
        /*
        *0: w
       * 1: wa
       * 2: wc
       * 3: wo
       * 4: wac
       * 5: wao
       * 6: wco
       * 7: waco
       */
        bool attackablewithinrange = false;
        bool unithasability = false;
        bool unitcancapture = false;
        int childtoactivate = 0;
        //to select if the unit is on top of a neutral property and can capture it
        if(map.GetTile<levelTile>(gridposition).controllable)
        {
            if (unitscript.typeOfUnit == "infantry" && map.GetInstantiatedObject(gridposition).GetComponent<controllable_script>().owner == 0)
            {
                unitcancapture = true;
            }
        }
        //to select if the unit has an ability
        if(unitscript.ability != "none")
        {
            unithasability = true;
        }
        //to select if the unit can attack anything
        if(unitscript.attackandmove || newposition == currentposition)
        {
            foreach (var pos in units.cellBounds.allPositionsWithin)
            {
                Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
                if (units.HasTile(localPlace))
                {
                    if (getunit(localPlace) != null)
                    {
                        if (getunit(localPlace).owner != unitscript.owner)
                        {
                            attackablewithinrange = true;

                        }
                    }
                    if(map.GetTile<levelTile>(localPlace).controllable)
                    {
                        int controlledby = map.GetInstantiatedObject(localPlace).GetComponent<controllable_script>().owner;
                        if ( controlledby != unitscript.owner && controlledby != 0)
                        {
                            attackablewithinrange = true;
                        }
                    }
                }
            }
        }

        //here we select which panel we want to show
        if(unitcancapture || unithasability || attackablewithinrange)
        {
            if(!unithasability && !unitcancapture) { childtoactivate = 1; }
            if(!unithasability && !attackablewithinrange) { childtoactivate = 2; }
            if(!unitcancapture && !attackablewithinrange) { childtoactivate = 3; }
            if (!unithasability && attackablewithinrange && unitcancapture) { childtoactivate = 4; }
            if (unithasability && attackablewithinrange && !unitcancapture) { childtoactivate = 5; }
            if (unithasability && !attackablewithinrange && unitcancapture) { childtoactivate = 6; }
            if (unithasability && attackablewithinrange && unitcancapture) { childtoactivate = 7; }
        }

        else
        {
            childtoactivate = 0;
        }

        turnpanel(unitobject, true, childtoactivate);
        return childtoactivate;
    }

    public void turnpanel(GameObject unitobject, bool onoroff, int child)
    {
        unitobject.transform.GetChild(1).transform.GetChild(child).gameObject.SetActive(onoroff);
    }
}

