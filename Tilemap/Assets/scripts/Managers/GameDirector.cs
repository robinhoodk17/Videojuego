using NodeCanvas.DialogueTrees;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    private static string SceneSaver = "currentScene";
    public DialogueTreeController dialogueTreeController;
    void Start()
    {
        string currentScene = PlayerPrefs.GetString("currentScene", string.Empty);
        if(string.IsNullOrEmpty(currentScene))
        {
            currentScene = "tutorial";
            PlayerPrefs.SetString(SceneSaver, currentScene);
        }
        switch(currentScene)
        {
            case("tutorial"):
                dialogueTreeController.StartDialogue();
                break;

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
