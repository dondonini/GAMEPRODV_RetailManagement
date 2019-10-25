using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForProductState : NormalCustomer_SM
{
    private readonly NormalCustomer_AI stateMachine;

    public WaitForProductState(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        
    }

    #region Transition

    public void ToDecideProductState()
    {
        
    }

    public void ToFacePosition()
    {
        
    }

    public void ToPickupState()
    {
        
    }

    public void ToPurchaseState()
    {
        
    }

    public void ToWalkToPositionState()
    {
        
    }

    #endregion

    public void UpdateState()
    {
        
    }

    public void FixedUpdateState()
    {

    }

    public void ToDecideRegisterState()
    {
        throw new System.NotImplementedException();
    }

    public void ToQueuingState()
    {
        throw new System.NotImplementedException();
    }
}
