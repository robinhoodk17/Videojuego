using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public enum BattleState { START, PLAYERTURN, ENDTURN, WON, LOST }
//remember to add in the inspector the tiles in the tiledatas and the tilemap in "map" (for loading the map) and tilebases (for being able to edit)

public class MapManager : MonoBehaviour
{
    /*If a prefab wants to access the name and status of a tile, create and position a prefab on the tile map, then in its script
     you can use these lines:

        mapManager = FindObjectOfType<Mapmanager>();
        nameoftile = mapManager.Getname(transform.position);
        statusoftile = mapManager.Getstatus(transform.position);

     */

    /* To set a tile to the element y of the tileDatas dictionary
     * 
    map.SetTile(gridPosition, levelTile[y]);

     */
    [SerializeField]
    private Tilemap map, conditions, units;
    [SerializeField]
    public List<GameObject> Buildables;
    public bool clicked;
    public int numberOfPlayers;
    public BattleState state;
    public int activeplayer = 1;
    public GameObject playerstartpanel;
    public TextMeshProUGUI activeplayertext;
    public GameObject[] resourcePanels;
    int[] food;
    int[] SUP;
    int turnnumber = 0;
    bool barracksSelected = false;
    Vector3Int currentposition;
    bool unitselected = false;
    private SelectionManager eventraiser;

    public int CurrentButtonPressed;
    void Start()
    {
        food = new int[numberOfPlayers];
        SUP = new int[numberOfPlayers];
        state = BattleState.PLAYERTURN;
        eventraiser = GameObject.FindGameObjectWithTag("SelectionManager").GetComponent<SelectionManager>();
        eventraiser.OnUnitSelected += OnUnitSelected;
        eventraiser.OnUnitDeselected += OnUnitDeselected;
    }
    void Update()
    {
        if(turnnumber == 0)
        {
            GameObject[] owners = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject assign in owners)
            {
                GameObject controllable = map.GetInstantiatedObject(gridPosition(assign.transform.position));
                controllable.GetComponent<controllable_script>().ownerchange(assign.GetComponent<ownerAssginScript>().owner);
                Destroy(assign);
            }
            int[] foodSUP = CalculateIncome();
            food[activeplayer - 1] = foodSUP[0];
            SUP[activeplayer - 1] = foodSUP[1];
            turnnumber = 1;
            resourceshow(foodSUP);
        }

