using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanCardsButton : MonoBehaviour
{
    public GameObject Humans;
    public GameObject Avatars;

    public void OnClick()
    {
        Humans.SetActive(true);
        Avatars.SetActive(false);
    }
}
