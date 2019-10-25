using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecideRegisterState : NormalCustomer_SM
{
    private readonly NormalCustomer_AI stateMachine;

    int maxTries = 5;

    bool decided = false;

    public DecideRegisterState(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        stateMachine.currentTask = Tasks_AI.GoToRegister;
        decided = false;
    }

    #region Transitions

    public void ToDecideProductState()
    {
        
    }

    public void ToDecideRegisterState()
    {

    }

    public void ToFacePosition()
    {
        
    }

    public void ToPickupState()
    {
        
    }

    public void ToPurchaseState()
    {
        
    }

    public void ToWalkToPositionState()
    {
        stateMachine.currentState = stateMachine.moveToPositionState;
    }

    public void ToQueuingState()
    {
        
    }

    #endregion

    public void UpdateState()
    {
        if (decided)
        {
            ToWalkToPositionState();
        }
        else
        {
            if (SetupCustomerQueue())
                decided = true;

            
        }
    }

    public void FixedUpdateState()
    {
        
    }

    bool SetupCustomerQueue(int tries = 0)
    {
        if (tries >= maxTries) return false;

        CashRegister foundRegister = stateMachine.mapManager.GetRandomCashRegister();

        if (foundRegister)
        {
            // Retry if the register queue is full
            if (foundRegister.IsFull()) SetupCustomerQueue(tries++);

            // Get queuing position
            Vector3 customerPosition = foundRegister.AddToQueue(stateMachine.gameObject);

            // Attach queue change event to QueueState
            foundRegister.QueueChanged.AddListener(stateMachine.queuingState.QueueChanged);

            stateMachine.taskDestination = foundRegister.transform;
            stateMachine.taskDestinationPosition = customerPosition;
        }

        return true;
    }
}
