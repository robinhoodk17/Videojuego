using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class healthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;
    public void SetColor(int owner)
    {
        if(owner == 1)
        {
            fill.color = Color.green;
        }
        
        if(owner == 2)
        {
            fill.color = Color.red;
        }

        if(owner == 3)
        {
            fill.color = Color.blue;
        }

        if(owner == 4)
        {
            fill.color = Color.magenta;
        }

    }
    public void SetMaxHealth()
    {
        fill.color = gradient.Evaluate(1f);
    }
    public void SetHealth(int health, int maxhealth)
    {
        slider.value = (float) health / maxhealth;
    }
}
