using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetProductState_LC : GetProductState, ILostCustomer_SM
{
    private new LostCustomer_AI stateMachine = null;

    public GetProductState_LC(LostCustomer_AI _SM)
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
