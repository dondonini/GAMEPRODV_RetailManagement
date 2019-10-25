using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueuingState : NormalCustomer_SM
{
    readonly NormalCustomer_AI stateMachine;

    CashRegister register;
    int queueRank = 0;

    public QueuingState(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        register = stateMachine.TaskDestinationAsRegister();
    }

    #region Transition

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

    }

    public void FixedUpdateState()
    {
        
    }

    public void QueueChanged()
    {
        if (stateMachine.currentState != this)
            return;

        // Get new queue position
        stateMachine.taskDestinationPosition = register.GetCustomerQueuePostion(stateMachine.gameObject);

        // Change state to walking
        ToWalkToPositionState();
    }
}
