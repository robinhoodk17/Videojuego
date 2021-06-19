using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    map.SetTile(gridPosition, tileBases[y]);

     */
    [SerializeField]
    private Tilemap map;

    [SerializeField]
    private List<TileData> tileDatas;

    [SerializeField]
    public List<TileBase> tileBases;

    private Dictionary<TileBase, TileData> dataFromTiles;
    //this creates the dictionary using the key "tileBase" and stores the data of each tile on its "TileData"
    private void Awake()
    {
        dataFromTiles = new Dictionary<TileBase, TileData>();

        foreach (var tileData in tileDatas)
        {
            foreach (var tile in tileData.tiles)
            {
                dataFromTiles.Add(tile, tileData);
            }
        }

    }

    
    void Update()
    {
        /*This is an example of how to get all the info in a tile and then change it to another tile.
        if(Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPosition = map.WorldToCell(mousePosition);

            TileBase clickedTile = map.GetTile(gridPosition);

            string tilename = dataFromTiles[clickedTile].name;
            string tilestatus = dataFromTiles[clickedTile].status;

            print("At position " + gridPosition + "there is a " + tilename + " with " + tilestatus + " weather");
            map.SetTile(gridPosition, tileBases[0]);
        }*/
    }
    public string Getname(Vector2 worldPosition)
    {
        Vector3Int gridPosition = map.WorldToCell(worldPosition);

        TileBase tile = map.GetTile(gridPosition);

        if (tile == null)
            return "null";

        string tilename = dataFromTiles[tile].name;

        return tilename;
    }

    public string Getstatus(Vector2 worldPosition)
    {
        Vector3Int gridPosition = map.WorldToCell(worldPosition);

        TileBase tile = map.GetTile(gridPosition);

        if (tile == null)
            return "null";
        string tilestatus = dataFromTiles[tile].status;

        return tilestatus;
    }
}
