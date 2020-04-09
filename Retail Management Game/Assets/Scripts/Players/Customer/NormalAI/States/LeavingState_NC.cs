using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeavingState_NC : LeavingState, INormalCustomer_SM
{
    private new readonly NormalCustomer_AI stateMachine;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_SM">Active State Machine</param>
    public LeavingState_NC(NormalCustomer_AI _SM)
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
        Debug.LogWarning("Cannot transition to self!", stateMachine);
    }

    public void ToPurchaseState()
    {
        
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