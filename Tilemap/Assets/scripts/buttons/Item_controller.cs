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
    private SelectionManager selectionmanager;
    //this is true when it finds the mapeditor, and false when it finds the selectionmanager
    private bool editorOrSelectionManager = false;

    // This script controls the buttons in the map editor
    void Start()
    {
        Clicked = false;
        quantityText.text = quantity.ToString();
        if(GameObject.FindGameObjectWithTag("MapEditorManager"))
        {
            editor = GameObject.FindGameObjectWithTag("MapEditorManager").GetComponent<MapEditorManager>();
            editorOrSelectionManager = true;
        }
        else
        {
            selectionmanager = GameObject.FindGameObjectWithTag("SelectionManager").GetComponent<SelectionManager>();
        }
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
        if(editorOrSelectionManager)
        {
            Instantiate(editor.ItemImage[ID], new Vector3(worldPosition.x, worldPosition.y, 0), Quaternion.identity);
            editor.CurrentButtonPressed = ID;
        }
    }
}
