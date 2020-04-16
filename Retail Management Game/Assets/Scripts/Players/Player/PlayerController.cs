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
    [SerializeField]
    private float movementSpeed = 10.0f;
    [Range(0.0f, 1.0f)]
    [SerializeField]
    private float rotationSpeed = 1.0f;
    [Range(45.0f, 90.0f)]
    [SerializeField]
    private bool movementRelativeToCamera;
    [SerializeField] private float pickupAngle = 90.0f;
    [SerializeField] private float maxPickupDistance = 2.0f;
    //[SerializeField] float maxShelfDistance = 0.5f;
    [SerializeField] private float dashMultiplier = 100.0f;
    [SerializeField] private float dashDuration = 5.0f;
    [Tooltip("The amount of force when throwing your equipped item.")]
    [SerializeField]
    private float throwPower = 100.0f;
    [Tooltip("Push multiplier when the character hits other rigidbodies.")]
    [SerializeField]
    private float pushPower = 2.0f;
    [Tooltip("The gap between button presses until it detects a double tap.")]
    [SerializeField]
    private int doubleTapThreshold = 100;
    [Tooltip("How long you have to hold the drop button until it turns into a throw action in milliseconds.")]
    [SerializeField]
    private int holdThreshold = 125;

    /************************************************/
    [Space]
    [Header("Links")]
    [SerializeField]
    private Camera currentCamera;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform equippedPosition;
    [SerializeField] private BoxCollider pickupArea;
    private GameObject _equippedItem;

    /************************************************/
    // Runtime Variables

    private PlayerMode _currentPlayerMode = PlayerMode.Walking;

    private Vector3 _playerDirection = Vector3.zero;
    private Vector3 _playerVelocity = Vector3.zero;
    private Vector3 _previousPlayerPosition = Vector3.zero;
    private bool _throwItem;
    private bool _justPickedUp;
    private string _lastPressedKeyControl = "";

    // Timers
    private float _dashTapTimer;
    private int _dashTapCount;
    private float _dashTimer;
    private float _throwHoldTimer;

    // Managers
    private MapManager _mapManager;
    private GameManager _gameManager;

    // Current Interactive
    private CashRegister _interactRegister;

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

        _playerDirection = new Vector3(movementVector.x, 0.0f, movementVector.y);

        // Check if input device is the keyboard
        if (context.control.device.name == "Keyboard" && context.started)
        {
            ButtonControl keyControl = context.control as ButtonControl;

            // Detect if change is not cancelled
            if (movementVector.magnitude > 0.0f)
            {
                if (keyControl != null && keyControl.name != _lastPressedKeyControl)
                {
                    _lastPressedKeyControl = keyControl.name;
                }
                else
                {
                    if (_dashTapTimer > 0.0f)
                    {
                        _dashTapCount++;
                    }
                    _dashTapTimer = doubleTapThreshold / 1000.0f;
                }
            }
        }

        if (_dashTapCount == 1)
        {
            // TODO: Implement dash
            Dash();
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        if (_dashTimer <= 0.0f)
            Dash();
    }

    public void OnPickup(InputAction.CallbackContext context)
    {
        if (_gameManager && _gameManager.IsGameOver())
            _gameManager.ToScoreBoard();


        switch (_currentPlayerMode)
        {
            case PlayerMode.Walking:
                {
                    // Check for button down/up state
                    if (context.started)
                    {
                        // Check if player is already holding an item
                        if (_equippedItem)
                        {
                            // Start throw timer
                            _throwHoldTimer = holdThreshold / 1000.0f;
                        }
                        else
                        {
                            PickupObject();
                        }
                    }
                    else if (context.canceled)
                    {
                        // Check if player just picked up a new item
                        if (!_justPickedUp)
                        {
                            _throwHoldTimer = 0.0f;

                            // To throw it or not?? Hmmm?
                            if (_throwItem)
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
                            _justPickedUp = false;
                        }
                    }

                    _throwItem = false;

                    break;
                }
            case PlayerMode.Cashier:
                {
                    if (!context.performed) return;

                    _interactRegister.CashCustomerOut();

                    break;
                }
            case PlayerMode.Receiving:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        switch (_currentPlayerMode)
        {
            case PlayerMode.Walking:
                {
                    Interact();

                    break;
                }
            case PlayerMode.Cashier:
                {
                    _interactRegister = null;
                    _currentPlayerMode = PlayerMode.Walking;

                    break;
                }
            case PlayerMode.Receiving:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnStart(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (_gameManager.GetGameState() == GameState.Paused)
        {
            _gameManager.ResumeGame();
        }
        else
        {
            _gameManager.PauseGame();
        }
    }

    // Allows player to push rigidbody objects
    private void OnControllerColliderHit(ControllerColliderHit hit)
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
    private void Start()
    {
        _mapManager = MapManager.GetInstance();
        _gameManager = GameManager.GetInstance();

        _previousPlayerPosition = transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        UpdatePlayer();

        #region Timer Management

        // Decrease dash timer for multi tap detection
        if (_dashTapTimer > 0.0f)
        {
            _dashTapTimer -= Time.deltaTime;
        }
        else if (_dashTapTimer < 0.0f)
        {
            _dashTapTimer = 0.0f;
            _dashTapCount = 0;
            _lastPressedKeyControl = "";
        }

        if (_dashTimer > 0.0f)
        {
            _dashTimer -= Time.deltaTime;
        }
        else if (_dashTimer < 0.0f)
        {
            _dashTimer = 0.0f;
        }

        // Decrease throw timer for hold detection
        if (_equippedItem)
        {
            if (_throwHoldTimer > 0.0f)
            {
                _throwHoldTimer -= Time.deltaTime;
            }
            else if (_throwHoldTimer < 0.0f)
            {
                _throwHoldTimer = 0.0f;

                if (!_throwItem)
                {
                    Debug.Log("THROW IT! YOU WON'T!");
                    _throwItem = true;
                }
            }
        }
        #endregion

        _playerVelocity = (transform.position - _previousPlayerPosition) * Time.deltaTime;

        _previousPlayerPosition = transform.position;
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
        if (_currentPlayerMode != PlayerMode.Walking) return;

        if (movementRelativeToCamera)
        {
            // TODO: Toggle camera relative movement
        }

        // Calculate relative camera direction
        Vector3 relativeDirection = currentCamera.transform.TransformVector(_playerDirection);
        relativeDirection.Scale(Vector3.forward + Vector3.right);

        Vector3 rotationResult;

        // Ease rotation
        if (_playerVelocity.magnitude < 0.1f)
        {
            rotationResult = characterController.transform.position + relativeDirection;
        }
        else
        {
            Transform characterControllerTransform = characterController.transform;
            Vector3 characterControllerPosition = characterControllerTransform.position;
            rotationResult = Vector3.Lerp(
                characterControllerPosition + relativeDirection,
                characterControllerPosition + characterControllerTransform.forward,
                rotationSpeed * _playerVelocity.normalized.magnitude
            );
        }


        // Rotate player
        characterController.transform.LookAt(rotationResult);
        //characterController.transform.rotation = Quaternion.LookRotation(relativeDirection);

        // Get player forward vector
        Vector3 forward = transform.TransformDirection(Vector3.forward);

        // Only move the player if you're not throwing an item
        if (!_throwItem)
            characterController.SimpleMove(forward * 
                (
                    _playerDirection.magnitude * movementSpeed * Time.fixedDeltaTime *
                    (1 + (dashMultiplier
                          * (_dashTimer / dashDuration)
                          * Time.fixedDeltaTime))
                )
            );
    }

    private void Dash()
    {
        _dashTimer = dashDuration;
    }

    /************************************************/
    // Item interaction methods

    private void Interact()
    {
        string[] tagsToScan = { "Register" };
        GameObject[] excludedGameObjects = { gameObject };

        GameObject closestInteractable = EssentialFunctions.GetClosestInteractableInFOV(transform, pickupArea, pickupAngle, maxPickupDistance, tagsToScan, excludedGameObjects);

        if (!closestInteractable) return;
        
        switch (closestInteractable.tag)
        {
            case "Register":
            {
                _interactRegister = closestInteractable.GetComponent<CashRegister>();

                _currentPlayerMode = PlayerMode.Cashier;

                transform.position = _interactRegister.GetStandingPosition().position;
                transform.rotation = _interactRegister.GetStandingPosition().rotation;

                break;
            }
        }

    }

    private void PickupObject()
    {
        string[] tagsToScan = { "Product", "StockCrate", "Shelf" };
        GameObject[] excludedGameObjects = { gameObject, _equippedItem };

        GameObject closestInteractable = EssentialFunctions.GetClosestInteractableInFOV(transform, pickupArea, pickupAngle, maxPickupDistance, tagsToScan, excludedGameObjects);

        Vector3 org = EssentialFunctions.GetMaxBounds(gameObject).center;

        if (!closestInteractable) return;
        
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
                    if (_equippedItem)
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

        _justPickedUp = true;
    }



    // ////////////////////////////////////////
    // Shelf Interaction ///////////////////////
    // ////////////////////////////////////////

    private void GetStockFromShelf(ShelfContainer shelf)
    {

        StockTypes stockType = shelf.ShelfStockType;
        int amount = shelf.GetStock();

        if (amount == 0) return;
        
        GameObject newItem = Instantiate(_mapManager.GetStockTypePrefab(stockType)) as GameObject;
        EquipItem(newItem.transform);
    }

    private void AddStockToShelf(ShelfContainer shelf)
    {
        // Check if there is something equipped
        if (!_equippedItem) return;

        int result = 0;

        switch (_equippedItem.tag)
        {
            case "Product":
                {
                    // Get stock type that is equipped
                    StockTypes equippedType = _equippedItem.GetComponent<StockItem>().GetStockType();

                    result = shelf.AddStock(equippedType);
                    break;
                }
            case "StockCrate":
                {
                    StockCrate crateComponent = _equippedItem.GetComponent<StockCrate>();
                    if (crateComponent)
                    {
                        result = shelf.AddCrate(crateComponent);
                    }
                    break;
                }
        }

        if (result == 0)
        {
            Destroy(_equippedItem);
        }
    }

    // ////////////////////////////////////////
    // Stock Interaction ///////////////////////
    // ////////////////////////////////////////

    private void ThrowItem()
    {
        if (!_equippedItem) return;

        // Remember item to throw
        GameObject itemToThrow = _equippedItem;

        // Unequip item from player
        UnequipItem();

        // THROW DAT THING!
        itemToThrow.transform.GetComponent<Rigidbody>().AddForce(transform.forward * (characterController.velocity.magnitude + throwPower));
    }

    private void EquipItem(Transform item)
    {
        // Get game object
        _equippedItem = item.gameObject;

        // Get rigidbody from item and disable physics
        Rigidbody productRb = _equippedItem.GetComponent<Rigidbody>();
        productRb.isKinematic = true;

        // Attach item to player hold position
        _equippedItem.transform.SetParent(equippedPosition);
        _equippedItem.transform.localPosition = Vector3.zero;
    }

    private GameObject UnequipItem()
    {
        if (!_equippedItem) return null;

        GameObject wasHolding = _equippedItem;

        // Get rigidbody on item and enable physics
        Rigidbody productRb = _equippedItem.GetComponent<Rigidbody>();
        productRb.isKinematic = false;

        // De-attach item from player and remove item from equip slot
        _equippedItem.transform.SetParent(null);
        _equippedItem = null;

        return wasHolding;
    }
}
