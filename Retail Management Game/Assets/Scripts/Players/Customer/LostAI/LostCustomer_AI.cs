using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class LostCustomer_AI : MonoBehaviour
{
    [Header("Settings")]
    float pushPower = 2.0f;

    //************************************************************************
    [Header("References - UI")]

    public Transform equippedPosition;
    [SerializeField] BoxCollider pickupArea = null;
    public ColliderToUnityEvents colliderEvents = null;
    public Transform infoHalo = null;
    public Canvas patienceBillboard = null;
    public Animator patienceAnimator = null;
    public TextMeshProUGUI patienceText = null;

    //************************************************************************
    // States

    //[HideInInspector] public GetProductState_NC getProductState;
    //[HideInInspector] public LeavingState_NC leavingState;
    //[HideInInspector] public PurchaseState_NC purchaseProductState;
    //[HideInInspector] public WaitingForProductState_NC waitForProductState;
    //[HideInInspector] public 

    //************************************************************************
    // Runtime Variables

    [HideInInspector]
    public NavMeshAgent agent = null;
    public NavMeshObstacle obstacle = null;
    public NormalCustomer_SM currentState;
    NormalCustomer_SM previousState;
    Vector3 previousPosition;
    bool firstTime = false;

    MapManager mapManager = null;
    GameManager gameManager = null;

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

    // Start is called before the first frame update
    void Start()
    {
        // Get MapManager
        mapManager = MapManager.GetInstance();
        gameManager = GameManager.GetInstance();

        // Begin with default state
        //currentState = getProductState;

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
}
