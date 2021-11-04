using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using Unity;
using TMPro;


public class SelectionManager : MonoBehaviour
{
    //the tilemaps (units has the UI elements for the units)
    [SerializeField]
    private Tilemap map, conditions, units;
    public float timeBetweenClicks = 0.5f;
    float lastClick;
    public float HoverTime = .5f;
    private float startHover;
    private bool startedHovering = false;

    public int thisistheplayer;
    int activeplayer = 1;
    public int playernumber = 2;
    private bool load = false;
    //this list holds the tile types with the UI elements of the units for easy accesibility
    /*
    movement: 0
    attack: 1
    attackandmove: 2
    */
    public List<levelTile> movementUI;
    public GameObject damagePreview;
    private bool unitselected = false;
    private unitScript unit;
    private GameObject unitprefab;
    private Vector3Int currentposition = new Vector3Int();
    private Vector3Int newposition = new Vector3Int();
    private Vector3Int hoveringTile = new Vector3Int();
    private bool areWeHovering = false;
    //The unit panel to turn off after moving it
    int turnoff = 0;

    //for selecting the tiles in the tilemap
    [SerializeField]
    private LayerMask mask;

    //lists and dictionaries with the data each tile needs for movement
    private Dictionary<Vector3Int, List<Vector3Int>> neighborlist = new Dictionary<Vector3Int, List<Vector3Int>>();
    private Dictionary<Vector3Int, Vector3Int> parentlist = new Dictionary<Vector3Int, Vector3Int>();
    private Dictionary<Vector3Int, int> distancelist = new Dictionary<Vector3Int, int>();
    private Dictionary<Vector3Int, bool> visitlist = new Dictionary<Vector3Int, bool>();
    private bool usingability = false;
    List<Vector3Int> selectableTiles = new List<Vector3Int>();
    Stack<Vector3Int> path = new Stack<Vector3Int>();
    

    public event Action<Vector3Int, Vector3Int> Oncombatstart;
    public event Action<GameObject> OnUnitSelected;
    public event Action OnUnitDeselected;
    public event Action<Vector3Int, Vector3Int, GameObject> Oncombathover;
    private Camera _mainCamera;
    private gridCombat combatManager;
    //for the animations
    string unitstate ="idle";
    //for the tooltip
    infoPanel unitToolTip;
    bool showingtooltip = false;
    private MapManager mapmanager;

    private void Start()
    {
        //Oncombatstart += Oncombat;
        _mainCamera = Camera.main;
        combatManager = new gridCombat();
        combatManager.Start();
        combatManager.map = map;
        combatManager.conditions = conditions;
        mapmanager = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
    }