        if ((Input.GetMouseButtonDown(0) && barracksSelected && !EventSystem.current.IsPointerOverGameObject()) || Input.GetMouseButtonDown(1))
        {
            if(barracksSelected)
            {
                map.GetInstantiatedObject(currentposition).transform.GetChild(0).gameObject.SetActive(false);
            }
            barracksSelected = false;
        }
        if (Input.GetMouseButtonDown(0) && !barracksSelected && !unitselected)
        {
            currentposition = gridPosition(Input.mousePosition, true);
            if(getunit(currentposition) ==null)
            {
                if (map.HasTile(currentposition))
                {
                    levelTile Tile = map.GetTile<levelTile>(currentposition);
                    if (Tile.controllable)
                    {
                        if (Tile.type == tileType.barracks)
                        {
                            GameObject barracks = map.GetInstantiatedObject(currentposition);
                            barracks.transform.GetChild(0).gameObject.SetActive(true);
                            barracksSelected = true;
                        }
                    }
                }

            }
        }
        if(clicked)
        {
            int[] costs = new int[2];
            unitScript spawnedUnit = Buildables[CurrentButtonPressed].GetComponent<unitScript>();
            costs[0] = spawnedUnit.foodCost;
            costs[1] = spawnedUnit.SUPCost;
            if (costs[0] <= food[activeplayer-1] && costs[1] <= SUP[activeplayer - 1])
            {
                GameObject.Instantiate(Buildables[CurrentButtonPressed], map.GetCellCenterWorld(currentposition), Quaternion.identity);
                spawnedUnit = getunit(currentposition);
                spawnedUnit.exhausted = true;
                spawnedUnit.ownerChange(activeplayer);
                GameObject unitprefab = spawnedUnit.gameObject;
                unitprefab.GetComponent<SpriteRenderer>().color = new Color(.6f, .6f, .6f);
                food[activeplayer - 1] -= costs[0];
                SUP[activeplayer - 1] -= costs[1];
                clicked = false;
                int[] foodSUP = new int[2];
                foodSUP[0] = food[activeplayer - 1];
                foodSUP[1] = SUP[activeplayer - 1];
                resourceshow(foodSUP);
                map.GetInstantiatedObject(currentposition).transform.GetChild(0).gameObject.SetActive(false);
                clicked = false;
            }
            else
            {
                clicked = false;
            }
        }
    }

    public Vector3Int gridPosition (Vector2 mouseposition)
    {
        Vector3Int gridposition = map.WorldToCell(mouseposition);
        return gridposition;
    }
    public IEnumerator panel(float waitingtime)
    {
        playerstartpanel.SetActive(true);
        yield return new WaitForSeconds(waitingtime);
        playerstartpanel.SetActive(false);
    }
    public void OnTurnEnd()
    {
        state = BattleState.ENDTURN;
        GameObject[] allunits = GameObject.FindGameObjectsWithTag("Unit");

        foreach (GameObject unit in allunits)
        {
            unitScript instanceofunit = unit.GetComponent<unitScript>();
            if (instanceofunit.owner == activeplayer)
            {
                instanceofunit.turnEnd();
            }
        }
        resourcePanels[activeplayer - 1].SetActive(false);
        if (activeplayer < numberOfPlayers)
        { activeplayer++; }
        else
        { activeplayer = 1; turnnumber++; }
        state = BattleState.START;
        //making the turn start message pop up
        //panel turns off the panel after f seconds
        activeplayertext.text = "Player " + activeplayer.ToString();
        StartCoroutine(panel(.7f));

        int[] foodSUP = CalculateIncome();
        food[activeplayer - 1] += foodSUP[0];
        SUP[activeplayer - 1] += foodSUP[1];
        foodSUP[0] = food[activeplayer - 1];
        foodSUP[1] = SUP[activeplayer - 1];
        resourceshow(foodSUP);
        foreach (GameObject unit in allunits)
        {
            unitScript instanceofunit = unit.GetComponent<unitScript>();
            if(instanceofunit.owner == activeplayer)
            {
                instanceofunit.trackactiveplayer(activeplayer);
                instanceofunit.turnStart();
            }
        }
    }

    public int[] CalculateIncome()
    {
        int[] foodSUP = new int[2];
        foodSUP[0] = 0;
        foodSUP[1] = 0;
        foreach (var posi in map.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(posi.x, posi.y, posi.z);
            if (map.HasTile(localPlace))
            {
                levelTile Tile = map.GetTile<levelTile>(localPlace);
                if(Tile.controllable)
                {
                    int owner = map.GetInstantiatedObject(localPlace).GetComponent<controllable_script>().owner;
                    if(owner == activeplayer)
                    {
                        if(Tile.type == tileType.market)
                        {
                            foodSUP[0] += 100;
                        }
                        if(Tile.type == tileType.bonfire)
                        {
                            foodSUP[1] += 1;
                        }
                    }
                }
            }
        }
        return foodSUP;
    }
    public void resourceshow(int[] resources)
    {
        resourcePanels[activeplayer - 1].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = resources[0].ToString();
        resourcePanels[activeplayer - 1].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = resources[1].ToString();
        resourcePanels[activeplayer - 1].SetActive(true);
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

    private void OnUnitSelected(GameObject unit)
    {
        unitselected = true;
    }
    private void OnUnitDeselected()
    {
        unitselected = false;
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
    public GameObject getunitprefab(Vector3Int position, bool screen = true)
    {
        Vector3 worldposition = worldPosition(position);
        var ray = Camera.main.ScreenPointToRay(worldposition);
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