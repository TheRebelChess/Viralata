using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [HideInInspector]
    public Animator playerAnimator;

    private PlayerInteraction playerInteraction;

    private Vector2 movementInput;
    private PlayerInput input;
    private Rigidbody rb;
    private Transform mainCameraTransform;

    private List<GameObject> itemsInRage;

    public BoxCollider swordTrigger;
    public BoxCollider daggerTrigger1;
    public BoxCollider daggerTrigger2;
    public Image healthFill;

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
    private int attackDamage = 0;

    private PlayerHealth playerHealthScript;
    private float health = 10f;


    private bool isAimLocked = false;
    private bool isRolling = false;
    private bool isPreparingAttack = false;
    private bool isAttacking = false;
    private bool isBlocking = false;
    private bool canAttack = true;
    private bool isInvincible = false;
    private bool inventoryActive = false;

    public int playerLevel = 1;
    public int currentXp = 0;
    [SerializeField]
    private int[] xpRequiredByLv;

    public int strength = 1;
    public float maxHealth = 10f;
    public float percHealth;

    public float invincibilityTime = 1f;
    
    public CinemachineVirtualCamera aimLockCamera;
    public CinemachineVirtualCamera playerCamera;

    public Transform attackOrigin;
    public GameObject shot;
    public LayerMask hitableMask;
    public LayerMask NPC;

    public QuestSystem questSystem;
    public GameManager gameManager;
    public Inventory inventoryScript;

    public AudioSource audioSource;
    public AudioClip lightAttackSFX;
    public AudioClip heavyAttackSFX;
    public AudioClip blockSFX;
    public AudioClip deathSFX;

    [Header("NPC Interaction")]
    public GameObject pressToInteract;

    [HideInInspector]
    public int playerIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<PlayerInput>();
        playerAnimator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        playerHealthScript = GetComponent<PlayerHealth>();
        playerInteraction = GetComponent<PlayerInteraction>();

        itemsInRage = new List<GameObject>();

        AddInputActionsCallbacks();

        timeToReachTargetRotation = 0.14f;
        speedModifier = runSpeedModifier;
        mainCameraTransform = Camera.main.transform;

        swordTrigger.enabled = false;
        daggerTrigger1.enabled = false;
        daggerTrigger2.enabled = false;

        shot.SetActive(false);
    }

    private void OnDestroy()
    {
        RemoveInputActionsCallbacks();
    }

    private void OnDisable()
    {
        RemoveInputActionsCallbacks();
    }

    private void OnEnable()
    {
       AddInputActionsCallbacks();
       gameManager.inPlayerInteraction = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRolling || isAttacking || isBlocking || inventoryActive)
        {
            return;
        }

        if (playerIndex == 0)
        {
            if (isPreparingAttack)
            {
                attackTimer += Time.deltaTime;

                if (attackTimer > maxAttackDelay)
                {
                    attackTimer = 0;
                    isPreparingAttack = false;
                    isAttacking = true;
                    attackDamage = 5;
                    ResetHorizVel();
                    playerAnimator.SetTrigger("heavyAttack");
                    StartCoroutine(AttackTimer(1f));

                    return;
                }

            }
        }

        movementInput = input.playerActions.Movement.ReadValue<Vector2>();

        currentSpeed = Mathf.Min(movementInput.magnitude, speedModifier);

        if (currentSpeed != 0)
        {
            playerAnimator.SetBool("move", true);
        }
        else
        {
            playerAnimator.SetBool("move", false);
        }

        PressToTalk();


        percHealth = (float)health / (float)maxHealth;

        if(percHealth < 0)
        {
            percHealth = 0;
        }

        healthFill.fillAmount = percHealth;


    }

    private void FixedUpdate()
    {
        if (isAttacking || isBlocking || inventoryActive)
        {
            ResetHorizVel();
            return;
        }

        if (isRolling)
        {
            Roll();
            return;
        }

        Move();
    }

    private void ResetHorizVel()
    {
        playerAnimator.SetBool("move", false);
        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
    }

    private void Roll()
    {

        Vector3 targetRotationDirection = Quaternion.Euler(0f, rb.rotation.eulerAngles.y, 0f) * Vector3.forward;

        float movementSpeed = baseSpeed * currentSpeed;

        Vector3 currentPlayerHV = rb.velocity;
        currentPlayerHV.y = 0f;

        // Subtrai a velocidade horizontal do player pra
        // n�o somar os vetores em cada AddForce
        rb.AddForce(targetRotationDirection * movementSpeed - currentPlayerHV,
                                        ForceMode.VelocityChange);
    }

    private void Move()
    {
        Vector3 movementDirection = new Vector3(movementInput.x, 0f, movementInput.y);


        // Faz o player se mover em rela��o � camera
        float targetRotationYAngle = Rotate(movementDirection);

        Vector3 targetRotationDirection = Quaternion.Euler(0f, targetRotationYAngle, 0f) * Vector3.forward;
        //

        float movementSpeed = baseSpeed * currentSpeed;

        Vector3 currentPlayerHV = rb.velocity;
        currentPlayerHV.y = 0f;

        // Subtrai a velocidade horizontal do player pra
        // n�o somar os vetores em cada AddForce
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

        // Evita que a rota��o tenha angulos maiores que 360 graus
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

        // Rotaciona em dire��o ao target
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

    public void EnemyHit(GameObject enemy)
    {
        if (playerIndex == 0)
        {
            swordTrigger.enabled = false;
        }
        else if (playerIndex == 2)
        {
            daggerTrigger1.enabled = false;
            daggerTrigger2.enabled = false;
        }

        bool hasKilled = enemy.GetComponent<EnemyMovement>().TakeHit(attackDamage * strength);

        if (hasKilled)
        {
            questSystem.OnPlayerKill(enemy);
            GainXP(1);
        }
    }

    private void Attack()
    {

    }

    private void AttackRayCast()
    {
        RaycastHit hit;
        Debug.DrawRay(attackOrigin.position, transform.forward * 2f, Color.red);
        if (Physics.Raycast(attackOrigin.position, transform.forward, out hit, 2f, hitableMask))
        {
            // TODO(Nicole): Melhorar isso
            if (hit.transform.tag == "Enemy")
            {
                bool hasKilled = false;
                hasKilled = hit.transform.GetComponent<Enemy>().TakeHit(attackDamage * strength);
                if (hasKilled)
                {
                    questSystem.OnPlayerKill(hit.transform.gameObject);
                    GainXP(1);
                }

            }
        }
    }

    public void GainXP(int xp)
    {
        currentXp += xp;
        if (currentXp >= xpRequiredByLv[playerLevel - 1])
        {
            LevelUp();
            currentXp = 0;
        }
    }

    private void LevelUp()
    {
        playerLevel++;
        strength++;
    }

    private void AddInputActionsCallbacks()
    {
        //input.playerActions.AimLock.started += OnAimLockStarted;
        input.playerActions.Jump.started += OnRollStarted;
        input.playerActions.Interact.started += OnInteractStarted;
        input.playerActions.Inventory.started += OnInventoryStarted;

        input.playerActions.Attack1.started += OnAttackStarted;
        input.playerActions.Attack1.canceled += OnAttackReleased;

        input.playerActions.Block.started += OnBlockPressed;
        input.playerActions.Block.canceled += OnBlockReleased;
    }

    private void OnInventoryStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (inventoryScript.gameObject.activeSelf)
        {
            inventoryScript.gameObject.SetActive(false);
            playerCamera.GetComponent<CinemachineInputProvider>().enabled = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            canAttack = true;
            inventoryActive = false;
        }
        else
        {
            inventoryScript.gameObject.SetActive(true);
            playerCamera.GetComponent<CinemachineInputProvider>().enabled = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            canAttack = false;
            inventoryActive = true;
            ResetHorizVel();
        }
    }

    private void OnInteractStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        // Se tiver itens ao alcance, os coleta
        if (itemsInRage.Count > 0)
        {
            GameObject gameObject = itemsInRage[itemsInRage.Count - 1];
            itemsInRage.RemoveAt(itemsInRage.Count - 1);
            questSystem.OnPlayerCollect(gameObject);
            inventoryScript.AddItem(gameObject.GetComponent<ItemController>().item);

            Destroy(gameObject);
            return;
        }

        // Se n�o, tenta conversar com algum NPC
        RaycastHit hit;
        Debug.Log("entrou");
        if (Physics.Raycast(attackOrigin.position, transform.forward, out hit, 2f, hitableMask))
        {
            

            Debug.Log("acertou");
            if (hit.transform.tag == "NPC")
            {

                pressToInteract.SetActive(false);

                hit.transform.GetComponent<QuestTarget>()?.OnInteract();

                if (hit.transform.GetComponent<QuestGiver>())
                {
                    ActivateInteraction(hit.transform.gameObject);
                }
            }

          
        }    

    }

    void PressToTalk()
    {
        RaycastHit hit;
        if (Physics.Raycast(attackOrigin.position, transform.forward, out hit, 2f, hitableMask))
        {
            if (hit.transform.tag == "NPC")
            {
                pressToInteract.SetActive(true);
            }
        }

        else
        {
            pressToInteract.SetActive(false);
        }
    }

    public void ActivateInteraction(GameObject interactableObj)
    {
      
        ResetHorizVel();
        interactableObj.GetComponent<IInteractable>().OnInteract();

        AimAt(interactableObj.transform);

        playerInteraction.enabled = true;
        gameManager.inPlayerInteraction = true;
        playerInteraction.SetInteractableObj(interactableObj.transform.gameObject);

        this.enabled = false;
    }

    private void OnBlockPressed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isAttacking || isRolling || (playerIndex == 1) || (playerIndex == 2))
            return;

        isBlocking = true;
        playerAnimator.SetBool("block", true);
        playerAnimator.SetBool("move", false);
    }

    private void OnBlockReleased(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isAttacking || isRolling)
            return;

        isBlocking = false;
        playerAnimator.SetBool("block", false);
        Debug.Log(isBlocking);
    }


    private void OnAttackStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isAttacking || isRolling || !canAttack)
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
        //ResetHorizVel();

        if (playerIndex == 0)
        {
            if (attackTimer >= heavyAttackDelay)
            {
                playerAnimator.SetTrigger("heavyAttack");
                attackDamage = 5;
                StartCoroutine(AttackTimer(1f));
            }
            else
            {
                playerAnimator.SetTrigger("attack");
                attackDamage = 3;
                StartCoroutine(AttackTimer(.5f));
            }
        }

        else if(playerIndex == 1)
        {
            playerAnimator.SetTrigger("attack");
            attackDamage = 5;
            StartCoroutine(AttackTimer(.5f));
        }
    
        else if(playerIndex == 2)
        {
            playerAnimator.SetTrigger("attack");
            attackDamage = 1;
            StartCoroutine(AttackTimer(.3f));
        }


        attackTimer = 0;
    }

    private void RemoveInputActionsCallbacks()
    {
        //input.playerActions.AimLock.started -= OnAimLockStarted;
        input.playerActions.Jump.started -= OnRollStarted;
        input.playerActions.Interact.started -= OnInteractStarted;
        input.playerActions.Inventory.started -= OnInventoryStarted;

        input.playerActions.Attack1.started -= OnAttackStarted;
        input.playerActions.Attack1.canceled -= OnAttackReleased;

        input.playerActions.Block.started -= OnBlockPressed;
        input.playerActions.Block.canceled -= OnBlockReleased;
    }

    //private void OnAimLockStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    //{
    //    AimAt(null);
    //    playerAnimator.SetBool("IsAiming", isAimLocked);
    //}

    public void AimAt(Transform target)
    {
        isAimLocked = !isAimLocked;

        if (isAimLocked)
        {

            aimLockCamera.Priority = 20;

            if (target == null)
            {
                speedModifier = targetingSpeedModifier;
            }
            else
            {
                aimLockCamera.m_LookAt = target;
            }
        }
        else
        {
            aimLockCamera.Priority = -10;
            aimLockCamera.m_LookAt = null;
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


        // Evita que a rota��o tenha angulos maiores que 360 graus
        if (directionAngle > 360f)
        {
            directionAngle -= 360f;
        }

        Quaternion targetRotation = Quaternion.Euler(0f, directionAngle, 0f);

        rb.MoveRotation(targetRotation);

        playerAnimator.SetTrigger("roll");

    }

    IEnumerator RollTimer()
    {
        previousSpeed = speedModifier;
        speedModifier = rollSpeedModifier;
        currentSpeed = rollSpeedModifier;
        playerAnimator.speed = 1.3f;
        switch (playerIndex)
        {
            case 0:
                yield return new WaitForSeconds(.9f);
                break;
            case 1:
                yield return new WaitForSeconds(.6f);
                break;
            case 2:
                yield return new WaitForSeconds(.4f);
                break;
            default:
                yield return new WaitForSeconds(.9f);
                break;
        }
        playerAnimator.speed = 1f;
        isRolling = false;
        speedModifier = previousSpeed;
    }

    IEnumerator AttackTimer(float t)
    {
        if (playerIndex == 1)
        {
            shot.SetActive(true);
        }
        yield return new WaitForSeconds(t);
        if (playerIndex == 0)
            swordTrigger.enabled = true;
        else if (playerIndex == 2)
        {
            daggerTrigger1.enabled = true;
            daggerTrigger2.enabled = true;
        }
        if (attackDamage == 5f)
        {

            audioSource.PlayOneShot(heavyAttackSFX);
        }
        else
        {
            audioSource.PlayOneShot(lightAttackSFX);
        }
        Attack();

        switch (playerIndex)
        {
            case 0:
                yield return new WaitForSeconds(.5f);
                break;
            case 1:
                yield return new WaitForSeconds(.1f);
                shot.SetActive(false);
                break;
            case 2:
                yield return new WaitForSeconds(.4f);
                break;
            default:
                yield return new WaitForSeconds(.5f);
                break;
        }
        isAttacking = false;
        if (playerIndex == 0)
            swordTrigger.enabled = false;
        if (playerIndex == 2)
        {
            daggerTrigger1.enabled = false;
            daggerTrigger2.enabled = false;
        }
    }

    public void TakeHit(int damage)
    {
        if (isInvincible || isBlocking)
        {
            return;
        }
        StartCoroutine(InvincibilityTimer(invincibilityTime));

        health -= damage;

        if (health < 0)
            health = 0;

        playerHealthScript.UpdateHealth(health);

        if (health == 0)
        {
            audioSource.PlayOneShot(deathSFX);
            playerAnimator.SetTrigger("death");
            GetComponent<PlayerDeath>().enabled = true;
            GetComponent<CapsuleCollider>().enabled = false;
            rb.useGravity = false;
            this.enabled = false;
        }

    }

    IEnumerator InvincibilityTimer(float t)
    {
        isInvincible = true;
        yield return new WaitForSeconds(t);
        isInvincible = false;
    }

    public void IncreaseHealth(int amount)
    {
        health += amount;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        playerHealthScript.UpdateHealth(health);
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
