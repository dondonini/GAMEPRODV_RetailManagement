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

    [Header("AI Settings")]
    [SerializeField] float pickupDuration = 2.0f;
    [SerializeField] float purchaseDuration = 2.0f;

    //************************************************************************/
    // States


    [HideInInspector]
    public MoveToPositionState moveToPositionState;
    [HideInInspector]
    public RotateToVector rotateToVectorState;
    [HideInInspector]
    public DecideProductState decideProductState;

    //***********************************************************************/
    // Runtime Variables

    [HideInInspector]
    public MapManager mapManager = null;
    [HideInInspector]
    public NavMeshAgent agent = null;
    public NormalCustomer_SM currentState;
    NormalCustomer_SM previousState;

    public Tasks_AI currentTask = Tasks_AI.GetProduct;

    public Vector3 taskDestination;

    [HideInInspector]
    public StockTypes currentWantedProduct = StockTypes.None;

    void Awake()
    {
        moveToPositionState = new MoveToPositionState(this);
        rotateToVectorState = new RotateToVector(this);
        decideProductState = new DecideProductState(this);
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

        // Call initial start method on current state
        currentState.StartState();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if Map Manager is loaded
        if (!mapManager.isDoneLoading) return;

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
        if (!mapManager.isDoneLoading) return;

        // Fixed update current state
        currentState.FixedUpdateState();
    }
}
