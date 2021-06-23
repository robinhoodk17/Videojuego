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
    public List<TileBase> tileBases;

    //this is the current tilemap we are using (the same as in MapManager)
    [SerializeField]
    private Tilemap map;

    //this is the button that was most recently pressed, and so the kind of tile that will be spawned.
    public int CurrentButtonPressed;

    public GameObject[] ItemImage;

    private void Update()
    {
        //each time the frame is updated, we check the position of the mouse in the gridmap 
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPosition = map.WorldToCell(mousePosition);
        
        
        //when the button gets pressed, we insert the appropriate tile in the tilemap.
        if (Input.GetMouseButton(0) && ItemButtons[CurrentButtonPressed].Clicked)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
                map.SetTile(gridPosition, tileBases[CurrentButtonPressed]);
            //Destroy(GameObject.FindGameObjectWithTag("ItemImage"));
        }
        if (Input.GetMouseButtonDown(1))
        {
            map.SetTile(gridPosition, null);
        }

    }

}