    private void FixedUpdate()
    {
        if(unitselected)
        {
            if (unitstate == "moving")
            {
                if (path.Count > 0)
                {
                    Move(unitprefab.transform.position, path.Peek());
                }
                else
                {
                    units.ClearAllTiles();
                    unitstate = "thinking";
                    if (newposition == currentposition || unit.attackandmove)
                    {
                        findattackables(newposition, unit);
                    }

                    turnoff = showUnitPanel(unitprefab, unit, newposition);
                }
            }

        }

    }
    void Update()
    {
        //this is the click to move the unit
        if (Input.GetMouseButtonUp(0) && unitselected && !EventSystem.current.IsPointerOverGameObject() && unitstate != "moving" && thisistheplayer == activeplayer)
        {
            if (unit.owner != activeplayer) { Reset(); }
            else
            {
                if (unitstate == "idle")
                {
                    //these ifs set the unit in motion (works even if you press its own position) only if you are clicking on a movement UI tile while the
                    //unit is not exhausted an while the target position has no units.
                    newposition = gridPosition(Input.mousePosition, true);
                    if(getunit(newposition) != null)
                    {
                        if(getunit(newposition).istransport && getunit(newposition).transportedUnits.Count < getunit(newposition).unitCarryingCapacity && unit.typeOfUnit == TypeOfUnit.infantry)
                        {
                            load = true;
                            Debug.Log("we loaded");
                        }
                    }
                    if ((getunit(newposition) != null && newposition != currentposition && unitstate != "thinking" && !load) || (unit.exhausted || !units.HasTile(newposition) || unit.status == "stunned" || unit.status == "recovered"))
                    {
                        Reset();
                    }
                    else
                    {
                        if (!unit.exhausted && units.HasTile(newposition))
                        {
                            if (units.GetTile<levelTile>(newposition) == movementUI[0] || units.GetTile<levelTile>(newposition) == movementUI[2])
                            {
                                getPath(currentposition, newposition, unit);
                                unitstate = "moving";
                                path.Pop();
                            }
                        }
                        else
                        {
                            if (unitstate != "thinking")
                                Reset();
                        }
                    }

                }
            }
        }
        //this is the click to select a unit (can also select enemy units and see their movement)
        if (Input.GetMouseButtonUp(0) && !unitselected && !EventSystem.current.IsPointerOverGameObject() && thisistheplayer == activeplayer)
        {
            currentposition = gridPosition(Input.mousePosition, true);
            unitprefab = getunitprefab(Input.mousePosition, true);
            unit = getunit(currentposition);
            if (unit != null)
            {
                if(unitstate =="idle")
                {
                    findSelectabletiles(unit, currentposition);
                    unitselected = true;
                    unit.onMove();
                    OnUnitSelected?.Invoke(unitprefab);
                }
            }
        }
        //the right click resets the selection
        if (Input.GetMouseButtonDown(1) && thisistheplayer == activeplayer)
        {
            Reset();
            if (getunit(Input.mousePosition, true) != null && !showingtooltip && !unitselected)
            {
                showingtooltip = true;
                unitToolTip = getunitprefab(Input.mousePosition, true).GetComponent<infoPanel>();
                unitToolTip.showPanel();
            }
        }
        //after the unit is done moving
        if (unitselected && thisistheplayer == activeplayer)
        {
            //Here we check if there is an attackable unit,and if it is clicked,
            // we initiate combat (the selected unit is on "newposition" and the attacked unit is on "clickedtile")
            //We also check for abilities, such as heals
            if (unitstate == "thinking" && Time.time - lastClick > timeBetweenClicks)
            {
                Vector2 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int clickedtile = gridPosition(Input.mousePosition, true);
                if(!startedHovering)
                {
                    startedHovering = true;
                    hoveringTile = clickedtile;
                    startHover = Time.time;
                }
                if(startedHovering && hoveringTile == clickedtile)
                {
                    if (units.HasTile(clickedtile) && getunit(clickedtile) != null && !usingability)
                    {
                        if (getunit(clickedtile).owner != activeplayer && Time.time - startHover >= HoverTime && !areWeHovering)
                        {
                            GameObject damagePreviewInstance = Instantiate(damagePreview, worldPosition(clickedtile), Quaternion.identity);
                            Oncombathover?.Invoke(newposition, clickedtile, damagePreviewInstance);
                            areWeHovering = true;
                        }

                    }
                }
                if (areWeHovering && hoveringTile != clickedtile)
                {
                    Destroy(GameObject.FindGameObjectWithTag("damagePreview"));
                    areWeHovering = false;
                    startedHovering = false;
                }
                if (startedHovering && hoveringTile != clickedtile)
                {
                    startedHovering = false;
                }
                //this if invokes combat against another unit
                if (Input.GetMouseButtonUp(0) && units.HasTile(clickedtile) && getunit(clickedtile) != null && !usingability)
                {
                    if (getunit(clickedtile).owner != activeplayer)
                    {
                        if(getunit(clickedtile).HP > 0 || unit.ability == "capture")
                        {
                            onWait();
                            Oncombatstart?.Invoke(newposition, clickedtile);
                        }
                    }
                }
                //this if uses the ability of the unit
                if(Input.GetMouseButtonUp(0) && units.HasTile(clickedtile) && getunit(clickedtile) != null && usingability)
                {
                    #region heal
                    if (unit.ability == "heal")
                    {
                        //the healed unit
                        unitScript resUnit = getunit(clickedtile);
                        //this next region manages the cost for healing the unit
                        mapmanager.food[resUnit.owner - 1] -= resUnit.foodCost;
                        int food = mapmanager.food[resUnit.owner -1];
                        int SUP = mapmanager.SUP[resUnit.owner - 1];
                        int[] temp = new int[2];
                        temp[0] = food;
                        temp[1] = SUP;
                        mapmanager.resourceshow(temp);
                        //here we heal the unit and un-exhaust it
                        resUnit.HP = resUnit.maxHP;
                        resUnit.exhausted = false;
                        resUnit.status = "clear";
                        resUnit.healthChanged();
                        resUnit.sprite.color = new Color(1, 1, 1);
                        //the unit that healed the other unit waits
                        onWait();
                    }
                    #endregion
                    #region teach
                    if (unit.ability == "teach")
                    {
                        //the unit that gains a level
                        unitScript resUnit = getunit(clickedtile);
                        //this next region manages the cost for healing the unit
                        mapmanager.food[resUnit.owner - 1] -= resUnit.foodCost/10;
                        int food = mapmanager.food[resUnit.owner - 1];
                        int SUP = mapmanager.SUP[resUnit.owner - 1];
                        int[] temp = new int[2];
                        temp[0] = food;
                        temp[1] = SUP;
                        mapmanager.resourceshow(temp);
                        //here we the unit gains a level
                        resUnit.xp+= (resUnit.xptoincreaselv-1);
                        resUnit.gainXP();
                        //the unit that taught the other unit waits
                        onWait();

                    }
                    #endregion teach
                }
                //this if attacks a controllable tile
                if (units.HasTile(clickedtile) && getunit(clickedtile) == null && !usingability)
                {
                    if(map.GetTile<levelTile>(clickedtile).controllable)
                    {
                        GameObject attackedproperty = map.GetInstantiatedObject(clickedtile);
                        if(attackedproperty.GetComponent<controllable_script>().owner != activeplayer)
                        {
                            Oncombatstart?.Invoke(newposition, clickedtile);
                            onWait();
                        }
                    }
                }
            }
        }
    }

    
    public void onWait()
    {
        mapmanager.selectedUnitWaits(currentposition, newposition);
        currentposition = newposition;
        List<unitScript> auras = new List<unitScript>();
        if(unit.hasaura)
        {
            List<Vector3Int> affectedUnits = findifwithinrange(unit, newposition);
            foreach(Vector3Int affectedUnit in affectedUnits)
            {
                switch(unit.ability)
                {
                    case "deathdealer":
                        getunit(affectedUnit).Destroyed();
                        break;
                }
            }
        }
        foreach(GameObject unitloop in GameObject.FindGameObjectsWithTag("Unit"))
        {
            if(unitloop.GetComponent<unitScript>().haswaitingaura)
            {
                auras.Add(unitloop.GetComponent<unitScript>());
            }
        }
        foreach(unitScript unitwithaura in auras)
        {
            if (findifwithinrange(newposition, unitwithaura, gridPosition(unitwithaura)))
            {
                switch(unitwithaura.ability)
                {
                    case "pistolero":
                        Oncombatstart?.Invoke(gridPosition(unitwithaura), newposition);
                        break;
                }
            }
        }
        unit.exhausted = true;
        unit.sprite.color = new Color(.6f, .6f, .6f);
        Reset();
    }
    public void onLoad()
    {

    }
    public void onCap()
    {
        map.GetInstantiatedObject(newposition).GetComponent<controllable_script>().ownerchange(activeplayer, (double)((double) unit.HP/ (double)unit.maxHP));
        unit.onCap();
        onWait();
    }

