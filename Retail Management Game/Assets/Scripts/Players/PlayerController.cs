using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Adjustments")]
    [SerializeField] float playerSpeed = 10.0f;
    [Range(0.0f, 1.0f)]
    [SerializeField] float playerRotationSpeed = 1.0f;
    [Range(45.0f, 180.0f)]
    [SerializeField] float playerPickupAngle = 90.0f;
    [SerializeField] int playerDoubleTapMilliseconds = 100;

    [Space]
    [Header("Control Map")]
    [SerializeField] InputAction movement;
    [SerializeField] InputAction pickup;

    [Space]
    [Header("Links")]
    [SerializeField] Camera currentCamera;
    [SerializeField] CharacterController characterController;
    [SerializeField] Transform equippedPosition;
    GameObject equippedItem;

    // Internal Variables
    Vector3 playerDirection = Vector3.zero;
    float dashTapTimer = 0.0f;
    int dashTapCount = 0;

    private void Awake()
    {
        GameplayControls gameplayControls = new GameplayControls();

        // Attach all controls
        movement = gameplayControls.Default.Movement;
        movement.performed += OnMovementChanged;
        movement.canceled += OnMovementChanged;

        pickup = gameplayControls.Default.Pickup;
        pickup.performed += OnPickupChanged;
    }

    private void OnEnable()
    {
        movement.Enable();
    }
    private void OnDisable()
    {
        movement.Disable();
    }

    #region OnChanged Events

    private void OnMovementChanged(InputAction.CallbackContext context)
    {
        Vector2 movementVector = movement.ReadValue<Vector2>();

        // Parse movement data
        float directionX = movementVector.x;
        float directionY = movementVector.y;

        // Check if input device is the keyboard
        if (context.control.device.name == "Keyboard")
        {
            // Detect if change is not cancelled
            if (movementVector.magnitude > 0.0f)
            {
                if (dashTapTimer > 0.0f)
                {
                    dashTapCount++;
                }

                dashTapTimer = playerDoubleTapMilliseconds / 1000.0f;
            }
        }

        playerDirection = new Vector3(directionX, 0.0f, directionY);
    }

    private void OnPickupChanged(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        // Decrease dash timer for multi tap detection
        if (dashTapTimer > 0.0f)
        {
            dashTapTimer -= Time.deltaTime;
        }
        else if (dashTapTimer < 0.0f)
        {
            dashTapTimer = 0.0f;
            dashTapCount = 0;
        }

        UpdatePlayer();
    }

    /// <summary>
    /// Update player movement
    /// </summary>
    private void UpdatePlayer()
    {
        // Calculate relative camera direction
        Vector3 relativeDirection = currentCamera.transform.TransformVector(playerDirection);
        relativeDirection.Scale(Vector3.forward + Vector3.right);

        // Ease rotation
        Vector3 rotationResult = Vector3.Lerp(
            characterController.transform.position + characterController.transform.forward,
            characterController.transform.position + relativeDirection,
            playerRotationSpeed
        );

        // Rotate player
        characterController.transform.LookAt(rotationResult);

        // Get player forward vector
        Vector3 forward = transform.TransformDirection(Vector3.forward);

        // Move player
        characterController.SimpleMove(forward * (playerDirection.magnitude * playerSpeed * Time.deltaTime));
    }
}
