using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NormalCustomer_AI : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float movementSpeed = 10.0f;
    [Range(0.0f, 1.0f)]
    public float rotationSpeed = 1.0f;
    [Range(45.0f, 180.0f)]
    [SerializeField] float pickupAngle = 90.0f;
    [SerializeField] float maxPickupDistance = 2.0f;
    [Tooltip("Push multiplyer when the character hits other rigidbodies.")]
    [SerializeField] float pushPower = 2.0f;
    public float pickupDuration = 2.0f;
    public float purchaseDuration = 2.0f;

    //************************************************************************
    [Header("References")]

    public Transform equippedPosition;
    [SerializeField] BoxCollider pickupArea;

    //************************************************************************
    // States

    [HideInInspector] public MoveToPositionState moveToPositionState;
    [HideInInspector] public QueuingState queuingState;
    [HideInInspector] public RotateToVector rotateToVectorState;
    [HideInInspector] public DecideProductState decideProductState;
    [HideInInspector] public DecideRegisterState decideRegisterState;
    [HideInInspector] public GrabProductState pickupProductState;
    [HideInInspector] public PurchaseState purchaseProductState;

    //************************************************************************
    // Runtime Variables

    [HideInInspector]
    public NavMeshAgent agent = null;
    public NormalCustomer_SM currentState;
    NormalCustomer_SM previousState;

    [HideInInspector] public StockTypes currentWantedProduct = StockTypes.None;

    [Header("Real-Time Stats")]
    [ReadOnly] public Tasks_AI currentTask = Tasks_AI.GetProduct;
    [ReadOnly] public Vector3 taskDestinationPosition;
    [ReadOnly] public Transform taskDestination;
    [ReadOnly] public GameObject equippedItem;
    [ReadOnly] [SerializeField] bool isActive = false;

    //*************************************************************************
    // Managers

    [HideInInspector] public MapManager mapManager = null;

    private void OnValidate()
    {
        float boxSizeDepth = maxPickupDistance;
        float boxSizeHeight = 2.0f;
        float boxSizeWidth = maxPickupDistance * Mathf.Sin(pickupAngle * Mathf.Deg2Rad);

        pickupArea.size = new Vector3(boxSizeWidth * 2.0f, boxSizeHeight, boxSizeDepth);
        pickupArea.transform.localPosition = new Vector3(0.0f, 0.0f, boxSizeDepth * 0.5f);
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
        body.velocity = pushDir * (agent.velocity.magnitude * pushPower);
    }

    void Awake()
    {
        moveToPositionState = new MoveToPositionState(this);
        queuingState = new QueuingState(this);
        rotateToVectorState = new RotateToVector(this);
        decideProductState = new DecideProductState(this);
        decideRegisterState = new DecideRegisterState(this);
        pickupProductState = new GrabProductState(this);
        purchaseProductState = new PurchaseState(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get MapManager
        mapManager = MapManager.GetInstance();

        // Begin with default state
        currentState = decideProductState;

        // Get NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if Map Manager is loaded
        if (mapManager.isDoneLoading && !isActive)
        {
            isActive = true;

            // Call initial start method on current state
            currentState.StartState();
        }
        
        if (!mapManager.isDoneLoading)
        {
            return;
        }

        // Update current state
        currentState.UpdateState();

        // Detect state change and call StartState method in current state
        if (previousState != null && previousState != currentState)
        {
            Debug.Log("State changed! " + previousState + " -> " + currentState);

            // Activate start state
            currentState.StartState();
        }

        // Update previous state
        previousState = currentState;
    }

    private void FixedUpdate()
    {
        // Check if Map Manager is loaded
        if (!isActive) return;

        // Fixed update current state
        currentState.FixedUpdateState();
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
}
