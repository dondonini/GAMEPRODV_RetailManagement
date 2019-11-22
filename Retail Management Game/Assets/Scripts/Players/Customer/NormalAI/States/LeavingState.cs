using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeavingState : NormalCustomer_SM
{
    enum LeavingActions
    {
        None,
        Moving,
        Bye,
    }

    private readonly NormalCustomer_AI stateMachine;

    LeavingActions currentAction = LeavingActions.None;

    Transform exitPoint = null;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="_SM">State machine</param>
    public LeavingState(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        // Reset Variables
        currentAction = LeavingActions.None;
        stateMachine.taskDestination = null;
        stateMachine.taskDestinationPosition = Vector3.zero;
        exitPoint = null;
    }

    public void ExitState()
    {

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

    public void UpdateState()
    {
        switch(currentAction)
        {
            case LeavingActions.None:
                {
                    bool result = SetupDestination();
                    
                    if (result)
                    {
                        stateMachine.agent.SetDestination(stateMachine.taskDestination.position);

                        currentAction = LeavingActions.Moving;
                    }

                    break;
                }
            case LeavingActions.Moving:
                {
                    if (stateMachine.agent.remainingDistance < 2.0f)
                    {
                        currentAction = LeavingActions.Bye;
                    }
                    break;
                }
            case LeavingActions.Bye:
                {
                    //stateMachine.agent.ResetPath();

                    if (stateMachine.IsStateMachineActive())

                    // Stop statemachine from ticking... it's okay sweet prince, things will be okay...
                    stateMachine.DisableStateMachine();

                    // Shhhhhhh... you're going to a better place now
                    Object.Destroy(stateMachine.gameObject, 1.0f);

                    break;
                }
        }

        stateMachine.UpdateActionStatus(currentAction.ToString());
    }

    public void FixedUpdateState()
    {
        
    }

    bool SetupDestination()
    {
        exitPoint = EssentialFunctions.GetRandomFromArray(stateMachine.mapManager.GetMapExitPoints());

        if (!exitPoint)
            return false;

        stateMachine.taskDestination = exitPoint;

        stateMachine.taskDestinationPosition = exitPoint.position;

        return true;
    }
}
