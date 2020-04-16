using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetProductState : IBase_SM
{
    protected Customer_AI stateMachine = null;

    protected GetProductActions currentAction = GetProductActions.None;
    private const float MARGIN_OF_ERROR_AMOUNT = 0.01f;
    private Quaternion _angularVelocity;
    protected ShelfContainer shelf;

    protected enum GetProductActions
    {
        None,
        Moving,
        Turning,
        Grabbing,
    }

    public virtual void ExitState()
    {

    }

    public virtual void FixedUpdateState()
    {

    }

    public virtual void InterruptState()
    {

    }

    public virtual void StartState()
    {

    }

    public virtual void UpdateState()
    {
        // Wait until MapManager is done loading
        if (!stateMachine.mapManager.isDoneLoading) return;

        UpdateActions();

        stateMachine.UpdateInternalDebug("CurrentState = " + currentAction.ToString());
    }

    public virtual void UpdateActions()
    {
        switch (currentAction)
        {
            // Beginning state
            case GetProductActions.None:

                bool result = SetupDestination();

                if (result)
                {
                    // Move customer to target
                    stateMachine.navMeshAgent.SetDestination(stateMachine.taskDestinationPosition);
                    currentAction = GetProductActions.Moving;
                }

                break;

            // Ai is moving to shelf
            case GetProductActions.Moving:
                if (stateMachine.navMeshAgent.remainingDistance < stateMachine.maxPickupDistance)
                {
                    currentAction = GetProductActions.Turning;
                }

                break;

            // AI is turning towards the shelf
            case GetProductActions.Turning:
                // Calculate the angle between target position and customer forward vector
                float fromToDelta = EssentialFunctions.RotateTowardsTargetSmoothDamp(stateMachine.transform,
                    stateMachine.taskDestination, ref _angularVelocity, stateMachine.rotationSpeed);

                // Stop rotation if angle between target and forward vector is lower than margin of error
                if (fromToDelta <= MARGIN_OF_ERROR_AMOUNT)
                {
                    //Debug.Log("Customer is now looking at the target!", stateMachine.gameObject);
                    currentAction = GetProductActions.Grabbing;
                }

                break;
            case GetProductActions.Grabbing:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool SetupDestination()
    {
        if (stateMachine.currentWantedProduct == StockTypes.None)
        {
            // Decide on a random product to get
            stateMachine.currentWantedProduct = EssentialFunctions.GetRandomFromArray(stateMachine.mapManager.GetStockTypesAvailable());
            
            // Grab random shelf component on map with wanted product 
            shelf = stateMachine.mapManager.GetRandomShelvingUnit(stateMachine.currentWantedProduct);
        }
        else if (stateMachine.taskDestination)
        {
            // Get shelf container if task destination exists already
            shelf = stateMachine.taskDestination.GetComponent<ShelfContainer>();
        }
        
        // Check if shelf exists
        if (!shelf) return false;
        
        // Get shelf transform
        stateMachine.taskDestination = shelf.transform;

        // Get pickup position
        stateMachine.taskDestinationPosition = shelf.GetPickupPositions().Length == 1
            ? shelf.GetPickupPositions()[0]
            : EssentialFunctions.GetRandomFromArray(shelf.GetPickupPositions());

        return true;

    }
}
