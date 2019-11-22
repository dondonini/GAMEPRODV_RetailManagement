using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerController : MonoBehaviour
{

    /************************************************/
    [Header("Player Adjustments")]
    [SerializeField] float movmentSpeed = 10.0f;
    [Range(0.0f, 1.0f)]
    [SerializeField] float rotationSpeed = 1.0f;
    [Range(45.0f, 90.0f)]
    [SerializeField] float pickupAngle = 90.0f;
    [SerializeField] float maxPickupDistance = 2.0f;
    //[SerializeField] float maxShelfDistance = 0.5f;
    //[SerializeField] float dashPower = 100.0f;
    //[SerializeField] float dashDuration = 5.0f;
    //[SerializeField] float dashDeacceleration = 5.0f;
    [Tooltip("The amount of force when throwing your equipped item.")]
    [SerializeField] float throwPower = 100.0f;
    [Tooltip("Push multiplyer when the character hits other rigidbodies.")]
    [SerializeField] float pushPower = 2.0f;
    [Tooltip("The gap between button presses until it detects a double tap.")]
    [SerializeField] int doubleTapThreshold = 100;
    [Tooltip("How long you have to hold the drop button until it turns into a throw action in milliseconds.")]
    [SerializeField] int holdThreshold = 125;

    /************************************************/
    [Space]
    [Header("Links")]
    [SerializeField] Camera currentCamera = null;
    [SerializeField] CharacterController characterController = null;
    [SerializeField] Transform equippedPosition = null;
    [SerializeField] BoxCollider pickupArea = null;
    GameObject equippedItem = null;

    /************************************************/
    // Runtime Variables

    Vector3 playerDirection = Vector3.zero;
    bool throwItem = false;
    bool justPickedUp = false;

    // Timers
    float dashTapTimer = 0.0f;
    int dashTapCount = 0;
    float throwHoldTimer = 0.0f;

    // Managers
    MapManager mapManager;
    GameplayControls gameplayControls;

    // Controls
    InputAction c_movement;
    InputAction c_pickup;

    private void OnDrawGizmos()
    {
        // Show equip position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(equippedPosition.position, 0.1f);

    }

    private void OnValidate()
    {
        if (holdThreshold < 125)
        {
            holdThreshold = 125;
        }

        float boxSizeDepth = maxPickupDistance;
        float boxSizeHeight = 2.0f;
        float boxSizeWidth = maxPickupDistance * Mathf.Sin(pickupAngle * Mathf.Deg2Rad);

        pickupArea.size = new Vector3(boxSizeWidth * 2.0f, boxSizeHeight, boxSizeDepth);
        pickupArea.transform.localPosition = new Vector3(0.0f, 0.0f, boxSizeDepth * 0.5f);
    }

    private void Awake()
    {
        gameplayControls = new GameplayControls();

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

    /************************************************/
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

                dashTapTimer = doubleTapThreshold / 1000.0f;
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
            if (equippedItem)
            {
                // Start throw timer
                throwHoldTimer = holdThreshold / 1000.0f;
            }
            else
            {
                Interact();
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
                    Interact();

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
        body.velocity = pushDir * (characterController.velocity.magnitude * pushPower);
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        mapManager = MapManager.GetInstance();
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

        //closestInteractible = EssentialFunctions.GetClosestInteractableInFOV(transform, pickupArea, pickupAngle, maxPickupDistance);
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
            rotationSpeed
        );

        // Rotate player
        characterController.transform.LookAt(rotationResult);

        // Get player forward vector
        Vector3 forward = transform.TransformDirection(Vector3.forward);

        // Only move the player if you're not throwing an item
        if (!throwItem)
            characterController.SimpleMove(forward * (playerDirection.magnitude * movmentSpeed * Time.deltaTime));
    }

    /************************************************/
    // Item interaction methods

    private void Interact()
    {
        string[] tagsToScan = { "Product", "StockCrate", "Shelf" };
        GameObject[] excludedGameObjects = { gameObject, equippedItem };

        GameObject closestInteractable = EssentialFunctions.GetClosestInteractableInFOV(transform, pickupArea, pickupAngle, maxPickupDistance, tagsToScan, excludedGameObjects);

        if (closestInteractable)
        {
            Debug.DrawLine(EssentialFunctions.GetMaxBounds(gameObject).center, EssentialFunctions.GetMaxBounds(closestInteractable).center, Color.red);

            switch (closestInteractable.tag)
            {
                case "Product":
                    {
                        // Fill equip slot
                        EquipItem(closestInteractable.transform);

                        break;
                    }
                case "StockCrate":
                    {
                        EquipItem(closestInteractable.transform);

                        break;
                    }
                case "Shelf":
                    {
                        ShelfContainer shelfComponent = closestInteractable.GetComponent<ShelfContainer>();
                        if (shelfComponent)
                        {
                            if (equippedItem)
                            {
                                AddStockToShelf(shelfComponent);
                            }
                            else
                            {
                                GetStockFromShelf(shelfComponent);
                            }
                        }
                        else
                        {
                            Debug.LogError("Shelf \"" + shelfComponent.gameObject + "\" is missing a ShelfContainer component or wrongly tagged!");
                        }
                        break;
                    }
            }

            justPickedUp = true;
        }
    }

    // ////////////////////////////////////////
    // Shelf Interation ///////////////////////
    // ////////////////////////////////////////

    private void GetStockFromShelf(ShelfContainer shelf)
    {

        StockTypes stockType = shelf.ShelfStockType;
        int amount = shelf.GetStock();

        if (amount != 0)
        {
            GameObject newItem = Instantiate(mapManager.GetStockTypePrefab(stockType)) as GameObject;
            EquipItem(newItem.transform);
        }
    }

    private void AddStockToShelf(ShelfContainer shelf)
    {
        // Check if there is something equipped
        if (!equippedItem) return;

        int result = 0;

        switch (equippedItem.tag)
        {
            case "Product":
                {
                    // Get stock type that is equipped
                    StockTypes equippedType = equippedItem.GetComponent<StockItem>().GetStockType();

                    result = shelf.AddStock(equippedType);
                    break;
                }
            case "StockCrate":
                {
                    StockCrate crateComponent = equippedItem.GetComponent<StockCrate>();
                    if (crateComponent)
                    {
                        result = shelf.AddStock(crateComponent.GetQuantity(), crateComponent.GetStockType());
                    }
                    break;
                }
        }

        if (result == 0)
        {
            Destroy(equippedItem);
        }
    }

    // ////////////////////////////////////////
    // Stock Interation ///////////////////////
    // ////////////////////////////////////////

    private void ThrowItem()
    {
        if (!equippedItem) return;

        // Remember item to throw
        GameObject itemToThrow = equippedItem;

        // Unequip item from player
        UnequipItem();

        // THROW DAT THING!
        itemToThrow.transform.GetComponent<Rigidbody>().AddForce(transform.forward * (characterController.velocity.magnitude + throwPower));
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

    // ////////////////////////////////////////
    // Stock Crate Interation /////////////////
    // ////////////////////////////////////////

}
