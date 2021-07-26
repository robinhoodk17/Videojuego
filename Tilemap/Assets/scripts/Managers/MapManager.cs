using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;
using UnityEditor;

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
    public int numberOfPlayers;
    public BattleState state;
    public int activeplayer;
    public GameObject playerstartpanel;
    public TextMeshProUGUI activeplayertext;
    void Start()
    {
        activeplayer = 1;
        state = BattleState.PLAYERTURN;
    }
    void Update()
    {
        /*an example to make sure that the tiles are working: (it prints the tile you click and the 2 tiles on its right)
        if(Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPosition = map.WorldToCell(mousePosition);
            if (units.HasTile(gridPosition))
            {
                levelTile clickedTile = units.GetTile<levelTile>(gridPosition);

                string tilename = clickedTile.type.ToString();
                string tilestatus = clickedTile.status;

                print("At position " + gridPosition + "there is a " + tilename + " with " + tilestatus + " weather");

            }
            else
            {
                if (conditions.HasTile(gridPosition))
                {

                    levelTile clickedTile = conditions.GetTile<levelTile>(gridPosition);

                    string tilename = clickedTile.type.ToString();
                    string tilestatus = clickedTile.status;

                    print("At position " + gridPosition + "there is a " + tilename + " with " + tilestatus + " weather");
                }
                else
                {
                    if (map.HasTile(gridPosition))
                    {
                        levelTile clickedTile = map.GetTile<levelTile>(gridPosition);

                        string tilename = clickedTile.type.ToString();
                        string tilestatus = clickedTile.status;

                        print("At position " + gridPosition + "there is a " + tilename + " with " + tilestatus + " weather");
                        // An example to check the two tiles on the right
                        //for (int i = 1; i < 3; i++)
                        //{
                        //    clickedTile = map.GetTile<levelTile>((gridPosition + new Vector3Int(i,0,0)));
                        //    tilename = clickedTile.type.ToString();
                        //    tilestatus = clickedTile.status;
                        //    print("At the next position " + gridPosition + "there is a " + tilename + "with " + tilestatus + " weather");
                        //}
                    }

                }
            }
        }
        */
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

        if (activeplayer < numberOfPlayers)
        { activeplayer++; }
        else
        { activeplayer = 1; }
        state = BattleState.START;
        //making the turn start message pop up
        //panel turns off the panel after f seconds
        activeplayertext.text = "Player " + activeplayer.ToString();
        StartCoroutine(panel(2f));
        foreach (GameObject unit in allunits)
        {
            unitScript instanceofunit = unit.GetComponent<unitScript>();
            if(instanceofunit.owner == activeplayer)
            {
                instanceofunit.turnStart();
            }
        }
    }
}