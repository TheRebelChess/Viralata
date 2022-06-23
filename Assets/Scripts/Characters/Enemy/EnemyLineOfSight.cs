using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLineOfSight : MonoBehaviour
{
    private EnemyMovement EnemyMovement;
    // Start is called before the first frame update
    void Start()
    {
        EnemyMovement = GetComponentInParent<EnemyMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            EnemyMovement.SetTarget(other.transform);
        }
    }
}
