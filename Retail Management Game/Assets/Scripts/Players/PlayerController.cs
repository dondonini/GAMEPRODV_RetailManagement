using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] InputAction movement;
    [SerializeField] InputActionAsset playerControls;

    private void Awake()
    {
        InputActionMap gameplayActionMap = playerControls.FindActionMap("Gameplay");

        movement = gameplayActionMap.FindAction("Movement");

        movement.performed += OnMovementChanged;
    }

    private void OnDisable()
    {
        movement.Disable();
    }

    private void OnEnable()
    {
        movement.Enable();
    }

    private void OnMovementChanged(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();

        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
