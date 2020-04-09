using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NormalCustomer_AI : Customer_AI
{
    public Tasks_AI currentTask = Tasks_AI.GetProduct;


    [Header("Other Settings")]
    public float stuckThreshold = 3.0f;
    public float unstuckDuration = 3.0f;


    //************************************************************************
    // States

    [HideInInspector] public GetProductState_NC getProductState;
    [HideInInspector] public LeavingState_NC leavingState;
    [HideInInspector] public PurchaseState_NC purchaseProductState;
    [HideInInspector] public WaitingForProductState_NC waitForProductState;

    //************************************************************************
    // Debug Visuals

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

    //////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////

    void Awake()
    {
        getProductState = new GetProductState_NC(this);
        leavingState = new LeavingState_NC(this);
        purchaseProductState = new PurchaseState_NC(this);
        waitForProductState = new WaitingForProductState_NC(this);
    }

    protected override void Start()
    {
        base.Start();

        // Begin with default state
        currentState = getProductState;

        // Setup start task
        currentTask = Tasks_AI.GetProduct;
    }

    public INormalCustomer_SM GetPreviousState()
    {
        return previousState as INormalCustomer_SM;
    }
}
