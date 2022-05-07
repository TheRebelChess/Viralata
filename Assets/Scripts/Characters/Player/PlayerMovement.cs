using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Animator playerAnimator;

    private Vector2 movementInput;
    private PlayerInput input;
    private Rigidbody rb;
    private Transform mainCameraTransform;

    private List<GameObject> itemsInRage;

    private float currentTargetRotation;
    private float timeToReachTargetRotation;
    private float dampedTargetRotationCurrentVelocity;
    private float dampedTargetRotationPassedTime;
    private float angleToRotatePlayer;

    private float baseSpeed = 5f;
    private float runSpeedModifier = 0.8f;
    private float rollSpeedModifier = 1f;
    private float targetingSpeedModifier = 0.4f;
    private float currentSpeed = 0f;
    private float speedModifier = 0f;
    private float previousSpeed = 0f;

    private float attackTimer = 0f;
    private float heavyAttackDelay = .5f;
    private float maxAttackDelay = 1f;
    private float attackDamage = 0f;


    private bool isAimLocked = false;
    private bool isRolling = false;
    private bool isPreparingAttack = false;
    private bool isAttacking = false;
    private bool isBlocking = true;

    public CinemachineVirtualCamera aimLockCamera;
    public Transform attackOrigin;
    public LayerMask hitableMask;

    public QuestSystem questSystem;

    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<PlayerInput>();
        playerAnimator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        itemsInRage = new List<GameObject>();

        AddInputActionsCallbacks();

        timeToReachTargetRotation = 0.14f;
        speedModifier = runSpeedModifier;
        mainCameraTransform = Camera.main.transform;
        
    }

    private void OnDestroy()
    {
        RemoveInputActionsCallbacks();
    }

    // Update is called once per frame
    void Update()
    {
        if (isRolling || isAttacking )
        {
            return;
        }

        if (isPreparingAttack)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer > maxAttackDelay)
            {
                attackTimer = 0;
                isPreparingAttack = false;
                isAttacking = true;
                attackDamage = 5f;
                ResetHorizVel();
                playerAnimator.SetTrigger("Attack");
                playerAnimator.SetFloat("AttackForce", 1);
                StartCoroutine(AttackTimer(1f));

                return;
            }

        }

        movementInput = input.playerActions.Movement.ReadValue<Vector2>();

        currentSpeed = Mathf.Min(movementInput.magnitude, speedModifier);

        if (isAimLocked)
        {
            playerAnimator.SetFloat("Velocity X", movementInput.x);
            playerAnimator.SetFloat("Velocity Z", movementInput.y);
        }
        else
        {
            playerAnimator.SetFloat("Speed", currentSpeed);
        } 
        
    }

    private void FixedUpdate()
    {
        if (isRolling)
        {
            Roll();
            return;
        }

        if (isAttacking)
        {
            return;
        }

        Move();
    }

    private void ResetHorizVel()
    {
        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
    }

    private void Roll()
    {

        Vector3 targetRotationDirection = Quaternion.Euler(0f, rb.rotation.eulerAngles.y, 0f) * Vector3.forward;

        float movementSpeed = baseSpeed * currentSpeed;

        Vector3 currentPlayerHV = rb.velocity;
        currentPlayerHV.y = 0f;

        // Subtrai a velocidade horizontal do player pra
        // não somar os vetores em cada AddForce
        rb.AddForce(targetRotationDirection * movementSpeed - currentPlayerHV,
                                        ForceMode.VelocityChange);
    }

    private void Move()
    {
        Vector3 movementDirection = new Vector3(movementInput.x, 0f, movementInput.y);


        // Faz o player se mover em relação à camera
        float targetRotationYAngle = Rotate(movementDirection);

        Vector3 targetRotationDirection = Quaternion.Euler(0f, targetRotationYAngle, 0f) * Vector3.forward;
        //

        float movementSpeed = baseSpeed * currentSpeed;

        Vector3 currentPlayerHV = rb.velocity;
        currentPlayerHV.y = 0f;

        // Subtrai a velocidade horizontal do player pra
        // não somar os vetores em cada AddForce
        rb.AddForce(targetRotationDirection * movementSpeed - currentPlayerHV,
                                        ForceMode.VelocityChange);

    }

    private float Rotate(Vector3 direction)
    {

        // Note(Nicole): parametros invertidos na Atan para ajustar as coordenadas
        float directionAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        if (direction.magnitude == 0f)
        {
            directionAngle = angleToRotatePlayer;
        }
        else
        {
            angleToRotatePlayer = directionAngle;
        }

        // Converte angulos negativos pra positivos
        if (directionAngle < 0f)
        {
            directionAngle += 360f;
        }

        if (isAimLocked || direction.magnitude != 0)
        {
            directionAngle += mainCameraTransform.eulerAngles.y;
            angleToRotatePlayer = directionAngle;
        }

        // Evita que a rotação tenha angulos maiores que 360 graus
        if (directionAngle > 360f)
        {
            directionAngle -= 360f;
        }


        // Atualiza o target rotation
        if (directionAngle != currentTargetRotation)
        {
            if (!isAimLocked)
                currentTargetRotation = directionAngle;
            else
                currentTargetRotation = mainCameraTransform.eulerAngles.y;
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

            if (isAimLocked)
            {
                targetRotation = Quaternion.Euler(0f, currentTargetRotation, 0f);
            }

            rb.MoveRotation(targetRotation);
        }
        //

        return directionAngle;
    }


    private void Attack()
    {
        RaycastHit hit;
        Debug.DrawRay(attackOrigin.position, transform.forward * 2f, Color.red);
        if (Physics.Raycast(attackOrigin.position, transform.forward, out hit, 2f, hitableMask))
        {
            // TODO(Nicole): Melhorar isso
            if (hit.transform.tag == "Enemy")
            {
                bool hasKilled = false;
                hasKilled = hit.transform.GetComponent<Enemy>().TakeHit(attackDamage);
                if (hasKilled)
                    questSystem.OnPlayerKill(hit.transform.gameObject);
            }
        }
    }


    private void AddInputActionsCallbacks()
    {
        input.playerActions.AimLock.started += OnAimLockStarted;
        input.playerActions.Jump.started += OnRollStarted;
        input.playerActions.Interact.started += OnInteractStarted;

        input.playerActions.Attack1.started += OnAttackStarted;
        input.playerActions.Attack1.canceled += OnAttackReleased;

        input.playerActions.Block.started += OnBlockPressed;
        input.playerActions.Block.canceled += OnBlockReleased;
    }

    private void OnInteractStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        // Se tiver itens ao alcance, os coleta
        if (itemsInRage.Count > 0)
        {
            GameObject gameObject = itemsInRage[itemsInRage.Count - 1];
            itemsInRage.RemoveAt(itemsInRage.Count - 1);
            questSystem.OnPlayerCollect(gameObject);
            Destroy(gameObject);
            return;
        }

        // Se não, tenta conversar com algum NPC
        RaycastHit hit;
        if (Physics.Raycast(attackOrigin.position, transform.forward, out hit, 2f, hitableMask))
        {
            if (hit.transform.tag == "NPC")
            {
                hit.transform.GetComponent<QuestGiver>()?.OnInteract();
                hit.transform.GetComponent<QuestTarget>()?.OnInteract();
            }
        }
    }

    private void OnBlockPressed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isAttacking || isRolling)
            return;

        isBlocking = true;
        playerAnimator.SetBool("isBlocking", true);
    }

    private void OnBlockReleased(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isAttacking || isRolling)
            return;

        isBlocking = false;
        playerAnimator.SetBool("isBlocking", false);
        Debug.Log(isBlocking);
    }


    private void OnAttackStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isAttacking || isRolling)
            return;

        Debug.Log("Entrou");
        isPreparingAttack = true;
    }
    private void OnAttackReleased(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        // Checagem pra ver se o ataque foi cancelado de outra forma
        if (!isPreparingAttack)
            return;

        isPreparingAttack = false;
        isAttacking = true;
        ResetHorizVel();
        playerAnimator.SetTrigger("Attack");

        if (attackTimer >= heavyAttackDelay)
        {
            playerAnimator.SetFloat("AttackForce", 1);
            attackDamage = 2f;
            StartCoroutine(AttackTimer(1f));
        }
        else
        {
            playerAnimator.SetFloat("AttackForce", 0);
            attackDamage = 1f;
            StartCoroutine(AttackTimer(.5f));
        }


        attackTimer = 0;
    }

    private void RemoveInputActionsCallbacks()
    {
        input.playerActions.AimLock.started -= OnAimLockStarted;
        input.playerActions.Jump.started -= OnRollStarted;
        input.playerActions.Interact.started -= OnInteractStarted;

        input.playerActions.Attack1.started -= OnAttackStarted;
        input.playerActions.Attack1.canceled -= OnAttackReleased;

        input.playerActions.Block.started -= OnBlockPressed;
        input.playerActions.Block.canceled -= OnBlockReleased;
    }

    private void OnAimLockStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isAimLocked = !isAimLocked;
        playerAnimator.SetBool("IsAiming", isAimLocked);
        if (isAimLocked)
        {
            aimLockCamera.Priority = 20;
            speedModifier = targetingSpeedModifier;
        }
        else
        {
            aimLockCamera.Priority = -10;
            speedModifier = runSpeedModifier;
        }
    }
    private void OnRollStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isRolling)
            return;
        isRolling = true;
        StartCoroutine(RollTimer());
        movementInput = input.playerActions.Movement.ReadValue<Vector2>();

        float directionAngle = Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg;

        // Converte angulos negativos pra positivos
        if (directionAngle < 0f)
        {
            directionAngle += 360f;
        }

        directionAngle += mainCameraTransform.eulerAngles.y;
        angleToRotatePlayer = directionAngle;


        // Evita que a rotação tenha angulos maiores que 360 graus
        if (directionAngle > 360f)
        {
            directionAngle -= 360f;
        }

        Quaternion targetRotation = Quaternion.Euler(0f, directionAngle, 0f);

        rb.MoveRotation(targetRotation);

        playerAnimator.SetTrigger("Roll");

    }

    IEnumerator RollTimer()
    {
        previousSpeed = speedModifier;
        speedModifier = rollSpeedModifier;
        currentSpeed = rollSpeedModifier;
        playerAnimator.speed = 1.3f;
        yield return new WaitForSeconds(.9f);
        playerAnimator.speed = 1f;
        isRolling = false;
        speedModifier = previousSpeed;
    }

    IEnumerator AttackTimer(float t)
    {
        yield return new WaitForSeconds(t);
        Attack();
        isAttacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Place" && this.gameObject.tag == "Player")
        {
            questSystem.OnPlayerReach(other.gameObject);
            other.GetComponent<Collider>().enabled = false;
        }

        if (other.tag == "Item")
        {
            itemsInRage.Add(other.gameObject);
            Debug.Log("Added item " + other.gameObject.name + " to list");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Item")
        {
            if (itemsInRage.Contains(other.gameObject))
            {
                itemsInRage.Remove(other.gameObject);
                Debug.Log("Remove item " + other.gameObject.name + " from list");
            }
        }
    }
}
