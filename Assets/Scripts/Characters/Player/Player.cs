using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    enum PlayerMovementStates
    {
        IDLE,
        WALKING,
        RUNNING,
        AIRBORNE
    }


    // Private variables
    private PlayerMovementStates currentState;
    private Vector2 movementInput;
    private Animator playerAnimator;

    private float baseSpeed;
    private float speedModifier;

    private float currentTargetRotation;
    private float timeToReachTargetRotation;
    private float dampedTargetRotationCurrentVelocity;
    private float dampedTargetRotationPassedTime;

    private bool shouldWalk;
    private bool isGrounded;


    // Public Variables
    [HideInInspector] public PlayerInput input;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Transform mainCameraTransform;

    public PlayerData playerData;
    public float jumpHeight = 5f;
    public Transform groundCheckTransform;
    public LayerMask groundCheckLayerMask;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInput>();
        playerAnimator = GetComponentInChildren<Animator>();

        mainCameraTransform = Camera.main.transform;
    }

    private void Start()
    {
        AddInputActionsCallbacks();

        ChangeState(PlayerMovementStates.IDLE);

        baseSpeed = playerData.baseSpeed;
        timeToReachTargetRotation = playerData.timeToReachTargetRotation;
        shouldWalk = false;
    }

    private void OnDestroy()
    {
        RemoveInputActionsCallbacks();
    }

    private void Update()
    {
        //Debug.Log(currentState);
        movementInput = input.playerActions.Movement.ReadValue<Vector2>();
        isGrounded = Physics.CheckSphere(groundCheckTransform.position, .2f, groundCheckLayerMask);

        if (!isGrounded)
        {
            ChangeState(PlayerMovementStates.AIRBORNE);
        }
        else if(movementInput != Vector2.zero)
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
                playerAnimator.SetTrigger("Idle");

                currentState = PlayerMovementStates.IDLE;
                break;

            case PlayerMovementStates.WALKING:
                speedModifier = playerData.walkSpeedModifier;
                playerAnimator.SetTrigger("Walk");

                currentState = PlayerMovementStates.WALKING;
                break;

            case PlayerMovementStates.RUNNING:
                speedModifier = playerData.runSpeedModifier;
                playerAnimator.SetTrigger("Run");

                currentState = PlayerMovementStates.RUNNING;
                break;

            case PlayerMovementStates.AIRBORNE:
                playerAnimator.SetTrigger("Airborne");

                currentState = PlayerMovementStates.AIRBORNE;
                break;

            default:
                break;
        }

    }

    private void FixedUpdate()
    {
        if (currentState == PlayerMovementStates.IDLE)
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
    }

    private void OnJumpStarted(InputAction.CallbackContext obj)
    {   
        if (currentState == PlayerMovementStates.AIRBORNE)
        {
            return;
        }
        Jump();
    }

    private void RemoveInputActionsCallbacks()
    {
        input.playerActions.WalkToggle.started -= OnWalkToggleStarted;
        input.playerActions.Jump.started -= OnJumpStarted;
    }

    private void OnWalkToggleStarted(InputAction.CallbackContext context)
    {
        shouldWalk = !shouldWalk;
    }
}
