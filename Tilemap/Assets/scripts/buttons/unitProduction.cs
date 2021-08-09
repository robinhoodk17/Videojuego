using UnityEngine;
using TMPro;

public class unitProduction : MonoBehaviour
{
    public int ID;
    public bool Clicked = false;
    private MapManager manager;

    // This script controls the buttons in the mapmanager
    void Awake()
    {
        Clicked = false;
        manager = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
    }

    public void ButtonClicked()
    {
        Clicked = true;
        manager.CurrentButtonPressed = ID;
        manager.clicked = true;
    }

    /*
    [SerializeField]
    private TextMeshProUGUI myText;
    public void setText(string textString)
    {
        myText.text = textString;
    }*/
}
