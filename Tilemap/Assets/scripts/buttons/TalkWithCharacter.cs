using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkWithCharacter : MonoBehaviour
{
    public GameObject TlalocDialogue;
    // Start is called before the first frame update
    public void OnTlalocClick()
    {
        TlalocDialogue.SetActive(true);
    }
}
