using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadButton : MonoBehaviour
{
    public string scenename;
    private LoadLevel levelLoader;

    // Start is called before the first frame update
    void Start()
    {
        levelLoader = GameObject.FindGameObjectWithTag("GameController").GetComponent<LoadLevel>();
    }

    public void ButtonClicked()
    {
        levelLoader.LoadScene(scenename);
    }
}
