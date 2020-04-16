using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WanderState : IBase_SM
{
    protected Customer_AI stateMachine;
    
    protected WanderActions currentAction = WanderActions.None;
    protected bool isFirstWaypoint = true;
    protected float waitTime = 0.0f;

    protected Transform[] allPlayers;

    protected enum WanderActions
    {
        None,
        DecideOnRandomPosition,
        Moving,
        Waiting,
        Helped,
    }
    
    public virtual void StartState()
    {
        if (!StoreFloor.GetInstance().transform)
        {
            Debug.LogError("Floor doesn't exist!", stateMachine.gameObject);
        }
        
        // Get all players
        allPlayers = stateMachine.gameManager.GetPlayers();
        
        // Reset values
        isFirstWaypoint = true;
    }

    public virtual void ExitState()
    {
        
    }

    public virtual void UpdateState()
    {
        // Wait until MapManager is done loading
        if (!stateMachine.mapManager.isDoneLoading) return;
        
        UpdateActions();

        stateMachine.UpdateInternalDebug("CurrentState = " + currentAction);
    }

    public virtual void FixedUpdateState()
    {
        
    }

    public virtual void InterruptState()
    {
        throw new System.NotImplementedException();
    }

    public virtual void UpdateActions()
    {
        switch (currentAction)
        {
            case WanderActions.None:

                // Decide on a product you want to buy
                stateMachine.currentWantedProduct =
                    EssentialFunctions.GetRandomFromArray(stateMachine.mapManager.GetStockTypesAvailable());
                
                // Begin state with deciding on a random position
                currentAction = WanderActions.DecideOnRandomPosition;
                
                break;
            case WanderActions.DecideOnRandomPosition:
                
                // Setup random position for AI
                Vector3 randomPosition = GetRandomPositionInStoreFloor();
                UpdateDestinationToPosition(randomPosition);
                
                // Move on to moving
                currentAction = WanderActions.Moving;
                
                break;
            case WanderActions.Moving:
                
                // Wait until AI is at position
                if (stateMachine.navMeshAgent.remainingDistance < stateMachine.maxPickupDistance)
                {
                    currentAction = WanderActions.Waiting;
                }
                
                break;
            case WanderActions.Waiting:
                
                // Just wait for a bit
                if (waitTime <= 0.0f)
                {
                    currentAction = WanderActions.DecideOnRandomPosition;
                    
                    // This should be the second waypoint now. Not the first anymore
                    isFirstWaypoint = false;
                }

                waitTime -= Time.deltaTime;
                
                break;
            case WanderActions.Helped:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private Vector3 GetRandomPositionInStoreFloor()
    {
        return EssentialFunctions.GetRandomPositionInTransform(StoreFloor.GetInstance().transform);
    }
    
    private void UpdateDestinationToPosition(Vector3 newPosition)
    {
        NavMesh.SamplePosition(newPosition, out NavMeshHit hit,
            Vector3.Distance(stateMachine.transform.position, newPosition), 1);

        stateMachine.taskDestinationPosition = hit.position;
        stateMachine.navMeshAgent.SetDestination(stateMachine.taskDestinationPosition);
    }
}
