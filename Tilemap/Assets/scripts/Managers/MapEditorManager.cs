using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using Photon.Pun;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Text.RegularExpressions;

public class MapEditorManager : MonoBehaviour
{


    #region EventsVariables
    public static MapEditorManager Instance;
    public List<(eventtrigger,happenings, int)> EventsDuringGame;
    public eventtrigger currentTrigger;
    public int triggerCounter;
    public happenings currentHappening;
    public int PlayerAttacks = 1;
    public List<GameObject> EventEditingCanvases;
    public GameObject DefaultCanvas;
    public List<TMP_Text> Triggercounters;
    bool EventEditingMode = false;
    #endregion



    /*Here are stored the possible tiles for adding to the tilemap. You must add them in the Inspector.
    forest: 0
    mountain: 1
    planes: 2
    river: 3
    road: 4
    farm: 5
    bonfire: 6
    barracks:7
    HQ:8
     */
    public Item_controller[] ItemButtons;
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
    public GameObject EventsCanvas;
    public GameObject EventTriggersCanvas;
    private int numberoftiles;
    private Camera _mainCamera;
    private float timeBetweenSteps = 0.05f;
    float lastStep;
    public int numberofcontrollables = 0;

    public bool dragging = false;
    private Vector3Int topLeft;
    private Vector3Int bottomRight;
    private bool gotupperLeft = false;
    private void Awake()
    {
        Instance = this;
    }
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
        unitpanel.SetActive(false);
    }
    private void Update()
    {
        //each time the frame is updated, we check the position of the mouse in the gridmap 
        Vector3Int gridPosition = gridposition(Mouse.current.position.ReadValue(), true);

        //when we select a unit, we make currentbuttonpressed = numberoftiles + 1, so that it enters this if.
        if (Mouse.current.leftButton.wasReleasedThisFrame && CurrentButtonPressed >= numberoftiles && !dragging && !EventEditingMode)
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
                if (activeplayer < 1)
                {
                    activeplayer = 1;
                }
                spawnedUnit.ownerChange(activeplayer);
                spawnedUnit.customAwake();
            }
        }

        //when the button gets pressed, we insert the appropriate tile in the tilemap.
        if (CurrentButtonPressed < numberoftiles && !EventEditingMode)
        {
            if (Mouse.current.leftButton.isPressed && ItemButtons[CurrentButtonPressed].Clicked && Time.time - lastStep > timeBetweenSteps && !dragging)
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
        //this is just to destroy a tile with right click
        if (Mouse.current.rightButton.isPressed && !EventEditingMode)
        {
            map.SetTile(gridPosition, null);
            conditions.SetTile(gridPosition, null);
            units.SetTile(gridPosition, null);
            if (getunitprefab(gridPosition) != null)
            {
                Destroy(getunitprefab(gridPosition));
            }
        }

        #region mirroring
        if (Mouse.current.leftButton.wasPressedThisFrame && dragging &&!EventEditingMode)
        {
            topLeft = gridposition(Mouse.current.position.ReadValue(), true);
            gotupperLeft = true;
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame && dragging && gotupperLeft &&!EventEditingMode)
        {
            bottomRight = gridposition(Mouse.current.position.ReadValue(), true);
            Vector3Int realtopLeft = new Vector3Int(Mathf.Min(topLeft.x, bottomRight.x), Mathf.Max(topLeft.y, bottomRight.y), topLeft.z);
            Vector3Int realbottomright = new Vector3Int(Mathf.Max(topLeft.x, bottomRight.x), Mathf.Min(topLeft.y, bottomRight.y), topLeft.z);
            dragging = false;
            gotupperLeft = false;
            rotatedmirror(realtopLeft, realbottomright);
        }
    #endregion
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
    public void setAttackingPlayerForHappenings(int player)
    {
        PlayerAttacks = player;
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
        worldPosition = (gridposition * (map.cellGap.x + map.cellSize.x)) + new Vector3(map.cellSize.x / 2, map.cellSize.x / 2, 0);
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
                if (map.HasTile(new Vector3Int(upperleft.x + i, upperleft.y - j, upperleft.z)))
                {
                    levelTile Tile = map.GetTile<levelTile>(new Vector3Int(upperleft.x + i, upperleft.y - j, upperleft.z));
                    Vector3Int rotated = new Vector3Int(newbottomright.x - i + 1, newbottomright.y + j, newbottomright.z);
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
                else
                {
                    Vector3Int rotated = new Vector3Int(newbottomright.x - i + 1, newbottomright.y + j, newbottomright.z);
                    map.SetTile(rotated, null);
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
    public void ButtonForTurningEventTriggersOn()
    {
        if(!EventTriggersCanvas.activeSelf && !EventsCanvas.activeSelf)
        {
            EventTriggersCanvas.SetActive(true);
        }
    }
    public void CancelEvent()
    {
        EventTriggersCanvas.SetActive(false);
        EventsCanvas.SetActive(false);
        EventEditingModeOff();
    }
    public void newEventTrigger(int trigger)
    {
        string triggerCounterstring;
        if(trigger != 2)
        {
            triggerCounterstring = Triggercounters[trigger].text;
            Regex searchstrting = new Regex ("[0-9]+");
            triggerCounterstring = searchstrting.Match(triggerCounterstring).Value;
            int result;
            int.TryParse(triggerCounterstring, out result);
            triggerCounter = result;
        }
        else
        {
            triggerCounter = PlayerAttacks+1;
        }
        currentTrigger = (eventtrigger)trigger;
        EventTriggersCanvas.SetActive(false);
        EventsCanvas.SetActive(true);

    }

    public void newEvent(int happening)
    {
        currentHappening = (happenings)happening;
        turnEventsCanvasOff();
        EventEditingModeOn(happening);
    }
    public void confirmEvent()
    {
        EventsDuringGame.Add((currentTrigger, currentHappening, triggerCounter));
        EventEditingModeOff();
        EventTriggersCanvas.SetActive(true);
    }
    public void EventEditingModeOn(int happening)
    {
        DefaultCanvas.SetActive(false);
        EventEditingCanvases[(int)happening].SetActive(true);
    }
    public void EventEditingModeOff()
    {
        foreach(GameObject canvas in EventEditingCanvases)
        {
            canvas.SetActive(false);
        }
        DefaultCanvas.SetActive(true);
    }
    public void StartOfTurnButtonClicked(TextMeshProUGUI turnnumber)
    {
        int turn = Convert.ToInt16(turnnumber.text);
    }
    public void TurnEventsCanvasOn()
    {
        if(!EventsCanvas.activeSelf)
        {
            EventsCanvas.SetActive(true);
        }
    }
    public void turnEventsCanvasOff()
    {
        EventsCanvas.SetActive(false);
        EventTriggersCanvas.SetActive(false);
    }
}