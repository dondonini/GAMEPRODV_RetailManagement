using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GetProductState_NC : GetProductState, INormalCustomer_SM
{
    public new NormalCustomer_AI stateMachine;

    float waitTimer = 0.0f;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_SM">Active State Machine</param>
    public GetProductState_NC(NormalCustomer_AI _SM)
    {
        base.stateMachine = _SM;
        stateMachine = _SM;
    }

    public override void StartState()
    {
        // Reset variables
        currentAction = GetProductActions.None;

        waitTimer = stateMachine.pickupDuration;
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
        }
    }
}
