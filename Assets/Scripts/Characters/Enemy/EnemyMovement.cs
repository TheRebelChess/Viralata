using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    private Transform target;
    private NavMeshAgent navMeshAgent;
    private Animator enemyAnimator;

    public int health = 10;
    public Transform rayCastOrigin;

    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void LateUpdate()
    {

        Debug.Log(target?.position);
        if (navMeshAgent.velocity.magnitude > 0)
        {
            enemyAnimator.SetBool("Move", true);
        }
        else
        {
            enemyAnimator.SetBool("Move", false);
        }
        Debug.DrawRay(rayCastOrigin.position, rayCastOrigin.forward * 10f, Color.red);
        if (target != null)
        {

            UpdateLineOfSight();
        }
    }

    private void UpdateLineOfSight()
    {
        //transform.LookAt(target);
        RaycastHit hit;
        //Debug.DrawRay(rayCastOrigin.position, rayCastOrigin.forward * 10f, Color.red);
        if (Physics.Raycast(rayCastOrigin.position, rayCastOrigin.forward, out hit, 10f))
        {
            if (hit.transform.tag == "Player")
            {
                navMeshAgent.SetDestination(hit.transform.position);
            }
            else
            {
                target = null;
            }
        }
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
        navMeshAgent.SetDestination(target.position);
    }
}
