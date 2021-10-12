using TMPro;
using UnityEngine;
using Photon.Pun;

public class controllable_script : MonoBehaviourPun
{
    public int HP;
    public int maxHP = 175;
    public int owner = 0;
    public GameObject flagCanvas;
    public GameObject healthbar;
    public void ownerchange(int newowner, double capturedHP)
    {
        photonView.RPC("ownerChangeNetwork", RpcTarget.All, newowner, capturedHP);
    }

    [PunRPC]
    public void ownerChangeNetwork(int newowner, double capturedHP)
    {
        flagCanvas.transform.GetChild(owner).gameObject.SetActive(false);
        owner = newowner;
        flagCanvas.transform.GetChild(newowner).gameObject.SetActive(true);
        HP = maxHP;
        healthbar.SetActive(true);
        healthChanged();
    }

    public void ownerloss()
    {
        photonView.RPC("ownerlossNetwork", RpcTarget.All);
    }

    [PunRPC]

    public void ownerlossNetwork()
    {
        flagCanvas.transform.GetChild(owner).gameObject.SetActive(false);
        owner = 0;
        healthbar.SetActive(false);
    }


    public void healthChanged()
    {
        photonView.RPC("healthChangedNetwork", RpcTarget.All, HP, maxHP);
    }

    [PunRPC]
    public void healthChangedNetwork(int hp, int maxhp)
    {
        HP = hp;
        maxHP = maxhp;
        healthbar.GetComponent<healthBar>().SetHealth(HP, maxHP);
        healthbar.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = HP.ToString();
    }
}
