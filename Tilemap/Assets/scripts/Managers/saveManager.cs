using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class saveManager : MonoBehaviour
{
    string saveseparator = "#SAVE-VALUE#";
    string tileseparator = "#NEW-TILE#";
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
    private void Start()
    {
        foreach(levelTile tile in tileBases)
        {
            tileDictionary[tile.type.ToString()] = tile;
        }
    }
    public void Save()
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
        File.WriteAllText(Application.dataPath + "/SaveFiles" + "/save.txt", savestring);
    }

    public void Load()
    {
        string saveString = File.ReadAllText(Application.dataPath + "/SaveFiles" + "/save.txt");
        string[] alltiles = saveString.Split(new[] { tileseparator }, System.StringSplitOptions.None);
        foreach(string currentTile in alltiles)
        {
            string[] contents = currentTile.Split(new[] { saveseparator }, System.StringSplitOptions.None);
            int x = int.Parse(contents[0]);
            int y = int.Parse(contents[1]);
            int z = int.Parse(contents[2]);
            Vector3Int where = new Vector3Int(x, y, z);
            string tile = contents[3];
            map.SetTile(where, tileDictionary[tile]);
            if(bool.Parse(contents[4]))
            {
                GameObject controllable = map.GetInstantiatedObject(where);
                controllable.GetComponent<controllable_script>().ownerchange(int.Parse(contents[5]), int.Parse(contents[6]));
            }
            Debug.Log(where + " " + tileDictionary[tile]);
        }
    }
}
