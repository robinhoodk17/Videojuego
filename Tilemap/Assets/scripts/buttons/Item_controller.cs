using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;


public class Item_controller : MonoBehaviour
{
    public int ID;
    public int quantity;
    public TextMeshProUGUI quantityText;
    public bool Clicked = false;
    private MapEditorManager editor;

    // This script controls the buttons in the map editor
    void Start()
    {
        Clicked = false;
        quantityText.text = quantity.ToString();
        editor = GameObject.FindGameObjectWithTag("MapEditorManager").GetComponent<MapEditorManager>();
    }

    public void ButtonClicked()
    {
        if(GameObject.FindGameObjectWithTag("ItemImage"))
        {
            Destroy(GameObject.FindGameObjectWithTag("ItemImage"));
        }
        Clicked = true;
        Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        Instantiate(editor.ItemImage[ID], new Vector3(worldPosition.x, worldPosition.y, 0), Quaternion.identity);
        editor.CurrentButtonPressed = ID;
        //print(ID);
    }
}
