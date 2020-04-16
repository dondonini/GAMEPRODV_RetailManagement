using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public abstract class Customer_AI : MonoBehaviour
{
    public enum Tasks
    {
        GetProduct,
        GetToRegister,
        LeaveStore
    }

    [Header("Movement Settings")]
    [Range(0.0f, 1.0f)]
    public float rotationSpeed = 1.0f;
    public float maxPickupDistance = 1.0f;
    [Range(45.0f, 180.0f)]
    [SerializeField] protected float pickupAngle = 90.0f;
    [Tooltip("Push multiplier when the character hits other rigidbodies.")]
    [SerializeField] protected float pushPower = 2.0f;

    [Header("Waiting Durations")]
    public float pickupDuration = 2.0f;
    public float purchaseDuration = 2.0f;

    [Tooltip("How long the customer will wait in a queue (standing still).")]
    public float queuingPatience = 20.0f;
    public float stockPatience = 20.0f;

    public float collisionSensitivity = 2.0f;

    //************************************************************************
    [Header("References - UI")]

    [SerializeField] protected BoxCollider pickupArea;
    public ColliderToUnityEvents colliderEvents;
    public Transform infoHalo;
    public GameObject billboard;
    public Canvas billboardCanvas;
    public Animator billboardAnimator;
    public Animator patienceAnimator;
    public TextMeshProUGUI patienceText;

    //*************************************************************************
    // Managers

    [HideInInspector] public MapManager mapManager;
    [HideInInspector] public GameManager gameManager;

    //************************************************************************
    // Runtime Variables

    public Transform equippedPosition;

    [FormerlySerializedAs("agent")] [HideInInspector]
    public NavMeshAgent navMeshAgent;
    public IBase_SM currentState;
    protected IBase_SM previousState;
    protected Vector3 previousPosition;

    protected bool firstTime;

    [HideInInspector] 

    //************************************************************************
    // Debug
    [Header("Debug")]
    [ReadOnly] public Vector3 taskDestinationPosition;
    [ReadOnly] public Transform taskDestination;
    [ReadOnly] public StockTypes currentWantedProduct = StockTypes.None;
    [ReadOnly] public GameObject equippedItem;
    [ReadOnly] [SerializeField] protected bool isActive;
    [ReadOnly] public bool isGettingUnstuck;

#pragma warning disable IDE0052 // Remove unread private members
    [ReadOnly] [SerializeField] private string internalDebugLog = "";
#pragma warning restore IDE0052 // Remove unread private members

    //////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////

    // Allows player to push rigidbody objects
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // No rigidbody
        if (body == null || body.isKinematic)
        {
            return;
        }

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.2f)
        {
            return;
        }

        /***
         * Calculate push direction from move direction,
         * we only push objects to the sides never up and down
         */
        Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

        /***
         * If you know how fast your character is trying to move,
         * then you can also multiply the push velocity by that.
         */

        // Apply the push
        body.velocity = pushDirection * (navMeshAgent.velocity.magnitude * pushPower);
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        // Get Managers
        mapManager = MapManager.GetInstance();
        gameManager = GameManager.GetInstance();

        // Get NavMeshAgent
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.autoRepath = true;
    }

    // Update is called once per frame
    private void Update()
    {
        // Check if Map Manager is loaded
        if (mapManager.isDoneLoading && !firstTime)
        {
            firstTime = true;

            EnableStateMachine();

            // Call initial start method on current state
            currentState.StartState();
        }

        if (!isActive) return;

        // Update current state
        currentState.UpdateState();

        // Detect state change and call StartState method in current state
        if (previousState != null && previousState != currentState)
        {
            Debug.Log("State changed! " + previousState + " -> " + currentState);

            // Activate exit state of previous state
            previousState.ExitState();

            // Activate start state
            currentState.StartState();
        }

        // Update previous state
        previousState = currentState;

        previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // Check if Map Manager is loaded
        if (!isActive) return;

        // Fixed update current state
        currentState.FixedUpdateState();
    }

    public void EnableStateMachine()
    {
        isActive = true;
    }

    public void DisableStateMachine()
    {
        isActive = false;
    }

    public bool IsStateMachineActive()
    {
        return isActive;
    }

    #region Interactions

    public void Interact()
    {
        ShelfContainer interactShelf = TaskDestinationAsShelf();

        // Check if task destination is a shelf
        if (!interactShelf) return;
        
        // Is not holding a product in hand
        if (!equippedItem)
        {
            GetStockFromShelf(interactShelf);
        }
    }

    public bool IsHoldingItem()
    {
        return equippedItem;
    }

    public void EquipItem(Transform item)
    {
        // Get game object
        equippedItem = item.gameObject;

        // Get rigidbody from item and disable physics
        Rigidbody productRigidbody = equippedItem.GetComponent<Rigidbody>();
        productRigidbody.isKinematic = true;

        // Attach item to player hold position
        equippedItem.transform.SetParent(equippedPosition);
        equippedItem.transform.localPosition = Vector3.zero;
    }

    public GameObject UnequipItem()
    {
        if (!equippedItem) return null;

        GameObject wasHolding = equippedItem;

        // Get rigidbody on item and enable physics
        Rigidbody productRigidbody = equippedItem.GetComponent<Rigidbody>();
        productRigidbody.isKinematic = false;

        // De-attach item from player and remove item from equip slot
        equippedItem.transform.SetParent(null);
        equippedItem = null;

        return wasHolding;
    }

    public void GetStockFromShelf(ShelfContainer shelf)
    {
        StockTypes stockType = shelf.ShelfStockType;
        int amount = shelf.GetStock();

        if (amount == 0) return;
        
        GameObject newItem = Instantiate(mapManager.GetStockTypePrefab(stockType));
        EquipItem(newItem.transform);
    }

    #endregion

    public ShelfContainer TaskDestinationAsShelf()
    {
        ShelfContainer result = taskDestination.GetComponent<ShelfContainer>();
        return result;
    }

    public CashRegister TaskDestinationAsRegister()
    {
        CashRegister result = taskDestination.GetComponent<CashRegister>();
        return result;
    }

    public void UpdateInternalDebug(string debugMsg)
    {
        internalDebugLog = debugMsg;
    }
}
