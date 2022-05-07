using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthOld : MonoBehaviour
{
    public GameObject sliderObject;
    public Canvas canvas;

    private GameObject currentSlider;
    private Player playerScript;
    private Slider slider;

    // Start is called before the first frame update
    void Start()
    {
        SpawnSlider();
        playerScript = GetComponent<Player>();
        slider.maxValue = playerScript.health;
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = playerScript.health;
    }

    private void SpawnSlider()
    {
        currentSlider = Instantiate(sliderObject, canvas.transform);
        slider = currentSlider.GetComponent<Slider>();
    }
}
