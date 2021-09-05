using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class MapEditorManager : MonoBehaviour
{
    public Item_controller[] ItemButtons;

    /*Here are stored the possible tiles for adding to the tilemap. You must add them in the Inspector.
    forest: 0
    mountain: 1
    planes: 2
    river: 3
    road: 4
    farm: 5
    bonfire: 6
     */
    public List<levelTile> tileBases;
    public List<GameObject> Buildables;
    //we use a dictionary to access each unit by name instead of by number
    public Dictionary<string, GameObject> selectedbuildables = new Dictionary<string, GameObject>();

    //this is the current tilemap we are using (the same as in MapManager)
    [SerializeField]
    private Tilemap map, conditions, units;

    //this is the button that was most recently pressed, and so the kind of tile that will be spawned.
    public int CurrentButtonPressed;

    public GameObject[] ItemImage;
    public GameObject unitpanel;
    public GameObject unitpanelcontent;
    public int activeplayer = 1;
    public string unitpressed;
    private int numberoftiles;
    private Camera _mainCamera;
    private float timeBetweenSteps = 0.05f;
    float lastStep;
    private void Start()
    {
        _mainCamera = Camera.main;
        foreach (GameObject card in Buildables)
        {
            GameObject unitprefab = card.GetComponent<UnitCards>().unitprefab;
            selectedbuildables[unitprefab.GetComponent<unitScript>().unitname] = unitprefab;
            GameObject instantiatedCard = Instantiate(card, new Vector3(0, 0, 0), Quaternion.identity);
            instantiatedCard.transform.SetParent(unitpanelcontent.transform, false);
            numberoftiles = tileBases.Count;
        }
    }
    private void Update()
    {
        //each time the frame is updated, we check the position of the mouse in the gridmap 
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPosition = gridposition(Input.mousePosition, true);

        //when we select a unit, we make currentbuttonpressed = numberoftiles + 1, so that it enters this if.
        if (Input.GetMouseButtonUp(0) && CurrentButtonPressed >= numberoftiles)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            if (map.HasTile(gridPosition))
            {
                Vector3 where = getWorldPosition((Vector3)gridPosition);
                //the unit is built from our unit dictionary, then selected and it gets its respective owner
                GameObject.Instantiate(selectedbuildables[unitpressed], map.GetCellCenterWorld(gridPosition), Quaternion.identity).SetActive(true);
                unitScript spawnedUnit = getunit(gridPosition);
                if(activeplayer < 1)
                {
                    activeplayer = 1;
                }
                Debug.Log("you can't have neutral units");
                spawnedUnit.ownerChange(activeplayer);
            }
        }

        //when the button gets pressed, we insert the appropriate tile in the tilemap.
        if(CurrentButtonPressed < numberoftiles)
        {
            if (Input.GetMouseButton(0) && ItemButtons[CurrentButtonPressed].Clicked && Time.time - lastStep > timeBetweenSteps)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                levelTile selectedtile = tileBases[CurrentButtonPressed];
                int typenumber = (int)selectedtile.type;
                if (typenumber < 1000)
                {
                    map.SetTile(gridPosition, tileBases[CurrentButtonPressed]);
                    lastStep = Time.time;
                    if (tileBases[CurrentButtonPressed].controllable)
                    {
                        map.GetInstantiatedObject(gridPosition).GetComponent<controllable_script>().ownerchange(activeplayer, 1);
                    }
                }
                if (typenumber >= 2000)
                {
                    lastStep = Time.time;
                    conditions.SetTile(gridPosition, tileBases[CurrentButtonPressed]);
                }
                //Destroy(GameObject.FindGameObjectWithTag("ItemImage"));
            }
        }
        if (Input.GetMouseButton(1))
        {
            map.SetTile(gridPosition, null);
            conditions.SetTile(gridPosition, null);
            units.SetTile(gridPosition, null);
            if(getunitprefab(gridPosition) != null)
            {
                Destroy(getunitprefab(gridPosition));
            }
        }

    }

    public void onClick(unitScript unit)
    {
        unitpanel.SetActive(false);
        CurrentButtonPressed = numberoftiles + 1;
        unitpressed = unit.unitname;
        Destroy(GameObject.FindGameObjectWithTag("ItemImage"));
    }
    public void TurnPanelOn()
    {
        unitpanel.SetActive(true);
    }
    public void setActivePlayer(int player)
    {
        activeplayer = player;
    }
    public Vector3Int gridposition(Vector3 position, bool screen = false)
    {
        if (screen)
        {
            position = _mainCamera.ScreenToWorldPoint(position);
        }

        Vector3Int gridposition = map.WorldToCell(position);
        return gridposition;
    }
    private Vector3 getWorldPosition(Vector3 gridposition)
    {
        Vector3 worldPosition = new Vector3();
        worldPosition = (gridposition*(map.cellGap.x+map.cellSize.x)) + new Vector3(map.cellSize.x / 2, map.cellSize.x / 2, 0);
        return worldPosition;
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
    public GameObject getunitprefab(Vector3Int position)
    {
        return getunitprefab(Camera.main.WorldToScreenPoint(map.GetCellCenterWorld(position)));
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
}
