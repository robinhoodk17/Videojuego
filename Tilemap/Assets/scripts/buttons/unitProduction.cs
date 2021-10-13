using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class unitProduction : MonoBehaviour
{
    public string ID;
    public bool Clicked = false;
    public TextMeshProUGUI name;
    public TextMeshProUGUI foodCost;
    public TextMeshProUGUI SUPCost;
    private MapManager manager;
    public Image sprite;
    // This script controls the buttons in the mapmanager
    void Awake()
    {
        Clicked = false;
        manager = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
    }

    public void customAwake(GameObject unit)
    {
        sprite.sprite = unit.transform.GetChild(3).GetComponent<SpriteRenderer>().sprite;
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
