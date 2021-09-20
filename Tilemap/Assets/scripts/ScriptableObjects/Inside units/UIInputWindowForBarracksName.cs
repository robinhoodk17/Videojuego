using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIInputWindowForBarracksName : MonoBehaviour
{

    [SerializeField]
    private TMP_InputField inputField;
    unitScript newUnit;
    // Start is called before the first frame update
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show(unitScript spawnedUnit)
    {
        gameObject.SetActive(true);
        newUnit = spawnedUnit;
    }

    public void acceptPressed()
    {
        newUnit.barracksname = inputField.text;
        inputField.text = "";
        newUnit.customAwake();
        Hide();
    }
}