    public void onAttackButtonClicked()
    {
        turnpanel(unitprefab, false, turnoff);

        lastClick = Time.time;
    }
    public void onAbilityclicked()
    {
        turnpanel(unitprefab, false, turnoff);
        usingability = true;
        lastClick = Time.time;
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
    private void Reset()
    {
        load = false;
        neighborlist.Clear();
        selectableTiles.Clear();
        path.Clear();
        distancelist.Clear();
        if (unitselected)
        {
            unitstate = "idle";
            unit.animator.SetTrigger("idle");
            unitprefab.transform.position = (map.GetCellCenterWorld(currentposition));
                //SetPositionAndRotation(map.GetCellCenterWorld(currentposition), Quaternion.identity);
            turnpanel(unitprefab, false, turnoff);
            neighborlist.Clear();
            selectableTiles.Clear();
            path.Clear();
            distancelist.Clear();
            unitselected = false;
            units.ClearAllTiles();
            //there was a weird bug happening. I hope this fixes it.
            unit.healthChanged();
            currentposition = newposition;
        }
        startedHovering = false;
        if(areWeHovering)
        {
            Destroy(GameObject.FindGameObjectWithTag("damagePreview"));
            areWeHovering = false;
        }
        unitselected = false;
        usingability = false;
        OnUnitDeselected?.Invoke();
        if(showingtooltip)
        {
            unitToolTip.hidePanel();
            showingtooltip = false;
        }
    }

    //finds the neighbors of a tile in gridposition "position" using the unit's movement
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
                    if(checkedTile.controllable)
                    {
                        tileowner = map.GetInstantiatedObject(position + left).GetComponent<controllable_script>().owner;
                    }
                    if ((getunit(position + left) == null || getunit(position + left).owner == unit.owner || getunit(position + left).status == "downed") && (tileowner == 0|| tileowner == unit.owner))
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
    //tries to get the unit at screenposition "position" if screen = true
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
    //tries to get a unit given a gridposition
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
    //get the gridposition given a world position (if screen = true, it calculates given a screen position instead)
    public Vector3Int gridPosition(Vector3 position, bool screen = false)
    {
        if (screen)
        {
            position = _mainCamera.ScreenToWorldPoint(position);
        }

        Vector3Int gridposition = map.WorldToCell(position);
        return gridposition;
    }

    public Vector3Int gridPosition(unitScript unit)
    {
        return map.WorldToCell(unit.gameObject.transform.position);
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
                distancelist[localPlace] = Tile.movecost(unit.movementtype.ToString());
            }
        }

