using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Slider healthSlider;

    public void UpdateHealth(float h)
    {
        healthSlider.value = h;
    }
}
