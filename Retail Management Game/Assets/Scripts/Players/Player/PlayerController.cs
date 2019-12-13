using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public enum PlayerMode
{
    Walking,
    Cashier,
    Receiving,
}

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
    [SerializeField] float dashMultiplier = 100.0f;
    [SerializeField] float dashDuration = 5.0f;
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

    PlayerMode currentPlayerMode = PlayerMode.Walking;

    Vector3 playerDirection = Vector3.zero;
    Vector3 playerVelocity = Vector3.zero;
    Vector3 previousPlayerPosition = Vector3.zero;
    bool throwItem = false;
    bool justPickedUp = false;
    string lastPressedKeyControl = "";

    // Timers
    float dashTapTimer = 0.0f;
    int dashTapCount = 0;
    float dashTimer = 0.0f;
    float throwHoldTimer = 0.0f;

    // Managers
    MapManager mapManager;
    GameManager gameManager;

    // Current Interactive
    CashRegister interact_Register = null;

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

    /************************************************/
    #region OnChanged Events
    
    public void OnMovement(InputAction.CallbackContext context)
    {
        Vector2 movementVector = context.ReadValue<Vector2>();

        playerDirection = new Vector3(movementVector.x, 0.0f, movementVector.y);

        // Check if input device is the keyboard
        if (context.control.device.name == "Keyboard" && context.started)
        {
            ButtonControl keyControl = context.control as ButtonControl;

            // Detect if change is not cancelled
            if (movementVector.magnitude > 0.0f)
            {
                if (keyControl.name != lastPressedKeyControl)
                {
                    lastPressedKeyControl = keyControl.name;
                }
                else
                {
                    if (dashTapTimer > 0.0f)
                    {
                        dashTapCount++;
                    }
                    dashTapTimer = doubleTapThreshold / 1000.0f;
                }
            }
        }

        if (dashTapCount == 1)
        {
            // TODO: Implement dash
            Dash();
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
            if (dashTimer <= 0.0f)
                Dash();
    }

    public void OnPickup(InputAction.CallbackContext context)
    {
        if (gameManager && gameManager.IsGameOver())
            gameManager.ToScoreBoard();


        switch (currentPlayerMode)
        {
            case PlayerMode.Walking:
                {
                    // Check for button down/up state
                    if (context.started)
                    {
                        // Check if player is already holding an item
                        if (equippedItem)
                        {
                            // Start throw timer
                            throwHoldTimer = holdThreshold / 1000.0f;
                        }
                        else
                        {
                            PickupObject();
                        }
                    }
                    else if (context.canceled)
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
                                PickupObject();

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

                    break;
                }
            case PlayerMode.Cashier:
                {
                    if (!context.performed) return;

                    interact_Register.CashCustomerOut();

                    break;
                }
        }
        
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        switch (currentPlayerMode)
        {
            case PlayerMode.Walking:
                {
                    Interact();

                    break;
                }
            case PlayerMode.Cashier:
                {
                    interact_Register = null;
                    currentPlayerMode = PlayerMode.Walking;

                    break;
                }
        }
    }

    public void OnStart(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (gameManager.GetGameState() == GameState.Paused)
        {
            gameManager.ResumeGame();
        }
        else
        {
            gameManager.PauseGame();
        }
    }

    // Allows player to push rigidbody objects
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // No rigidbody
        if (body == null || body.isKinematic) return;

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.2f) return;

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
        gameManager = GameManager.GetInstance();

        previousPlayerPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayer();

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
            lastPressedKeyControl = "";
        }

        if (dashTimer > 0.0f)
        {
            dashTimer -= Time.deltaTime;
        }
        else if (dashTimer < 0.0f)
        {
            dashTimer = 0.0f;
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

        playerVelocity = (transform.position - previousPlayerPosition) * Time.deltaTime;

        previousPlayerPosition = transform.position;
    }

    private void FixedUpdate()
    {

    }

    /// <summary>
    /// Update player movement
    /// </summary>
    private void UpdatePlayer()
    {
        // Only walk when player is in walking mode
        if (currentPlayerMode != PlayerMode.Walking) return;

        // Calculate relative camera direction
        Vector3 relativeDirection = currentCamera.transform.TransformVector(playerDirection);
        relativeDirection.Scale(Vector3.forward + Vector3.right);

        Vector3 rotationResult;

        // Ease rotation
        if (playerVelocity.magnitude < 0.1f)
        {
            rotationResult = characterController.transform.position + relativeDirection;
        }
        else
        {
            rotationResult = Vector3.Lerp(
                characterController.transform.position + relativeDirection,
                characterController.transform.position + characterController.transform.forward,
                rotationSpeed * playerVelocity.normalized.magnitude
            );
        }


        // Rotate player
        characterController.transform.LookAt(rotationResult);
        //characterController.transform.rotation = Quaternion.LookRotation(relativeDirection);

        // Get player forward vector
        Vector3 forward = transform.TransformDirection(Vector3.forward);

        // Only move the player if you're not throwing an item
        if (!throwItem)
            characterController.SimpleMove(forward * 
                (
                    playerDirection.magnitude * movmentSpeed * Time.fixedDeltaTime *
                    (1 + (dashMultiplier
                          * (dashTimer / dashDuration)
                          * Time.fixedDeltaTime))
                )
            );
    }

    private void Dash()
    {
        dashTimer = dashDuration;
    }

    /************************************************/
    // Item interaction methods

    private void Interact()
    {
        string[] tagsToScan = { "Register" };
        GameObject[] excludedGameObjects = { gameObject };

        GameObject closestInteractable = EssentialFunctions.GetClosestInteractableInFOV(transform, pickupArea, pickupAngle, maxPickupDistance, tagsToScan, excludedGameObjects);

        if (closestInteractable)
        {
            switch (closestInteractable.tag)
            {
                case "Register":
                    {
                        interact_Register = closestInteractable.GetComponent<CashRegister>();

                        currentPlayerMode = PlayerMode.Cashier;

                        transform.position = interact_Register.GetStandingPosition().position;
                        transform.rotation = interact_Register.GetStandingPosition().rotation;

                        break;
                    }
            }
        }

    }

    private void PickupObject()
    {
        string[] tagsToScan = { "Product", "StockCrate", "Shelf" };
        GameObject[] excludedGameObjects = { gameObject, equippedItem };

        GameObject closestInteractable = EssentialFunctions.GetClosestInteractableInFOV(transform, pickupArea, pickupAngle, maxPickupDistance, tagsToScan, excludedGameObjects);

        Vector3 org = EssentialFunctions.GetMaxBounds(gameObject).center;

        if (closestInteractable)
        {
            Vector3 tar = EssentialFunctions.GetMaxBounds(closestInteractable).center;

            Vector3 dirToInteractable = org - tar;

            Debug.DrawRay(org, dirToInteractable, Color.red, 1.0f);

            if (Physics.Raycast(org, dirToInteractable, out RaycastHit hit, 5.0f) && hit.transform.root != closestInteractable.transform) return;

            //Debug.Log(hit.transform);

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
                        result = shelf.AddCrate(crateComponent);
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

    private GameObject UnequipItem()
    {
        if (!equippedItem) return null;

        GameObject wasHolding = equippedItem;

        // Get rigidbody on item and enable physics
        Rigidbody productRB = equippedItem.GetComponent<Rigidbody>();
        productRB.isKinematic = false;

        // De-attach item from player and remove item from equip slot
        equippedItem.transform.SetParent(null);
        equippedItem = null;

        return wasHolding;
    }
}
