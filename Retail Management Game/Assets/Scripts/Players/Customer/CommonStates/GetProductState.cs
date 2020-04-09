using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetProductState : IBase_SM
{
    protected Customer_AI stateMachine = null;

    protected GetProductActions currentAction = GetProductActions.None;
    protected readonly float marginOfErrorAmount = 0.01f;
    protected Quaternion angularVelocity;
    protected ShelfContainer shelf;

    public enum GetProductActions
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
        }
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
