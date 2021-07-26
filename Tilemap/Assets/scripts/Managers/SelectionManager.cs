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
    int playernumber = 2;
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

    unitScript unit = new unitScript();
    Vector3Int currentposition = new Vector3Int();
    void Update()
    {
        //this is the click to select a unit (can also select enemy units and see their movement)
        if (Input.GetMouseButtonDown(0) && !unitselected)
        {
            Vector3Int position = gridPosition(Input.mousePosition, true);
            unitScript unit = getunit(Input.mousePosition);
            if(unit != null)
            {
                findSelectabletiles(unit, position);
                unitselected = true;
            }
        }
        //the right click resets the selection
        if(Input.GetMouseButtonDown(1))
        {
            Reset();
        }
        //this is the click to move the unit
        if(Input.GetMouseButtonUp(0) && unitselected)
        {
            if(unit.owner == activeplayer)
            {
                Vector3Int position = gridPosition(Input.mousePosition, true);
            }
        }

    }

    private void Reset()
    {
        neighborlist.Clear();
        selectableTiles.Clear();
        path.Clear();
        distancelist.Clear();
        unitselected = false;
        foreach (var pos in units.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (units.HasTile(localPlace))
            {
                units.SetTile(localPlace, null);
            }
        }
    }
    //finds the neighbors of a tile in gridposition "position" using the unit's movement
    void findneighbors(unitScript unit, Vector3Int position)
    {
        List<Vector3Int> adjacencyList = new List<Vector3Int>();
        string movementType = unit.movementtype;
        levelTile Tile = map.GetTile<levelTile>(position);
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
        if(!screen)
        {
            position = Camera.main.WorldToScreenPoint(position);
        }
        var ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var selection = hit.transform;
            var unit = selection.GetComponent<unitScript>();
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
    //get the gridposition given a world position (if screen = true, it calculates given a screen position instead)
    public Vector3Int gridPosition(Vector3 position, bool screen = false)
    {
        if(screen)
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
            selectableTiles.Add(pos);
            foreach (Vector3Int vector in neighborlist[pos])
            {
                if (!visitlist[vector] && (distancelist[pos] + distancelist[vector]) <= unit.movement)
                {
                    parentlist[vector] = pos;
                    visitlist[vector] = true;
                    distancelist[vector] = distancelist[pos] + distancelist[vector];
                    process.Enqueue(vector);
                }
            }
        }

        //this foreach sets the UI elements to show where the unit can move
        foreach(Vector3Int selectable in selectableTiles)
        {
            if(unit.attackandmove)
            {
                units.SetTile(selectable, movementUI[2]);
            }
            else
            {
                units.SetTile(selectable, movementUI[0]);
            }
        }
    }

    public void OnTurnEnd()
    {
        Reset();
        if(activeplayer < playernumber)
        {
            activeplayer++;
        }
        else
        {
            activeplayer = 0;
        }
    }
}

