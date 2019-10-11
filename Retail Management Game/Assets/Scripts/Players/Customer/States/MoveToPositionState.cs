using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPositionState : NormalCustomer_SM
{
    private readonly NormalCustomer_AI stateMachine;

    public MoveToPositionState(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        if (!stateMachine.taskDestination)
        {
            stateMachine.taskDestination = stateMachine.mapManager.GetRandomShelvingUnit();
        }
            
        stateMachine.agent.SetDestination(stateMachine.taskDestination.transform.position);
        
        
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

    public void UpdateState()
    {
        
    }
}
