using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabProductState : NormalCustomer_SM
{
    private readonly NormalCustomer_AI stateMachine;

    public GrabProductState(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        
    }

    #region Transitions

    public void ToDecideState()
    {
        
    }

    public void ToFacePosition()
    {
        
    }

    public void ToPickupState()
    {
        // Cannot transition to self!
    }

    public void ToPurchaseState()
    {
        
    }

    public void ToWalkToPositionState()
    {
        stateMachine.currentState = stateMachine.moveToPositionState;
    }

    #endregion

    public void UpdateState()
    {
        if (!stateMachine.equippedItem)
        {
            stateMachine.Interact();
        }
        else
        {
            stateMachine.currentTask = Tasks_AI.PuchaseProduct;
            ToWalkToPositionState();
        }
    }

    public void FixedUpdateState()
    {
        
    }
}
