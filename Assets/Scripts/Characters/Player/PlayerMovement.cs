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

    private float currentTargetRotation;
    private float timeToReachTargetRotation;
    private float dampedTargetRotationCurrentVelocity;
    private float dampedTargetRotationPassedTime;
    private float angleToRotatePlayer;

    private float baseSpeed = 5f;
    private float currentSpeed = 0f;
    private float speedModifier = 0f;

    private bool isAimLocked = false;

    public CinemachineVirtualCamera aimLockCamera;

    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<PlayerInput>();
        playerAnimator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        AddInputActionsCallbacks();

        timeToReachTargetRotation = 0.14f;
        speedModifier = 0.8f;
        mainCameraTransform = Camera.main.transform;
        
    }

    private void OnDestroy()
    {
        RemoveInputActionsCallbacks();
    }

    // Update is called once per frame
    void Update()
    {
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
        Move();
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

        Debug.Log(currentPlayerHV);
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

    private void AddInputActionsCallbacks()
    {
        input.playerActions.AimLock.started += OnAimLockStarted;
    }

    private void OnAimLockStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isAimLocked = !isAimLocked;
        playerAnimator.SetBool("IsAiming", isAimLocked);
        if (isAimLocked)
        {
            aimLockCamera.Priority = 20;
            timeToReachTargetRotation = 0.01f;
            speedModifier = 0.4f;
        }
        else
        {
            aimLockCamera.Priority = -10;
            timeToReachTargetRotation = 0.14f;
            speedModifier = 0.8f;
        }
    }

    private void RemoveInputActionsCallbacks()
    {
        input.playerActions.AimLock.started -= OnAimLockStarted;
    }
}
