using UnityEngine;
using NodeCanvas.DialogueTrees;

public class startingDialogue : MonoBehaviour
{

	public DialogueTreeController dialogue;

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			dialogue.StartDialogue();
		}
	}
}