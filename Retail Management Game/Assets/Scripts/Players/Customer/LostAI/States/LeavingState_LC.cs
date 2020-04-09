using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeavingState_LC : LeavingState, ILostCustomer_SM
{
    private new readonly NormalCustomer_AI stateMachine;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_SM">Active State Machine</param>
    public LeavingState_LC(NormalCustomer_AI _SM)
    {
        base.stateMachine = _SM;
        stateMachine = _SM;
    }

    #region Transitions
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
