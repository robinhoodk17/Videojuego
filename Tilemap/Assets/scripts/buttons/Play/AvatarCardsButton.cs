using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarCardsButton : MonoBehaviour
{
    public GameObject Humans;
    public GameObject Avatars;

    public void OnClick()
    {
        Humans.SetActive(false);
        Avatars.SetActive(true);
    }
}
