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
    private Vector3 raycastTarget;
    private bool isAttacking = false;
    private bool canAttack = true;
    private bool isDead = false;
    private float stopFollowingTimer;

    public int health = 10;
    public Transform raycastOrigin;
    public BoxCollider lineOfSight;
    public BoxCollider swordTrigger;
    public float timeToStopFollowing = 3f;
    public float attackCooldown = 3f;
    public int damage = 3;

    public AudioSource audioSource;
    public AudioClip attackSFX;
    public AudioClip deathSFX;
    public AudioClip tookHitSFX;

    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponentInChildren<Animator>();
        raycastTarget = new Vector3(0, raycastOrigin.position.y, 0);
        swordTrigger.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            return;
        }

        DecreaseFollowTimer();

        if (navMeshAgent.velocity.magnitude > 0)
        {
            enemyAnimator.SetBool("Move", true);
        }
        else
        {
            enemyAnimator.SetBool("Move", false);
            if (target != null && canAttack)
            {
                Attack();
            }
        }

    }
    private void LateUpdate()
    {
        if (isDead)
        {
            return;
        }

        if (target != null && !isAttacking)
        {
            UpdateLineOfSight();
        }

    }

    private void Attack()
    {
        transform.LookAt(target);
        isAttacking = true;
        canAttack = false;
        enemyAnimator.SetTrigger("Attack");
        StartCoroutine(AttackTimer(.7f));
    }

    IEnumerator AttackTimer(float t)
    {
        yield return new WaitForSeconds(t);
        swordTrigger.enabled = true;
        yield return new WaitForSeconds(.3f); // end of sword swing
        swordTrigger.enabled = false;
        isAttacking = false;
        StartCoroutine(AttackCooldown(attackCooldown));
    }

    IEnumerator AttackCooldown(float t)
    {
        yield return new WaitForSeconds(t);
        canAttack = true;
    }

    private void UpdateLineOfSight()
    {
        raycastTarget.x = target.position.x;
        raycastTarget.z = target.position.z;
        RaycastHit hit;

        Debug.DrawRay(raycastOrigin.position, (raycastTarget - raycastOrigin.position).normalized * 10f, Color.red);

        if (Physics.Raycast(raycastOrigin.position, (raycastTarget - raycastOrigin.position).normalized, out hit, 10f))
        {
            if (hit.transform.CompareTag("Player"))
            {
                stopFollowingTimer = timeToStopFollowing;
                navMeshAgent.SetDestination(hit.transform.position);
            }
        }
    }

    private void DecreaseFollowTimer()
    {
        stopFollowingTimer -= Time.fixedDeltaTime;
        if (stopFollowingTimer <= 0)
        {
            target = null;
            lineOfSight.enabled = true;
        }
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
        stopFollowingTimer = timeToStopFollowing;
        navMeshAgent.SetDestination(target.position);
        lineOfSight.enabled = false;
    }

    public bool TakeHit(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            GetComponent<Collider>().enabled = false;
            navMeshAgent.enabled = false;
            audioSource.PlayOneShot(deathSFX);
            enemyAnimator.SetBool("Death", true);
            return true;
        }
        audioSource.PlayOneShot(tookHitSFX);

        return false;
    }
}
