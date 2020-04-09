using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForProductState_NC : WaitingForProductState, INormalCustomer_SM
{
    public new NormalCustomer_AI stateMachine;

    public WaitingForProductState_NC(NormalCustomer_AI _SM)
    {
        base.stateMachine = _SM;
        stateMachine = _SM;
    }
    
    #region Transition

    public void ToGetProductState()
    {
        
    }

    public void ToLeaveStoreState()
    {
        stateMachine.currentState = stateMachine.leavingState;
    }

    public void ToPurchaseState()
    {
        stateMachine.currentState = stateMachine.purchaseProductState;
    }

    public void ToWaitForProductState()
    {
        
    }

    public void ToWaitForRegisterState()
    {
        
    }

    #endregion
}
