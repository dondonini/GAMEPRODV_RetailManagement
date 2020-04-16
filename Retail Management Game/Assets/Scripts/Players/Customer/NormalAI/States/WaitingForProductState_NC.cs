using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForProductState_NC : WaitingForProductState, INormalCustomer_SM
{
    private readonly NormalCustomer_AI _stateMachine;

    public WaitingForProductState_NC(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
        _stateMachine = _SM;
    }
    
    #region Transition

    public void ToGetProductState()
    {
        
    }

    public void ToLeaveStoreState()
    {
        _stateMachine.currentState = _stateMachine.leavingState;
    }

    public void ToPurchaseState()
    {
        _stateMachine.currentState = _stateMachine.purchaseProductState;
    }

    public void ToWaitForProductState()
    {
        
    }

    public void ToWaitForRegisterState()
    {
        
    }

    #endregion
}
