using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerController : MonoBehaviour
{
    [Header("Player Adjustments")]
    [SerializeField] float playerSpeed = 10.0f;
    [Range(0.0f, 1.0f)]
    [SerializeField] float playerRotationSpeed = 1.0f;
    [Range(45.0f, 180.0f)]
    [SerializeField] float playerPickupAngle = 90.0f;
    [SerializeField] float playerMaxDistance = 2.0f;
    [Tooltip("The amount of force when throwing your equipped item.")]
    [SerializeField] float playerThrowPower = 100.0f;
    [Tooltip("Push multiplyer when the character hits other rigidbodies.")]
    [SerializeField] float playerPushPower = 2.0f;
    [Tooltip("The gap between button presses until it detects a double tap.")]
    [SerializeField] int playerDoubleTap = 100;
    [Tooltip("How long you have to hold the drop button until it turns into a throw action in milliseconds.")]
    [SerializeField] int playerHoldThreshold = 125;

    [Space]
    [Header("Control Map")]
    [SerializeField] InputAction c_movement;
    [SerializeField] InputAction c_pickup;

    [Space]
    [Header("Links")]
    [SerializeField] Camera currentCamera;
    [SerializeField] CharacterController characterController;
    [SerializeField] Transform equippedPosition;
    [SerializeField] BoxCollider pickupArea;
    GameObject equippedItem;

    // Internal Variables
    Vector3 playerDirection = Vector3.zero;
    float dashTapTimer = 0.0f;
    int dashTapCount = 0;
    float throwHoldTimer = 0.0f;
    bool throwItem = false;
    bool justPickedUp = false;

    private void OnDrawGizmosSelected()
    {
        // Show equip position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(equippedPosition.position, 0.1f);

        // Show pickup line
    }

    private void OnValidate()
    {
        if (playerHoldThreshold < 125)
        {
            playerHoldThreshold = 125;
        }


        float boxSizeDepth = playerMaxDistance;
        float boxSizeHeight = 2.0f;
        float boxSizeWidth = playerMaxDistance * Mathf.Sin(playerPickupAngle);

        pickupArea.size = new Vector3(boxSizeWidth * 2.0f, boxSizeHeight, boxSizeDepth);
        pickupArea.transform.localPosition = new Vector3(0.0f, 0.0f, 1.0f + (boxSizeDepth * 0.5f));
    }

    private void Awake()
    {
        GameplayControls gameplayControls = new GameplayControls();

        // Attach all controls
        c_movement = gameplayControls.Default.Movement;
        c_movement.performed += OnMovementChanged;
        c_movement.canceled += OnMovementChanged;

        c_pickup = gameplayControls.Default.Pickup;
        c_pickup.started += OnPickupChanged;
        c_pickup.canceled += OnPickupChanged;
    }

    private void OnEnable()
    {
        c_movement.Enable();
        c_pickup.Enable();
    }
    private void OnDisable()
    {
        c_movement.Disable();
        c_pickup.Disable();
    }

    #region OnChanged Events

    private void OnMovementChanged(InputAction.CallbackContext context)
    {
        Vector2 movementVector = c_movement.ReadValue<Vector2>();

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

                dashTapTimer = playerDoubleTap / 1000.0f;
            }
        }

        if (dashTapCount == 1)
        {
            // TODO: Implement dash
        }

        playerDirection = new Vector3(directionX, 0.0f, directionY);
    }

    private void OnPickupChanged(InputAction.CallbackContext context)
    {
        ButtonControl pickupButton = c_pickup.activeControl as ButtonControl;

        // Check for button down/up state
        if (pickupButton.isPressed)
        {
            // Check if player is already holding an item
            if (!equippedItem)
            {
                // Fill equip slot
                if (PickupItem())
                {
                    justPickedUp = true;
                }
            }
            else
            {
                // Start throw timer
                throwHoldTimer = playerHoldThreshold / 1000.0f;
            }
        }
        else
        {
            // Check if player just picked up a new item
            if (!justPickedUp)
            {
                throwHoldTimer = 0.0f;

                // To throw it or not?? Hmmm?
                if (throwItem)
                {
                    Debug.Log("LMAO, MANZ ACTUALLY DID IT! XDDD");
                    // THROW IT!
                    ThrowItem();
                }
                else
                {
                    // You're lazy
                    UnequipItem();
                }
            }
            else
            {
                justPickedUp = false;
            }
        }

        throwItem = false;
    }

    // Allows player to push rigidbody objects
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // No rigidbody
        if (body == null || body.isKinematic)
        {
            return;
        }

        // We dont want to push objects below us
        //if (hit.moveDirection.y < -0.2f)
        //{
        //    return;
        //}

        /***
         * Calculate push direction from move direction,
         * we only push objects to the sides never up and down
         */
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

        /***
         * If you know how fast your character is trying to move,
         * then you can also multiply the push velocity by that.
         */

        // Apply the push
        body.velocity = pushDir * (characterController.velocity.magnitude * playerPushPower);
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        #region Timer Management
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

        // Decrease throw timer for hold detection
        if (equippedItem)
        {
            if (throwHoldTimer > 0.0f)
            {
                throwHoldTimer -= Time.deltaTime;
            }
            else if (throwHoldTimer < 0.0f)
            {
                throwHoldTimer = 0.0f;

                if (!throwItem)
                {
                    Debug.Log("THROW IT! YOU WON'T!");
                    throwItem = true;
                }
            }
        }
        #endregion
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

        // Only move the player if you're not throwing an item
        if (!throwItem)
            characterController.SimpleMove(forward * (playerDirection.magnitude * playerSpeed * Time.deltaTime));
    }

    private bool PickupItem()
    {
        // Get all items in pickup area
        Collider[] inPickupArea = Physics.OverlapBox(pickupArea.transform.position, pickupArea.size, pickupArea.transform.rotation);

        // Items in pickup area that is also in player view angle
        List<Transform> validItems = new List<Transform>();

        //Debug.Log("There are " + inPickupArea.Length + " items in the pickup area.");

        // Collect all items in pickup area and check if they are valid
        foreach (Collider c in inPickupArea)
        {
            // Calculate angle of item in pickup area from player
            Vector3 targetDir = c.transform.position - transform.position;
            float targetAngleFromPlayer = Vector3.Angle(targetDir, transform.forward);
            float targetDistanceFromPlayer = Vector3.Distance(transform.position, c.transform.position);

            //Debug.Log("Item: " + c.gameObject + " Angle: " + targetAngleFromPlayer + " Distance: " + targetDistanceFromPlayer);

                
            // Item is in player view and is not too far
            if (Math.Abs(targetAngleFromPlayer) < playerPickupAngle * 0.5f ||
                targetDistanceFromPlayer < playerMaxDistance)
            {
                // Skip self and items without the tag "Product"
                if (c.transform == transform)
                    continue;

                // Check if StockItem is an existing component in root of item
                StockItem stockItem = c.transform.GetComponentInParent<StockItem>();
                if (stockItem)
                {
                    // Add to valid list
                    validItems.Add(stockItem.transform);
                }
            }
        }

        // Stop code if there are no valid items in list
        if (validItems.Count == 0)
            return false;

        // Calculate the closest item from player
        Transform closestItem = validItems[0];
        float minDistance = Vector3.Distance(transform.position, validItems[0].position);

        // Continue scanning list if there are more than one item in the list
        if (validItems.Count > 1)
        {
            // Compare next item in the list to previously scanned closest item from player
            for (int i = 1; i < validItems.Count; i++)
            {
                float measuredDistance = Vector3.Distance(transform.position, validItems[i].position);

                if (measuredDistance < minDistance)
                {
                    closestItem = validItems[i];
                    minDistance = measuredDistance;
                }
            }
        }

        // Equip closest item
        EquipItem(closestItem);

        return true;
    }

    private void ThrowItem()
    {
        if (!equippedItem) return;

        // Remember item to throw
        GameObject itemToThrow = equippedItem;

        // Unequip item from player
        UnequipItem();

        // THROW DAT THING!
        itemToThrow.transform.GetComponent<Rigidbody>().AddForce(transform.forward * (characterController.velocity.magnitude + playerThrowPower));
    }

    private void EquipItem(Transform item)
    {
        // Get game object
        equippedItem = item.gameObject;

        // Get rigidbody from item and disable physics
        Rigidbody productRB = equippedItem.GetComponent<Rigidbody>();
        productRB.isKinematic = true;

        // Attach item to player hold position
        equippedItem.transform.SetParent(equippedPosition);
        equippedItem.transform.localPosition = Vector3.zero;
    }

    private void UnequipItem()
    {
        if (!equippedItem) return;

        // Get rigidbody on item and enable physics
        Rigidbody productRB = equippedItem.GetComponent<Rigidbody>();
        productRB.isKinematic = false;

        // De-attach item from player and remove item from equip slot
        equippedItem.transform.SetParent(null);
        equippedItem = null;
    }
}
