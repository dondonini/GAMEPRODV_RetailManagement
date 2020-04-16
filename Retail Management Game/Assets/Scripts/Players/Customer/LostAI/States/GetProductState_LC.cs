using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetProductState_LC : GetProductState, ILostCustomer_SM
{
    private LostCustomer_AI _stateMachine = null;

    private float _waitTimer = 0.0f;
    
    public GetProductState_LC(LostCustomer_AI _SM)
    {
        base.stateMachine = _SM;
        _stateMachine = _SM;
    }

    public override void StartState()
    {
        // Reset variables
        currentAction = GetProductActions.None;

        _waitTimer = _stateMachine.pickupDuration;
    }
    
    #region Transitions

    public void ToWanderState()
    {
        throw new System.NotImplementedException();
    }

    public void ToFollowState()
    {
        throw new System.NotImplementedException();
    }

    public void ToGetProductState()
    {
        throw new System.NotImplementedException();
    }

    public void ToLeaveStoreState()
    {
        throw new System.NotImplementedException();
    }

    public void ToPurchaseState()
    {
        _stateMachine.currentState = _stateMachine.purchaseState;
    }

    public void ToWaitForProductState()
    {
        _stateMachine.currentState = _stateMachine.waitingForProductState;
    }

    public void ToWaitForRegisterState()
    {
        throw new System.NotImplementedException();
    }
    #endregion

    public override void UpdateActions()
    {
        // Call base method
        base.UpdateActions();

        // AI has picked up and moving to purchase state
        if (currentAction == GetProductActions.Grabbing)
        {
            if (shelf.IsEmpty())
            {
                ToWaitForProductState();
            }
            else
            {
                if (!_stateMachine.equippedItem)
                {
                    // Pick up product
                    _stateMachine.Interact();
                }

                if (_waitTimer > 0.0f)
                {
                    _waitTimer -= Time.deltaTime;
                }
                else
                {
                    // Go to purchase state
                    ToPurchaseState();

                }
            }
        }
    }
}
