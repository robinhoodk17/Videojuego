using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDirectorMap : MonoBehaviour
{
    int DialogueCounter;
    int TurnNumber; 
    // Start is called before the first frame update
    void Start()
    {
        DialogueCounter = PlayerPrefs.GetInt("DialogueNumber", 1);
        TurnNumber = 0;
        onTurnStart();
    }

    // Update is called once per frame
    void onTurnStart()
    {
        TurnNumber++;
        switch(DialogueCounter){
            case 0:
                tutorial(DialogueCounter);
                break;
        }

    }

    void tutorial(int turnnumber)
    {

    }

    void custom(int turnnumber)
    {
        
    }
}
