using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForProductState_LC : WaitingForProductState, ILostCustomer_SM
{
    private readonly LostCustomer_AI _stateMachine;

    public WaitingForProductState_LC(LostCustomer_AI _SM)
    {
        stateMachine = _SM;
        _stateMachine = _SM;
    }
    
    #region Transition

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
        
    }

    public void ToLeaveStoreState()
    {
        _stateMachine.currentState = _stateMachine.leavingState;
    }

    public void ToPurchaseState()
    {
        _stateMachine.currentState = _stateMachine.purchaseState;
    }

    public void ToWaitForProductState()
    {
        
    }

    public void ToWaitForRegisterState()
    {
        
    }

    #endregion
}
