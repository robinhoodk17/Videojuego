using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class otherButtonScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        string text = gameObject.transform.parent.transform.parent.GetComponentInParent<unitScript>().ability;
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    public eventsScript ability;
    public void OnClick()
    {
        ability.Raise();
    }
}
