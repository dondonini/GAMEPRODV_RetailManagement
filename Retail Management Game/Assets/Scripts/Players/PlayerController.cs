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

    [Space]
    [Header("Control Map")]
    [SerializeField] InputAction movement;

    [Space]
    [Header("Links")]
    [SerializeField] Camera currentCamera;
    [SerializeField] CharacterController characterController;
    Vector3 playerDirection = Vector3.zero;

    private void Awake()
    {
        GameplayControls gameplayControls = new GameplayControls();

        // Attach all controls
        movement = gameplayControls.Default.Movement;
        movement.performed += OnMovementChanged;
        movement.canceled += OnMovementChanged;


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
        // Parse movement data

        float directionX = movement.ReadValue<Vector2>().x;
        float directionY = movement.ReadValue<Vector2>().y;

        playerDirection = new Vector3(directionX, 0.0f, directionY);
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
        UpdatePlayer();
    }

    /// <summary>
    /// Update player movement
    /// </summary>
    private void UpdatePlayer()
    {
        // Calculate relative camera
        Vector3 relativeDirection = currentCamera.transform.TransformVector(playerDirection);
        relativeDirection.Scale(Vector3.forward + Vector3.right);

        // Rotate player
        characterController.transform.LookAt(
            Vector3.Lerp(characterController.transform.position + characterController.transform.forward, 
            characterController.transform.position + relativeDirection, 
            playerRotationSpeed
            ));

        // Get player forward vector
        Vector3 forward = transform.TransformDirection(Vector3.forward);

        // Move player
        characterController.Move(forward * (playerDirection.magnitude * playerSpeed * Time.deltaTime));

        
    }
}
