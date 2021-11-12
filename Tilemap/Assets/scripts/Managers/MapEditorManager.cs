using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using Photon.Pun;

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
    private List<GameObject> Buildables = new List<GameObject>();
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
    public int numberofcontrollables = 0;

    public bool dragging = false;
    private Vector3Int topLeft;
    private Vector3Int bottomRight;
    private bool gotupperLeft = false;
    private void Start()
    {
        PhotonNetwork.OfflineMode = true;
        Buildables.Clear();
        Buildables = GameObject.FindGameObjectWithTag("BuildableUnits").GetComponent<BuildableUnits>().Buildables;
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
        Vector3Int gridPosition = gridposition(Input.mousePosition, true);

        //when we select a unit, we make currentbuttonpressed = numberoftiles + 1, so that it enters this if.
        if (Input.GetMouseButtonUp(0) && CurrentButtonPressed >= numberoftiles && !dragging)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            if (map.HasTile(gridPosition))
            {
                //the unit is built from our unit dictionary, then selected and it gets its respective owner

                if (getunitprefab(gridPosition) != null)
                {
                    Destroy(getunitprefab(gridPosition));
                }

                PhotonNetwork.Instantiate("Units/" + selectedbuildables[unitpressed].name, map.GetCellCenterWorld(gridPosition), Quaternion.identity);
                unitScript spawnedUnit = getunit(gridPosition);
                if(activeplayer < 1)
                {
                    activeplayer = 1;
                }
                spawnedUnit.ownerChange(activeplayer);
            }
        }

        //when the button gets pressed, we insert the appropriate tile in the tilemap.
        if(CurrentButtonPressed < numberoftiles)
        {
            if (Input.GetMouseButton(0) && ItemButtons[CurrentButtonPressed].Clicked && Time.time - lastStep > timeBetweenSteps && !dragging)
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
                        GameObject controllable = map.GetInstantiatedObject(gridPosition);
                        PhotonView tileID = controllable.GetComponent<PhotonView>();
                        tileID.ViewID = 999 - numberofcontrollables;
                        numberofcontrollables++;
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

        if(Input.GetMouseButtonDown(0) && dragging)
        {
            topLeft = gridposition(Input.mousePosition, true);
            gotupperLeft = true;
        }
        if(Input.GetMouseButtonUp(0) && dragging && gotupperLeft)
        {
            bottomRight = gridposition(Input.mousePosition, true);
            rotatedmirror(topLeft, bottomRight);
            gotupperLeft = false;
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

    private void rotatedmirror(Vector3Int upperleft, Vector3Int bottomright)
    {
        Vector3Int upperright = new Vector3Int(bottomright.x, upperleft.y, upperleft.z);
        Vector3Int bottomleft = new Vector3Int(upperleft.x, bottomright.y, upperleft.z);
        int height = (upperleft.y - bottomleft.y);
        int width = (upperright.x - upperleft.x);
        Vector3Int newbottomright = new Vector3Int(upperright.x + width, bottomleft.y, upperleft.z);
        for (int i = 0; i <= width; i++)
        {
            for (int j = 0; j <= height; j++)
            {
                levelTile Tile = map.GetTile<levelTile>(new Vector3Int(upperleft.x + i, upperleft.y - j, upperleft.z));
                Vector3Int rotated = new Vector3Int(newbottomright.x - i, newbottomright.y + j, newbottomright.z);
                map.SetTile(rotated, Tile);
                if (Tile.controllable)
                {
                    GameObject controllable = map.GetInstantiatedObject(rotated);
                    PhotonView tileID = controllable.GetComponent<PhotonView>();
                    tileID.ViewID = 999 - numberofcontrollables;
                    numberofcontrollables++;
                    map.GetInstantiatedObject(rotated).GetComponent<controllable_script>().ownerchange(activeplayer, 1);
                }
            }
        }
        dragging = false;
    }
    public void startDrag()
    {
        dragging = true;
        Destroy(GameObject.FindGameObjectWithTag("ItemImage"));
    }
}
