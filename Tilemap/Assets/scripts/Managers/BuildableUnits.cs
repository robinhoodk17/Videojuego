using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildableUnits : MonoBehaviour
{
    public static BuildableUnits instance;
    public List<GameObject> Buildables;
    // Start is called before the first frame update
    void Start()
    {
        if (instance != null && instance != this)
        {
            gameObject.SetActive(false);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

    }

}
