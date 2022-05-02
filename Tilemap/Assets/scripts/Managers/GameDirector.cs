using System;
using UnityEngine;
using System.Collections.Generic;
using NodeCanvas.DialogueTrees;
using NodeCanvas.BehaviourTrees;

public class GameDirector : MonoBehaviour
{
    //private static string SceneSaver = "currentScene";
    [SerializeField]
    private int _DialogueCounter;
    public DialogueTreeController dialogueTreeController;

    public int DialogueCounter{
        get {return _DialogueCounter;}
        set {_DialogueCounter = value;}
    }
 
    public List<string> dialogueHistory;
 
    void Start(){
        /*
        switch(currentScene)
        {
            case("tutorial"):
                dialogueTreeController.StartDialogue();
                bre

        }
        */
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
    void Update()
    {/*
        if(Input.GetKeyDown("w"))
        {
            Debug.Log(dialogueHistory.Count);
        }
        */
    }

    public void DialogueFinished()
    {
        _DialogueCounter++;
        DialogueCounter = _DialogueCounter;
        PlayerPrefs.SetInt("DialogueNumber", _DialogueCounter);
    }
}
