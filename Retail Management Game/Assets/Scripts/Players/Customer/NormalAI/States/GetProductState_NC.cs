using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GetProductState_NC : GetProductState, INormalCustomer_SM
{
    private new readonly NormalCustomer_AI _stateMachine;

    private float _waitTimer = 0.0f;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_SM">Active State Machine</param>
    public GetProductState_NC(NormalCustomer_AI _SM)
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

    public void ToGetProductState()
    {
        Debug.LogWarning("Cannot transition to self!", _stateMachine);
    }

    public void ToLeaveStoreState()
    {
        
    }

    public void ToPurchaseState()
    {
        _stateMachine.currentState = _stateMachine.purchaseProductState;
    }

    public void ToWaitForProductState()
    {
        _stateMachine.currentState = _stateMachine.waitForProductState;
    }

    public void ToWaitForRegisterState()
    {

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
