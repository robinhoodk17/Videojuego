using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    private GameObject selectedGameObject;
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var selection = hit.transform;
                Debug.Log("selected something");
                var selectionRenderer = selection.GetComponent<Renderer>();
                if (selectionRenderer != null)
                {
                    if(selection.CompareTag("Unit"))
                    {
                        GameObject square = selection.transform.GetChild(0).gameObject;
                        square.SetActive(true);
                        unitScript unit = selection.GetComponent<unitScript>();
                        Debug.Log(unit.typeOfUnit);
                    }
                }
            }

        }
    }
}
