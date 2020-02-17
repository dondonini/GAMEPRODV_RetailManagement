using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

[RequireComponent(typeof(NavMeshAgent))]
public class ImpatientCustomer_AI : MonoBehaviour
{
    [Header("Movement Settings")]
    [Range(0.0f, 1.0f)]
    public float rotationSpeed = 1.0f;
    [Range(45.0f, 180.0f)]
    [SerializeField] float pickupAngle = 90.0f;
    public float maxPickupDistance = 1.0f;
    [Tooltip("Push multiplyer when the character hits other rigidbodies.")]
    [SerializeField] float pushPower = 2.0f;

    [Header("Waiting Durations")]
    public float pickupDuration = 2.0f;
    public float purchaseDuration = 2.0f;
    [Tooltip("How long the customer will wait in a queue (standing still).")]
    public float queuingPatience = 20.0f;
    public float stockPatience = 20.0f;

    [Header("Other Settings")]
    public float stuckThreshold = 3.0f;
    public float unstuckDuration = 3.0f;
    public float collisionSensitivity = 2.0f;

    //************************************************************************
    [Header("References")]

    public Transform equippedPosition;
    [SerializeField] BoxCollider pickupArea = null;
    public ColliderToUnityEvents colliderEvents = null;
    public Transform infoHalo = null;
    public Canvas patienceBillboard = null;
    public Animator patienceAnimator = null;
    public TextMeshProUGUI patienceText = null;

    //************************************************************************
    // States

    [HideInInspector] public GetProductState_IC getProductState;
    [HideInInspector] public LeavingState_IC leavingState;
    [HideInInspector] public PurchaseState_IC purchaseProductState;
    [HideInInspector] public WaitingForProductState_IC waitForProductState;

    //************************************************************************
    // Runtime Variables

    [HideInInspector]
    public NavMeshAgent agent = null;
    public NavMeshObstacle obstacle = null;
    public NormalCustomer_SM currentState;
    NormalCustomer_SM previousState;
    Vector3 previousPosition;

    bool firstTime = false;
    float unstuckTimer = 0.0f;

    [HideInInspector] public StockTypes currentWantedProduct = StockTypes.None;

    [Header("Real-Time Stats")]
#pragma warning disable IDE0052 // Remove unread private members
    [ReadOnly] [SerializeField] string currentStateAction = "";
    [ReadOnly] [SerializeField] string subStateMachineState = "";
#pragma warning restore IDE0052 // Remove unread private members
    [ReadOnly] public Vector3 taskDestinationPosition;
    [ReadOnly] public Transform taskDestination = null;
    [ReadOnly] public GameObject equippedItem = null;
    [ReadOnly] [SerializeField] bool isActive = false;
    [ReadOnly] public bool isGettingUnstuck = false;

    //*************************************************************************
    // Managers

    [HideInInspector] public MapManager mapManager = null;
    [HideInInspector] public GameManager gameManager = null;

    private void OnValidate()
    {
        float boxSizeDepth = maxPickupDistance;
        float boxSizeHeight = 2.0f;
        float boxSizeWidth = maxPickupDistance * Mathf.Sin(pickupAngle * Mathf.Deg2Rad);

        pickupArea.size = new Vector3(boxSizeWidth * 2.0f, boxSizeHeight, boxSizeDepth);
        pickupArea.transform.localPosition = new Vector3(0.0f, 0.0f, boxSizeDepth * 0.5f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(equippedPosition.position, 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        if (agent)
        {
            Gizmos.color = Color.black;

            NavMeshPath path = agent.path;

            if (path.corners.Length > 2) //if the path has 1 or no corners, there is no need
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
                    //Gizmos.DrawSphere(path.corners[i], 0.1f);
                }
            else if (path.corners.Length == 2)
            {
                Gizmos.DrawLine(path.corners[0], path.corners[1]);
            }
        }
        
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
        body.velocity = pushDirection * (agent.velocity.magnitude * pushPower);
    }

    void Awake()
    {
        getProductState = new GetProductState_IC(this);
        leavingState = new LeavingState_IC(this);
        purchaseProductState = new PurchaseState_IC(this);
        waitForProductState = new WaitingForProductState_IC(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get MapManager
        mapManager = MapManager.GetInstance();
        gameManager = GameManager.GetInstance();

        // Begin with default state
        currentState = getProductState;

        // Get NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        agent.autoRepath = true;

        // Get NavMeshObstacle
        obstacle = GetComponent<NavMeshObstacle>();
    }

    // Update is called once per frame
    void Update()
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

    public void UpdateActionStatus(string action)
    {
        currentStateAction = string.Format("{0} / {1}", currentState, action);
        subStateMachineState = action;
    }
    

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

    public bool IsHoldingItem()
    {
        return equippedItem;
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

    public void EquipItem(Transform item)
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
    public GameObject UnequipItem()
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

    public void Interact()
    {
        ShelfContainer interactShelf = TaskDestinationAsShelf();

        // Check if task destination is a shelf
        if (interactShelf)
        {
            // Is not holding a product in hand
            if (!equippedItem)
            {
                GetStockFromShelf(interactShelf);
            }
        }
    }

    public void GetStockFromShelf(ShelfContainer shelf)
    {
        StockTypes stockType = shelf.ShelfStockType;
        int amount = shelf.GetStock();

        if (amount != 0)
        {
            GameObject newItem = Object.Instantiate(mapManager.GetStockTypePrefab(stockType)) as GameObject;
            EquipItem(newItem.transform);
        }
    }

    public NormalCustomer_SM GetPreviousState()
    {
        return previousState;
    }
}
