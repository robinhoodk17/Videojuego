using TMPro;
using UnityEngine;

public class controllable_script : MonoBehaviour
{
    public int HP;
    public int maxHP = 175;
    public int owner = 0;
    public GameObject flagCanvas;
    public GameObject healthbar;
    public void ownerchange(int newowner, double capturedHP)
    {
        flagCanvas.transform.GetChild(owner).gameObject.SetActive(false);
        owner = newowner;
        flagCanvas.transform.GetChild(newowner).gameObject.SetActive(true);
        HP = (int)(capturedHP * maxHP);
        healthbar.SetActive(true);
        healthChanged();
    }

    public void ownerloss()
    {
        flagCanvas.transform.GetChild(owner).gameObject.SetActive(false);
        owner = 0;
        healthbar.SetActive(false);
    }
    public void healthChanged()
    {
        healthbar.GetComponent<healthBar>().SetHealth(HP, maxHP);
        healthbar.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = HP.ToString();
    }
}
