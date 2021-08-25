using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class infoPanel : MonoBehaviour
{
    public GameObject panel;
    // Start is called before the first frame update

    
    public void showPanel()
    {
        panel.SetActive(true);
    }

    public void hidePanel()
    {
        panel.SetActive(false);
    }
}
