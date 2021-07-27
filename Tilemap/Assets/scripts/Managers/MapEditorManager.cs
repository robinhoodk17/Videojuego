using System.Collections;
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

    //this is the current tilemap we are using (the same as in MapManager)
    [SerializeField]
    private Tilemap map, conditions, units;

    //this is the button that was most recently pressed, and so the kind of tile that will be spawned.
    public int CurrentButtonPressed;

    public GameObject[] ItemImage;
    public List<GameObject> Prefabs;
    public int numberofunitsingame;

    private void Update()
    {
        //each time the frame is updated, we check the position of the mouse in the gridmap 
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPosition = map.WorldToCell(mousePosition);


        //when the button gets pressed, we insert the appropriate tile in the tilemap.
        int numberoftiles = tileBases.Count;
        if (Input.GetMouseButtonUp(0) && CurrentButtonPressed >= numberoftiles)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            if (map.HasTile(gridPosition))
            {
                Vector3 where = getWorldPosition((Vector3)gridPosition);
                GameObject.Instantiate(Prefabs[CurrentButtonPressed - numberoftiles], where, Quaternion.identity);
                //units.SetTile(gridPosition, tileBases[CurrentButtonPressed]);
            }
        }
        if (Input.GetMouseButton(0) && ItemButtons[CurrentButtonPressed].Clicked && CurrentButtonPressed < numberoftiles)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            levelTile selectedtile = tileBases[CurrentButtonPressed];
            int typenumber = (int)selectedtile.type;
            if (typenumber<1000)
            {
                map.SetTile(gridPosition, tileBases[CurrentButtonPressed]);
            }
            if (typenumber >= 2000)
            {
                conditions.SetTile(gridPosition, tileBases[CurrentButtonPressed]);
            }
            //Destroy(GameObject.FindGameObjectWithTag("ItemImage"));
        }
        if (Input.GetMouseButton(1))
        {
            map.SetTile(gridPosition, null);
            conditions.SetTile(gridPosition, null);
            units.SetTile(gridPosition, null);
        }

    }

    private Vector3 getWorldPosition(Vector3 gridposition)
    {
        Vector3 worldPosition = new Vector3();
        worldPosition = (gridposition*(map.cellGap.x+map.cellSize.x)) + new Vector3(map.cellSize.x / 2, map.cellSize.x / 2, 0);
        return worldPosition;
    }

}