        //here we initialize the queue where we will get all the selectable tiles
        Queue<Vector3Int> process = new Queue<Vector3Int>();
        visitlist[position] = true;
        distancelist[position] = 0;
        process.Enqueue(position);
        selectableTiles.Add(position);
        if (unit.attackandmove)
        {
            while (process.Count > 0)
            {
                Vector3Int pos = process.Dequeue();
                if (getunit(pos) == null)
                { selectableTiles.Add(pos); }
                else
                {
                    if((getunit(pos).istransport || (unit.ability == "attach" && getunit(pos).typeOfUnit == TypeOfUnit.vehicle)) && getunit(pos).transportedUnits.Count < getunit(pos).unitCarryingCapacity && unit.typeOfUnit == TypeOfUnit.infantry)
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
            foreach (Vector3Int selectable in selectableTiles)
            {
                findinnerattackables(selectable, unit);
            }
            foreach (Vector3Int selectable in selectableTiles)
            {
                if(units.HasTile(selectable))
                {
                    units.SetTile(selectable, movementUI[2]);
                }
            }
        }

        else
        {

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
                        int tempdistance = map.GetTile<levelTile>(vector).movecost(unit.movementtype.ToString());
                        if (tempdistance + distancelist[pos] < distancelist[vector])
                        {
                            parentlist[vector] = pos;
                            distancelist[vector] = tempdistance + distancelist[pos];
                        }
                    }
                }
            }
            findinnerattackables(position, unit);
            foreach(Vector3Int selectable in selectableTiles)
            {
                if(units.HasTile(selectable))
                {
                    units.SetTile(selectable, movementUI[2]);
                }
                else
                {
                    units.SetTile(selectable, movementUI[0]);
                }
            }
        }

        void findinnerattackables(Vector3Int position, unitScript unit)
        {
            Dictionary<Vector3Int, List<Vector3Int>> newneighborlist = new Dictionary<Vector3Int, List<Vector3Int>>();
            Dictionary<Vector3Int, Vector3Int> newparentlist = new Dictionary<Vector3Int, Vector3Int>();
            Dictionary<Vector3Int, int> newdistancelist = new Dictionary<Vector3Int, int>();
            Dictionary<Vector3Int, bool> newvisitlist = new Dictionary<Vector3Int, bool>();
            List<Vector3Int> newselectableTiles = new List<Vector3Int>();
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
                    newneighborlist[localPlace] = adjacencyList;
                    newvisitlist[localPlace] = false;
                    newparentlist[localPlace] = Vector3Int.zero;
                    newdistancelist[localPlace] = 1;
                }
            }

            Queue<Vector3Int> process = new Queue<Vector3Int>();
            newvisitlist[position] = true;
            newdistancelist[position] = 0;
            process.Enqueue(position);
            while (process.Count > 0)
            {
                Vector3Int pos = process.Dequeue();
                newselectableTiles.Add(pos);
                foreach (Vector3Int vector in newneighborlist[pos])
                {
                    if (!newvisitlist[vector] && (newdistancelist[pos] + newdistancelist[vector]) <= unit.attackrange)
                    {
                        newparentlist[vector] = pos;
                        newvisitlist[vector] = true;
                        newdistancelist[vector] = newdistancelist[pos] + newdistancelist[vector];
                        process.Enqueue(vector);
                    }
                }
            }

            foreach (Vector3Int selectable in newselectableTiles)
            {
                units.SetTile(selectable, movementUI[1]);
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
        if (Vector3.Distance(startposition, target) <= .11f)
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

    //this method returns true if the targetposition is within the unit's aura range (position is the unit with aura's position)
    public bool findifwithinrange(Vector3Int targetposition, unitScript unit, Vector3Int position)
    {

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
            if(selectable == targetposition)
            {
                if (unit.auracheck(getunit(targetposition)))
                    return true;
            }
        }
        return false;
    }
    public List<Vector3Int> findifwithinrange(unitScript unit, Vector3Int position)
    {
        List<Vector3Int> affectedUnits = new List<Vector3Int> ();
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
            if(getunit(selectable) != null)
            {
                if (unit.auracheck(getunit(selectable)))
                {
                    affectedUnits.Add(selectable);
                }
            }
        }
        return affectedUnits;
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
       * 8: load
       */
        bool attackablewithinrange = false;
        bool unithasability = false;
        bool unitcancapture = false;
        int childtoactivate = 0;
        //to select if the unit is on top of a neutral property and can capture it
        if(map.GetTile<levelTile>(gridposition).controllable)
        {
            if (unitscript.typeOfUnit == TypeOfUnit.infantry && map.GetInstantiatedObject(gridposition).GetComponent<controllable_script>().owner == 0)
            {
                unitcancapture = true;
            }
        }
        //to select if the unit has an ability
        unithasability = unit.abilityCheck(newposition);
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
        if(unitcancapture || unithasability || attackablewithinrange || load)
        {
            if(!unithasability && !unitcancapture) { childtoactivate = 3; }
            if(!unithasability && !attackablewithinrange) { childtoactivate = 4; }
            if(!unitcancapture && !attackablewithinrange) { childtoactivate = 5; }
            if (!unithasability && attackablewithinrange && unitcancapture) { childtoactivate = 6; }
            if (unithasability && attackablewithinrange && !unitcancapture) { childtoactivate = 7; }
            if (unithasability && !attackablewithinrange && unitcancapture) { childtoactivate = 8; }
            if (unithasability && attackablewithinrange && unitcancapture) { childtoactivate = 9; }
            if (load) { childtoactivate = 11; }
        }

        else
        {
            childtoactivate = 2;
        }

        turnpanel(unitobject, true, childtoactivate);
        return childtoactivate;
    }

    public void turnpanel(GameObject unitobject, bool onoroff, int child)
    {
        unitobject.transform.GetChild(0).transform.GetChild(child).gameObject.SetActive(onoroff);
    }

    /*public void Oncombat(Vector3Int attacker, Vector3Int defender)
    {
        currentposition = attacker; 
        unit.exhausted = true;
        unit.sprite.color = new Color(.6f, .6f, .6f);
        Reset();
    }*/

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
            //This if happens if the unit is attacking a tile
            if (getunit(defendposition) == null)
            {
                attackerScript = getunit(attackposition);
                int damage = attackerScript.attackdamage;
                damage = (int)(damage * attackerScript.HP / attackerScript.maxHP * (1 + attackerScript.level / 10) * (1 + GlobalModifiers(attackerScript.owner)[0]));
                controllable_script attackedTile = map.GetInstantiatedObject(defendposition).GetComponent<controllable_script>();
                if(attackerScript.ability == "siege")
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
                attackerScript = getunit(attackposition);
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
                            if(defenderScript.ability == "capture")
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

