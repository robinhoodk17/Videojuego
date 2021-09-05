using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class saveManager : MonoBehaviour
{
    string saveseparator = "#SAVE-VALUE#";
    string tileseparator = "#NEW-TILE#";
    private UIInputWindowForSaveMap inputwindow;
    private UIInputWindowForSaveMap inputwindowforLoad;
    public GameObject savewindow;
    public GameObject loadwindow;
    /*forest: 0
    mountain: 1
    planes: 2
    river: 3
    road: 4
    farm: 5
    bonfire: 6
     */
    public List<levelTile> tileBases;
    private Dictionary<string, levelTile> tileDictionary = new Dictionary<string, levelTile>();
    [SerializeField]
    public Tilemap map, conditions;
    private int z;
    private void Start()
    {
        inputwindow = savewindow.GetComponent<UIInputWindowForSaveMap>();
        inputwindowforLoad = loadwindow.GetComponent<UIInputWindowForSaveMap>();
        foreach (levelTile tile in tileBases)
        {
            tileDictionary[tile.type.ToString()] = tile;
        }
        z = gridPosition(new Vector3(0, 0, 0)).z;
    }
    //this is called by the Save button
    public void SaveMap()
    {
        inputwindow.Show("IwIllFoRGeT");
    }
    public void LoadMap()
    {
        inputwindowforLoad.Show("lol nice try");
    }
    public void QuickSaveMap(string savename = "save")
    {

        string savestring = "";
        List<string> alltiles = new List<string>();
        string[] contents;
        foreach (var posi in map.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(posi.x, posi.y, posi.z);
            if (map.HasTile(localPlace))
            {
                levelTile Tile = map.GetTile<levelTile>(localPlace);
                if (Tile.controllable)
                {
                    controllable_script controllable = map.GetInstantiatedObject(localPlace).GetComponent<controllable_script>();
                    int owner = controllable.owner;
                    int HP = controllable.HP;
                    contents = new string[] {
                        "tile",
                        "" + localPlace.x,
                        "" + localPlace.y,
                        "" + localPlace.z,
                        "" + Tile.type,
                        "" + Tile.controllable,
                        "" + owner,
                        "" + HP
                    };
                }
                else
                {
                    contents = new string[] {
                        "tile",
                        "" + localPlace.x,
                        "" + localPlace.y,
                        "" + localPlace.z,
                        "" + Tile.type,
                        "" + Tile.controllable
                    };
                }
                savestring = string.Join(saveseparator, contents);
                alltiles.Add(savestring);
            }
        }
        savestring = string.Join(tileseparator, alltiles);
        File.WriteAllText(Application.dataPath + "/SaveFiles" + "/" + savename + ".map", savestring);
    }

    public void QuickLoadMap(string savename = "save")
    {
        string saveString = File.ReadAllText(Application.dataPath + "/SaveFiles" + "/" + savename + ".map");
        string[] alltiles = saveString.Split(new[] { tileseparator }, System.StringSplitOptions.None);
        foreach(string currentTile in alltiles)
        {
            string[] contents = currentTile.Split(new[] { saveseparator }, System.StringSplitOptions.None);
            if (contents[0] == "tile")
            {
                int x = int.Parse(contents[1]);
                int y = int.Parse(contents[2]);
                Vector3Int where = new Vector3Int(x, y, z);
                string tile = contents[4];
                map.SetTile(where, tileDictionary[tile]);
                if (bool.Parse(contents[5]))
                {
                    GameObject controllable = map.GetInstantiatedObject(where);
                    Debug.Log(controllable);
                    controllable.GetComponent<controllable_script>().ownerchange(int.Parse(contents[6]), int.Parse(contents[7]));
                }
            }
        }
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
}
