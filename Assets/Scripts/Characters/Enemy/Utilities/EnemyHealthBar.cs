using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public GameObject sliderObject;
    public Transform spawnPoint;
    public Canvas canvas;
    public Image enemyHpBar;

    private GameObject currentSlider;
    private EnemyMovement enemyScript;
    private Slider slider;

    // Start is called before the first frame update
    void Start()
    {
        SpawnSlider();
        enemyScript = GetComponent<EnemyMovement>();
        slider.maxValue = enemyScript.health;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentSlider == null)
            return;

        Vector3 position = Camera.main.WorldToScreenPoint(spawnPoint.position);
        currentSlider.transform.position = position;

        slider.value = enemyScript.health;
        if (enemyScript.health <= 0)
        {
            currentSlider.gameObject.SetActive(false);
        }

        Enemy.Instance.percHealth = Enemy.Instance.health / Enemy.Instance.MaxHealth;

        enemyHpBar.fillAmount = Enemy.Instance.percHealth;

        if(Enemy.Instance.percHealth < 0)
        {
            Enemy.Instance.percHealth = 0;
        }

    }

    private void SpawnSlider()
    {
        currentSlider = Instantiate(sliderObject, spawnPoint.position, spawnPoint.rotation, canvas.transform);
        slider = currentSlider.GetComponent<Slider>();
    }
}
