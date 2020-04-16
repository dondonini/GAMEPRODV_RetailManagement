using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class LostCustomer_AI : Customer_AI
{
    public Tasks_AI currentTask = Tasks_AI.Wander;

    public float wanderFrequency = 1.0f;
    public PlayerController playerToFollow;
    public float checkRadius = 2.0f;

    public Image wantedThumbnail;

    //************************************************************************
    // States

    [HideInInspector] public WanderState_LC wanderState;
    [HideInInspector] public FollowState_LC followState;
    [HideInInspector] public GetProductState_LC getProductState;
    [HideInInspector] public PurchaseState_LC purchaseState;
    [HideInInspector] public WaitingForProductState_LC waitingForProductState;
    [HideInInspector] public LeavingState_LC leavingState;

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

        Gizmos.color = playerToFollow ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, checkRadius);

        if (currentState == followState)
        {
            Gizmos.DrawWireSphere(playerToFollow.transform.position, checkRadius);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!navMeshAgent) return;
        
        Gizmos.color = Color.black;

        NavMeshPath path = navMeshAgent.path;

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
    
    //////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////

    private void Awake()
    {
        wanderState = new WanderState_LC(this, true);
        followState = new FollowState_LC(this);
        getProductState = new GetProductState_LC(this);
        purchaseState = new PurchaseState_LC(this);
        waitingForProductState = new WaitingForProductState_LC(this);
        leavingState = new LeavingState_LC(this);
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
        // Begin with default state
        currentState = wanderState;
        
        // Setup start task
        currentTask = Tasks_AI.Wander;
    }

    public ILostCustomer_SM GetPreviousState()
    {
        return previousState as ILostCustomer_SM;
    }
}
