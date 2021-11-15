using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class unitUnloadFromTransport : MonoBehaviour
{
    public string ID;
    public bool Clicked = false;
    public TextMeshProUGUI name;
    public TextMeshProUGUI foodCost;
    public TextMeshProUGUI SUPCost;
    private SelectionManager manager;
    public Image sprite;
    public unitScript unitHeld;
    // This script controls the buttons in the mapmanager
    void Awake()
    {
        Clicked = false;
        manager = GameObject.FindGameObjectWithTag("SelectionManager").GetComponent<SelectionManager>();
    }

    public void customAwake(GameObject unit)
    {
        sprite.sprite = unit.transform.GetChild(3).GetComponent<SpriteRenderer>().sprite;
    }
    public void ButtonClicked()
    {
        manager.unLoadClicked(this.gameObject);
        manager.unloadUnit = unitHeld;
    }
    public void destroyThis()
    {
        Destroy(this.gameObject);
    }
}
