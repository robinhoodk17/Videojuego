using TMPro;
using UnityEngine;

public class barracks_script : MonoBehaviour
{
    public GameObject buttonsprefab;
    public GameObject Content;
    private MapManager manager;
    private void Start()
    {

        manager = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
    }
    public void onActivation()
    {

        //buttons is a prefab where we store the selected units (which we save in the pool of humans and avatars)
        foreach (var entry in manager.selectedbuildables)
        {
            GameObject button = Instantiate(buttonsprefab, new Vector3(0, 0, 0), Quaternion.identity);
            button.transform.SetParent(Content.transform, false);
            string unitname = entry.Value.GetComponent<unitScript>().unitname;
            button.GetComponent<unitProduction>().ID = unitname;
            button.SetActive(true);
            button.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = unitname;
            button.GetComponent<unitProduction>().customAwake(entry.Value);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
