using System;
using UnityEngine;
using System.Collections.Generic;
using NodeCanvas.DialogueTrees;
using NodeCanvas.BehaviourTrees;

public class GameDirector : MonoBehaviour
{
    public static GameDirector instance;

    [SerializeField]
    private int _DialogueCounter;
    private int TurnNumber;
    public NetworkManager networkmanager;
    public DialogueTreeController dialogueTreeController;

    public int DialogueCounter{
        get {return _DialogueCounter;}
        set {_DialogueCounter = value;}
    }
 
    public List<string> dialogueHistory;

    void Start()
    {
        _DialogueCounter = PlayerPrefs.GetInt("DialogueNumber", 0);
        _DialogueCounter = DialogueCounter;
        if(_DialogueCounter > 1)
        {
            _DialogueCounter = 0;
            DialogueCounter = 0;
        }
        DialogueTree.OnDialogueStarted += (x)=>{ dialogueHistory.Clear(); };
        DialogueTree.OnSubtitlesRequest += OnSubtitlesRequest;
        dialogueTreeController.StartBehaviour();

    }
   
    void OnSubtitlesRequest(SubtitlesRequestInfo info){
        //dialogueHistory.Add(info.statement.text);
    }

    public void DialogueFinished(bool ChangeScene = false, string whichScene = "AI")
    {
        _DialogueCounter++;
        DialogueCounter = _DialogueCounter;
        PlayerPrefs.SetInt("DialogueNumber", _DialogueCounter);
        if(ChangeScene)
        {
            networkmanager.ChangeScene(whichScene);
        }
    }

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
