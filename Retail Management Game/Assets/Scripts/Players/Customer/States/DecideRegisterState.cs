using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecideRegisterState : NormalCustomer_SM
{
    private readonly NormalCustomer_AI stateMachine;

    bool decided = false;

    public DecideRegisterState(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        stateMachine.currentTask = Tasks_AI.PuchaseProduct;
        decided = false;
    }

    #region Transitions

    public void ToDecideState()
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
        if (decided)
        {
            // Change state to walk
        }
        else
        {
            CashRegister foundRegister = 
        }
    }

    public void FixedUpdateState()
    {
        
    }

    CashRegister GetRegisterDestination()
    {

    }
}
