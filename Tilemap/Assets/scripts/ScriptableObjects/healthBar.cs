using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class healthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;
    public void SetMaxHealth()
    {
        fill.color = gradient.Evaluate(1f);
    }
    public void SetHealth(int health, int maxhealth)
    {
        slider.value = (float) health / maxhealth;
        fill.color = gradient.Evaluate(slider.value);
    }
}
