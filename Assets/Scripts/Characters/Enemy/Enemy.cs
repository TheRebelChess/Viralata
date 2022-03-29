using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    enum EnemyMovementStates
    {
        PATROL,
        CHASING,
        RETURNING,
        COMBAT
    }


    // Private variables
    private EnemyMovementStates currentState;
    private Vector2 movementInput;
    private Animator enemyAnimator;
    private Transform target;

    private float visionRadius;

    private float baseSpeed;
    private float speedModifier;

    private float currentTargetRotation;
    private float timeToReachTargetRotation;
    private float dampedTargetRotationCurrentVelocity;
    private float dampedTargetRotationPassedTime;

    private bool shouldWalk;
    private bool isGrounded;
    private bool isReturning;
    private bool isInCombat;


    // Public Variables
    [HideInInspector] public PlayerInput input;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Transform mainCameraTransform;

    public PlayerData playerData;
    public float jumpHeight = 5f;
    public Transform groundCheckTransform;
    public LayerMask groundCheckLayerMask;

    public Transform home;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enemyAnimator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        ChangeState(EnemyMovementStates.PATROL);

        baseSpeed = 1f;
        timeToReachTargetRotation = playerData.timeToReachTargetRotation;
        shouldWalk = false;
    }

    private void Update()
    {
        Debug.Log(currentState);
        isGrounded = Physics.CheckSphere(groundCheckTransform.position, .2f, groundCheckLayerMask);

        if (target == null)
        {
            ChangeState(EnemyMovementStates.PATROL);
            return;
        }

        
        //else if (isInCombat)
        //{
        //    ChangeState(EnemyMovementStates.CHASING);
        //}

        //else if (target == home)
        //{
        //    ChangeState(EnemyMovementStates.RETURNING);
        //}
        //else
        //{
        //    ChangeState(EnemyMovementStates.CHASING);
        //}
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public void UnsetTarget()
    {
        target = null;
    }

    private void ChangeState(EnemyMovementStates state)
    {
        if (currentState == state)
        {
            return;
        }

        switch (state)
        {
            case EnemyMovementStates.PATROL:
                ResetHorizVelocity();
                speedModifier = 0f;
                enemyAnimator.SetTrigger("Patrol");

                currentState = EnemyMovementStates.PATROL;
                break;

            case EnemyMovementStates.RETURNING:
                speedModifier = 2f;
                enemyAnimator.SetTrigger("Walk");

                currentState = EnemyMovementStates.RETURNING;
                break;

            case EnemyMovementStates.CHASING:
                enemyAnimator.SetTrigger("Run");
                speedModifier = 4f;

                currentState = EnemyMovementStates.CHASING;
                break;

            case EnemyMovementStates.COMBAT:
                ResetHorizVelocity();
                enemyAnimator.SetTrigger("Combat");
                speedModifier = 0f;

                currentState = EnemyMovementStates.COMBAT;
                break;

            default:
                break;
        }

    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }

        
        else
        {
            Move();
        }
    }

    private void Move()
    {
        Vector3 movementDirection = target.position - transform.position;

        if (movementDirection.magnitude < 2f)
        {
            if (target == home)
            {
                UnsetTarget();
                transform.rotation = home.rotation;
            }
            else
            {
                ChangeState(EnemyMovementStates.COMBAT);
            }
        }
        else if (target != home)
        {
            ChangeState(EnemyMovementStates.CHASING);
        }

        movementDirection.Normalize();

        transform.LookAt(target);

        float movementSpeed = baseSpeed * speedModifier;

        movementDirection.y = 0f;

        rb.AddForce(movementDirection * movementSpeed - GetHorizVelocity(),
                                        ForceMode.VelocityChange);
    }

    private Vector3 GetHorizVelocity()
    {
        Vector3 currentHV = rb.velocity;

        currentHV.y = 0f;
        return currentHV;
    }

    private void ResetHorizVelocity()
    {
        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
    }

    public void Jump()
    {
        Vector3 jumpForce = new Vector3(0f, jumpHeight, 0f);
        rb.AddForce(jumpForce, ForceMode.VelocityChange);
    }

    public void SetWalk(bool shouldWalk)
    {
        this.shouldWalk = shouldWalk;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            SetTarget(other.transform);
            ChangeState(EnemyMovementStates.CHASING);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            UnsetTarget();
            SetTarget(home);
            ChangeState(EnemyMovementStates.RETURNING);
        }
    }
}
