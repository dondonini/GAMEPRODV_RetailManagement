using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GetProductState : NormalCustomer_SM
{
    enum GetProductActions
    {
        None,
        Moving,
        Turning,
        Grabbing,
    }

    readonly NormalCustomer_AI stateMachine;

    readonly float marginOfErrorAmount = 0.01f;
    GetProductActions currentAction = GetProductActions.None;
    Quaternion angularVelocity;
    ShelfContainer shelf;
    float waitTimer = 0.0f;

    Vector3 targetRot;

    public GetProductState(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        // Reset variables
        currentAction = GetProductActions.None;

        waitTimer = stateMachine.pickupDuration;
    }

    public void ExitState()
    {

    }

    #region Transitions

    public void ToGetProductState()
    {
        Debug.LogWarning("Cannot transition to self!", stateMachine);
    }

    public void ToLeaveStoreState()
    {
        
    }

    public void ToPurchaseState()
    {
        stateMachine.currentState = stateMachine.purchaseProductState;
    }

    public void ToWaitForProductState()
    {
        stateMachine.currentState = stateMachine.waitForProductState;
    }

    public void ToWaitForRegisterState()
    {

    }

    #endregion

    public void UpdateState()
    {
        // Wait until MapManager is done loading
        if (!stateMachine.mapManager.isDoneLoading) return;

        UpdateActions();
    }

    public void FixedUpdateState()
    {

    }

    void UpdateActions()
    {
        switch (currentAction)
        {
            // Beginning state
            case GetProductActions.None:
                {
                    bool result = SetupDestination();

                    if (result)
                    {
                        // Move customer to target
                        stateMachine.agent.SetDestination(stateMachine.taskDestinationPosition);
                        currentAction = GetProductActions.Moving;
                    }

                    break;
                }

            // Ai is moving to shelf
            case GetProductActions.Moving:
                {
                    if (stateMachine.agent.remainingDistance < stateMachine.maxPickupDistance)
                    {
                        //stateMachine.agent.ResetPath();
                        currentAction = GetProductActions.Turning;
                    }

                    break;
                }

            // AI is turning towards the shelf
            case GetProductActions.Turning:
                {
                    // Calculate the angle between target position and customer forward vector
                    float fromToDelta = EssentialFunctions.RotateTowardsTargetSmoothDamp(stateMachine.transform, stateMachine.taskDestination, ref angularVelocity, stateMachine.rotationSpeed);

                    // Stop rotation if angle between target and forward vector is lower than margin of error
                    if (fromToDelta <= marginOfErrorAmount)
                    {
                        //Debug.Log("Customer is now looking at the target!", stateMachine.gameObject);
                        currentAction = GetProductActions.Grabbing;
                    }

                    break;
                }

            // AI has picked up and moving to purchase state
            case GetProductActions.Grabbing:
                {
                    if (shelf.IsEmpty())
                    {
                        ToWaitForProductState();
                    }
                    else
                    {
                        if (!stateMachine.equippedItem)
                        {
                            // Pick up product
                            stateMachine.Interact();
                        }
                        
                        if (waitTimer > 0.0f)
                        {
                            waitTimer -= Time.deltaTime;
                        }
                        else
                        {
                            // Go to purchase state
                            ToPurchaseState();

                        }
                    }
                    
                    break;
                }
        }

        stateMachine.UpdateActionStatus(currentAction.ToString());
    }

    bool SetupDestination()
    {
        // Decide on a product wanted
        stateMachine.currentWantedProduct = EssentialFunctions.GetRandomFromArray(stateMachine.mapManager.GetStockTypesAvailable());

        // Grab random shelf component on map with wanted product 
        shelf = stateMachine.mapManager.GetRandomShelvingUnit(stateMachine.currentWantedProduct);

        if (shelf)
        {
            // Get shelf transform
            stateMachine.taskDestination = shelf.transform;

            // Get pickup position
            if (shelf.GetPickupPositions().Length == 1)
                stateMachine.taskDestinationPosition = shelf.GetPickupPositions()[0];
            else
                stateMachine.taskDestinationPosition = EssentialFunctions.GetRandomFromArray(shelf.GetPickupPositions());

            return true;
        }

        return false;
    }
}
