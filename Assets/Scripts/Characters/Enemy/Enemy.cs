using System.Collections;
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


    // Note(Nicole): Colocar no SO?
    private struct AttackType
    {
        public int BaseDamage;
        public float AnimationTime;
    };

    AttackType baseAttack;

    // Private variables
    private EnemyMovementStates currentState;
    private Animator enemyAnimator;
    private Transform target;

    private float baseSpeed;
    private float speedModifier;

    private bool isDisengaging;
    private bool isAttacking;
    private bool canSeePlayer;


    // Public Variables
    [HideInInspector] public PlayerInput input;
    [HideInInspector] public Rigidbody rb;

    public Transform home;
    public Transform attackOrigin;
    public LayerMask checkPlayerLM;

    // TODO(Nicole): Colocar no SO do inimigo
    public float walkingSpeed = 2f;
    public float runningSpeed = 4f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enemyAnimator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        ChangeState(EnemyMovementStates.PATROL);

        baseSpeed = 1f;

        //AttackType baseAttack = new AttackType();
        baseAttack.BaseDamage = 5;
        baseAttack.AnimationTime = 1f;
    }

    private void Update()
    {
        //Debug.Log(currentState);

        if (isAttacking)
            return;

        if (target == null)
        {
            ChangeState(EnemyMovementStates.PATROL);
            return;
        }
    }

    IEnumerator Attack(AttackType type)
    {
        yield return new WaitForSeconds(type.AnimationTime);
        RaycastHit hit;
        Debug.DrawRay(attackOrigin.position, transform.forward * 2f, Color.red);
        if(Physics.Raycast(attackOrigin.position, transform.forward, out hit, 2f, checkPlayerLM))
        {
            Debug.Log($"Hit {hit.transform.name} for {baseAttack.BaseDamage} damage");
        }

        isAttacking = false;
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
        if (target == null || isAttacking)
        {
            return;
        }

        Move();
    }

    private void Move()
    {
        Vector3 movementDirection = target.position - transform.position;

        if (movementDirection.magnitude < 1f)
        {
            if (target == home)
            {
                UnsetTarget();
                transform.rotation = home.rotation;
            }
            else
            {
                ChangeState(EnemyMovementStates.COMBAT);

                isAttacking = true;
                StartCoroutine(Attack(ChooseAttack()));
            }
        }
        else if (target != home)
        {
            ChangeState(EnemyMovementStates.CHASING);
        }

        movementDirection.Normalize();
        movementDirection.y = 0f;

        transform.rotation = Quaternion.LookRotation(movementDirection);

        float movementSpeed = baseSpeed * speedModifier;

        rb.AddForce(movementDirection * movementSpeed - GetHorizVelocity(),
                                        ForceMode.VelocityChange);
    }

    private AttackType ChooseAttack()
    {
        // Some logic to choose an attack
        return baseAttack;
    }

    IEnumerator ReleaseTarget()
    {
        Debug.Log("Trying to release");
        yield return new WaitForSeconds(3f);
        if (!canSeePlayer)
        {
            Debug.Log("Released");
            SetTargetHome();
        }
        Debug.Log("Couldn't release");
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            canSeePlayer = true;
            if(target != other.transform)
                SetTargetChase(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            canSeePlayer = false;
            StartCoroutine(ReleaseTarget());
        }
    }

    //// Public interface ////
    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public void SetTargetChase(Transform target)
    {
        SetTarget(target);
        ChangeState(EnemyMovementStates.CHASING);
    }

    public void SetTargetHome()
    {
        SetTarget(home);
        ChangeState(EnemyMovementStates.RETURNING);
    }

    public void UnsetTarget()
    {
        target = null;
    }
}
