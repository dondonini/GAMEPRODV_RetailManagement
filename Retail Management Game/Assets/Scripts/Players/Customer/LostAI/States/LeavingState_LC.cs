using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeavingState_LC : LeavingState, ILostCustomer_SM
{
    private readonly LostCustomer_AI _stateMachine;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_SM">Active State Machine</param>
    public LeavingState_LC(LostCustomer_AI _SM)
    {
        stateMachine = _SM;
        _stateMachine = _SM;
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
        throw new System.NotImplementedException();
    }

    public void ToWaitForProductState()
    {
        throw new System.NotImplementedException();
    }

    public void ToWaitForRegisterState()
    {
        throw new System.NotImplementedException();
    }
    #endregion
}
