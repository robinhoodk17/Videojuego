using System.Collections.Generic;
using UnityEngine;

public class barracks_script : MonoBehaviour
{
    public GameObject buttonsprefab;
    public GameObject Content;

    //we call onActivation when the player clicks on the barracks during playtime
    public void onActivation(Dictionary<string, GameObject> selectedBuildables)
    {
        //buttons is a prefab where we store the selected units (which we save in the pool of humans and avatars)
        foreach (var entry in selectedBuildables)
        {
            GameObject button = Instantiate(buttonsprefab, new Vector3(0, 0, 0), Quaternion.identity);
            unitProduction buttonscript = button.GetComponent<unitProduction>();
            button.transform.SetParent(Content.transform, false);
            unitScript currentUnit = entry.Value.GetComponent<unitScript>();
            string unitname = currentUnit.unitname;
            button.GetComponent<unitProduction>().ID = unitname;
            button.SetActive(true);
            buttonscript.name.text = unitname;
            buttonscript.foodCost.text = currentUnit.foodCost.ToString();
            buttonscript.SUPCost.text = currentUnit.SUPCost.ToString();
            button.GetComponent<unitProduction>().customAwake(entry.Value);
        }
    }

}
