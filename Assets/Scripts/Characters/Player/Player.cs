using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    enum PlayerMovementStates
    {
        IDLE,
        WALKING,
        RUNNING
    }


    // Private variables
    private PlayerMovementStates currentState;
    private Vector2 movementInput;

    private float baseSpeed;
    private float speedModifier;

    private float currentTargetRotation;
    private float timeToReachTargetRotation;
    private float dampedTargetRotationCurrentVelocity;
    private float dampedTargetRotationPassedTime;

    private bool shouldWalk;


    // Public Variables
    [HideInInspector] public PlayerInput input;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Transform mainCameraTransform;

    public PlayerData playerData;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInput>();

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
        movementInput = input.playerActions.Movement.ReadValue<Vector2>();

        if (movementInput != Vector2.zero)
        {
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
            ChangeState(PlayerMovementStates.IDLE);
        }
    }

    private void ChangeState(PlayerMovementStates state)
    {
        switch(state)
        {
            case PlayerMovementStates.IDLE:
                ResetVelocity();
                speedModifier = 0f;

                currentState = PlayerMovementStates.IDLE;
                break;

            case PlayerMovementStates.WALKING:
                speedModifier = playerData.walkSpeedModifier;

                currentState = PlayerMovementStates.WALKING;
                break;

            case PlayerMovementStates.RUNNING:
                speedModifier = playerData.runSpeedModifier;

                currentState = PlayerMovementStates.RUNNING;
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

        // Salva a velocidade horizontal do player
        Vector3 currentPlayerHV = rb.velocity;

        currentPlayerHV.y = 0f;
        //

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

    private void ResetVelocity()
    {
        rb.velocity = Vector3.zero;
    }


    private void AddInputActionsCallbacks()
    {
        input.playerActions.WalkToggle.started += OnWalkToggleStarted;
    }

    private void RemoveInputActionsCallbacks()
    {
        input.playerActions.WalkToggle.started -= OnWalkToggleStarted;
    }

    private void OnWalkToggleStarted(InputAction.CallbackContext context)
    {
        shouldWalk = !shouldWalk;
    }
}
