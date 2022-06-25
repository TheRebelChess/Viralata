using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHealthBar : MonoBehaviour
{
    public EnemyHealthBar enemyHealthBar;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemyHealthBar.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemyHealthBar.enabled = false;
        }
    }
}
