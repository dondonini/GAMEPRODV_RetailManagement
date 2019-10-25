using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// In this state, the AI walks to the destination position, 
/// then transitions to different states depending on the current task
/// </summary>
public class MoveToPositionState : NormalCustomer_SM
{
    private readonly NormalCustomer_AI stateMachine;

    public MoveToPositionState(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        stateMachine.agent.SetDestination(stateMachine.taskDestinationPosition);
    }

    #region Transition

    public void ToDecideProductState()
    {
        
    }

    public void ToDecideRegisterState()
    {
        
    }

    public void ToFacePosition()
    {
        stateMachine.currentState = stateMachine.rotateToVectorState;
    }

    public void ToPickupState()
    {
        
    }

    public void ToPurchaseState()
    {

    }

    public void ToWalkToPositionState()
    {
        // Cannot transition to self!
    }

    public void ToQueuingState()
    {
        stateMachine.currentState = stateMachine.queuingState;
    }

    #endregion

    public void UpdateState()
    {
        if (stateMachine.agent.remainingDistance < 0.25f)
        {
            // Go to a state when done walking
            switch (stateMachine.currentTask)
            {
                // AI should be at the shelf
                case Tasks_AI.GetProduct:
                    {
                        // Pickup product on shelf
                        ToFacePosition();

                        break;
                    }

                case Tasks_AI.GoToRegister:
                    {
                        CashRegister register = stateMachine.TaskDestinationAsRegister();
                        if (register)
                        {
                            if (register.GetCustomerQueueRank(stateMachine.gameObject) > 0)
                            {
                                stateMachine.currentState = stateMachine.queuingState;
                            }
                            else
                            {
                                ToFacePosition();
                            }
                        }
                        else
                        {
                            ToFacePosition();
                        }
                        
                        break;
                    }

                // AI should be at the register
                case Tasks_AI.PuchaseProduct:
                    {
                        // Purchase items
                        ToFacePosition();

                        break;
                    }

                // AI should be leaving the store
                case Tasks_AI.LeaveStore:
                    {
                        // TODO: Leave state

                        break;
                    }
            }
        }
    }

    public void FixedUpdateState()
    {

    }
}
