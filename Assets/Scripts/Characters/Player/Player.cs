using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    enum PlayerMovementStates
    {
        IDLE,
        WALKING,
        RUNNING,
        AIRBORNE,
        DEAD,
        LEAVING_COMBAT // GAMBIARRA
    }


    // Private variables
    private PlayerMovementStates currentState;
    private Vector2 movementInput;
    private Animator playerAnimator;
    private Transform attackOrigin;

    private float baseSpeed;
    private float speedModifier;

    private float currentCombatTimer;

    private float currentTargetRotation;
    private float timeToReachTargetRotation;
    private float dampedTargetRotationCurrentVelocity;
    private float dampedTargetRotationPassedTime;

    private bool shouldWalk;
    private bool isGrounded;
    private bool inCombat;
    private bool canAttack;
    private bool isAttacking;
    private bool isBlocking;


    // Public Variables
    [HideInInspector] public PlayerInput input;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Transform mainCameraTransform;

    public PlayerData playerData;
    public float jumpHeight = 5f;
    public Transform groundCheckTransform;
    public LayerMask groundCheckLayerMask;
    public float combatTimer = 5f;
    public LayerMask hitableMask;


    // TODO(Nicole): Colocar no struct de ataque
    public float attackCooldown = 2f;

    // TODO(Nicole): Colocar no struct de stats do player(?)
    public float health = 10f;
    public int currentDamage = 1;
    public int blockModifier = 2;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInput>();
        playerAnimator = GetComponentInChildren<Animator>();

        mainCameraTransform = Camera.main.transform;

        // TODO(Nicole): Fazer isso direito
        attackOrigin = transform.GetChild(1).transform;
    }

    private void Start()
    {
        AddInputActionsCallbacks();

        ChangeState(PlayerMovementStates.IDLE);

        baseSpeed = playerData.baseSpeed;
        timeToReachTargetRotation = playerData.timeToReachTargetRotation;
        shouldWalk = false;
        canAttack = true;

    }

    private void OnDestroy()
    {
        RemoveInputActionsCallbacks();
    }

    private void Update()
    {
        if (currentState == PlayerMovementStates.DEAD)
            return;

        //Debug.Log(currentState);
        movementInput = input.playerActions.Movement.ReadValue<Vector2>();
        isGrounded = Physics.CheckSphere(groundCheckTransform.position, .2f, groundCheckLayerMask);

        if (inCombat)
        {
            // Decrease combat cooldown
            currentCombatTimer -= Time.deltaTime;
            if (currentCombatTimer <= 0)
            {
                inCombat = false;
                currentState = PlayerMovementStates.LEAVING_COMBAT;
            }
        }

        if (!isGrounded)
        {
            ChangeState(PlayerMovementStates.AIRBORNE);
            return;
        }
        
        if(movementInput != Vector2.zero)
        {  
            if (currentState == PlayerMovementStates.AIRBORNE)
            {
                // Note(Nicole): Placeholder pra animação caindo em movimento
                playerAnimator.SetTrigger("Grounded");
            }
            if (shouldWalk)
            {
                ChangeState(PlayerMovementStates.WALKING);
            }
            else
            {
                ChangeState(PlayerMovementStates.RUNNING);
            }
        }
        else
        {
            if (currentState == PlayerMovementStates.AIRBORNE)
            {
                // NOTE(Nicole): Placeholder pra animação caindo parado
                playerAnimator.SetTrigger("Grounded");
            }
            ChangeState(PlayerMovementStates.IDLE);
        }
    }

    private void ChangeState(PlayerMovementStates state)
    {
        if (currentState == state)
        {
            return;
        }

        switch(state)
        {
            case PlayerMovementStates.IDLE:
                ResetHorizVelocity();
                speedModifier = 0f;

                if (inCombat)
                    playerAnimator.SetTrigger("Combat");
                else if (!isBlocking)
                    playerAnimator.SetTrigger("Idle");

                currentState = PlayerMovementStates.IDLE;
                break;

            case PlayerMovementStates.WALKING:
                speedModifier = playerData.walkSpeedModifier;

                if (!isBlocking)
                    playerAnimator.SetTrigger("Walk");

                currentState = PlayerMovementStates.WALKING;
                break;

            case PlayerMovementStates.RUNNING:
                speedModifier = playerData.runSpeedModifier;

                if (!isBlocking)
                    playerAnimator.SetTrigger("Run");

                currentState = PlayerMovementStates.RUNNING;
                break;

            case PlayerMovementStates.AIRBORNE:
                if (!isBlocking)
                    playerAnimator.SetTrigger("Airborne");

                currentState = PlayerMovementStates.AIRBORNE;
                break;

            case PlayerMovementStates.DEAD:
                speedModifier = 0f;
                playerAnimator.SetTrigger("Death");

                currentState = PlayerMovementStates.DEAD;
                break;

            default:
                break;
        }

    }

    private void FixedUpdate()
    {
        if (currentState == PlayerMovementStates.IDLE
            || currentState == PlayerMovementStates.DEAD
            || isAttacking)
        {
            return;
        }

        Move();
    }

    private void Move()
    {
        Vector3 movementDirection = new Vector3(movementInput.x, 0f, movementInput.y);

        // Faz o player se mover em relação à camera
        float targetRotationYAngle = Rotate(movementDirection);

        Vector3 targetRotationDirection = Quaternion.Euler(0f, targetRotationYAngle, 0f) * Vector3.forward;
        //

        float movementSpeed = baseSpeed * speedModifier;

        Vector3 currentPlayerHV = rb.velocity;
        currentPlayerHV.y = 0f;

        // Subtrai a velocidade horizontal do player pra
        // não somar os vetores em cada AddForce
        rb.AddForce(targetRotationDirection * movementSpeed - currentPlayerHV,
                                        ForceMode.VelocityChange);

        Debug.Log(currentPlayerHV);
    }

    private float Rotate(Vector3 direction)
    {

        // Note(Nicole): parametros invertidos na Atan para ajustar as coordenadas
        float directionAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        // Converte angulos negativos pra positivos
        if (directionAngle < 0f)
        {
            directionAngle += 360f;
        }

        directionAngle += mainCameraTransform.eulerAngles.y;

        // Evita que a rotação tenha angulos maiores que 360 graus
        if (directionAngle > 360f)
        {
            directionAngle -= 360f;
        }


        // Atualiza o target rotation
        if (directionAngle != currentTargetRotation)
        {
            currentTargetRotation = directionAngle;
            dampedTargetRotationPassedTime = 0f;
        }

        // Rotaciona em direção ao target
        float currentYAngle = rb.rotation.eulerAngles.y;

        if (currentYAngle != currentTargetRotation)
        {
            float smoothedYAngle = Mathf.SmoothDampAngle(currentYAngle,
                                                         currentTargetRotation,
                                                         ref dampedTargetRotationCurrentVelocity,
                                                         timeToReachTargetRotation - dampedTargetRotationPassedTime);

            dampedTargetRotationPassedTime += Time.fixedDeltaTime;

            Quaternion targetRotation = Quaternion.Euler(0f, smoothedYAngle, 0f);

            rb.MoveRotation(targetRotation);
        }
        //

        return directionAngle;
    }

    private void ResetHorizVelocity()
    {
        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
    }

    private void Jump()
    {
        Vector3 jumpForce = new Vector3(0f, jumpHeight, 0f);
        rb.AddForce(jumpForce, ForceMode.VelocityChange);
    }


    private void AddInputActionsCallbacks()
    {
        input.playerActions.WalkToggle.started += OnWalkToggleStarted;
        input.playerActions.Jump.started += OnJumpStarted;
        input.playerActions.Attack1.started += OnAttack1Started;
        input.playerActions.Attack2.started += OnAttack2Started;

        input.playerActions.Block.started += OnBlockPressed;
        input.playerActions.Block.canceled += OnBlockReleased;
    }
    private void OnBlockPressed(InputAction.CallbackContext obj)
    {
        if (!CanExecuteAction())
            return;

        isBlocking = true;
        playerAnimator.SetBool("isBlocking", true);
    }

    private void OnBlockReleased(InputAction.CallbackContext obj)
    {
        if (!CanExecuteAction())
            return;

        isBlocking = false;
        playerAnimator.SetBool("isBlocking", false);
    }


    private void RemoveInputActionsCallbacks()
    {
        input.playerActions.WalkToggle.started -= OnWalkToggleStarted;
        input.playerActions.Jump.started -= OnJumpStarted;
        input.playerActions.Attack1.started -= OnAttack1Started;
        input.playerActions.Attack2.started -= OnAttack2Started;

        input.playerActions.Block.started -= OnBlockPressed;
        input.playerActions.Block.canceled -= OnBlockReleased;
    }

    private void OnAttack1Started(InputAction.CallbackContext obj)
    {
        if (!CanExecuteAction())
            return;

        if (!canAttack)
            return;

        if (currentState == PlayerMovementStates.AIRBORNE)
        {
            // Ataque1 aereo
            return;
        }
        Punch();
        StartCoroutine(AttackCooldown());
    }

    private void Punch()
    {
        if (!inCombat)
        {
            inCombat = true;
        }
        playerAnimator.SetTrigger("Punch");

        RaycastHit hit;
        Debug.DrawRay(attackOrigin.position, transform.forward * 2f, Color.red);
        if (Physics.Raycast(attackOrigin.position, transform.forward, out hit, 2f, hitableMask))
        {
            // TODO(Nicole): Melhorar isso
            if (hit.transform.tag == "Enemy")
            {
                hit.transform.GetComponent<Enemy>().TakeHit(currentDamage);
                Debug.Log("Has hit");
            }
        }

        currentCombatTimer = combatTimer;
    }

    private void OnAttack2Started(InputAction.CallbackContext obj)
    {
        if (!CanExecuteAction())
            return;

        if (!canAttack)
            return;

        if (currentState == PlayerMovementStates.AIRBORNE)
        {
            // Ataque2 aereo
            return;
        }
        Kick();
        StartCoroutine(AttackCooldown());
    }

    IEnumerator AttackCooldown()
    {
        canAttack = false;
        isAttacking = true;

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
        isAttacking = false;
    }

    private void Kick()
    {
        if (!inCombat)
        {
            inCombat = true;
        }
        playerAnimator.SetTrigger("Kick");

        StartCoroutine(MultipleHits(.5f, 3, 1));
        

        currentCombatTimer = combatTimer;
    }

    IEnumerator MultipleHits(float cooldown, int numOfHits, int damage)
    {
        HitEnemy(damage);

        yield return new WaitForSeconds(cooldown);

        numOfHits--;

        if (numOfHits > 0)
        {
            StartCoroutine(MultipleHits(cooldown, numOfHits, damage));
            currentCombatTimer = combatTimer;
        }
        
    }

    private void HitEnemy(int damage)
    {
        RaycastHit hit;
        Debug.DrawRay(attackOrigin.position, transform.forward * 2f, Color.red);
        if (Physics.Raycast(attackOrigin.position, transform.forward, out hit, 2f, hitableMask))
        {
            // TODO(Nicole): Melhorar isso
            if (hit.transform.tag == "Enemy")
            {
                hit.transform.GetComponent<Enemy>().TakeHit(currentDamage);
                Debug.Log("Has hit");
            }
        }
    }

    private void OnJumpStarted(InputAction.CallbackContext obj)
    {
        if (!CanExecuteAction())
            return;

        Jump();
    }

    private void OnWalkToggleStarted(InputAction.CallbackContext context)
    {
        shouldWalk = !shouldWalk;
    }

    public void Hit(int damage)
    {
        if (isBlocking)
        {
            damage -= blockModifier;
        }
        if (damage <= 0)
            return;

        health -= damage;

        if (health <= 0)
        {
            ChangeState(PlayerMovementStates.DEAD);
        }
    }

    private bool CanExecuteAction()
    {
        return !(currentState == PlayerMovementStates.DEAD
            || isAttacking);
    }
}
