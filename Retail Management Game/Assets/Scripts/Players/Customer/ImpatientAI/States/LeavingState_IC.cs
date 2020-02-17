using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeavingState_IC : ImpatientCustomer_SM
{
    enum LeavingActions
    {
        None,
        Moving,
        Bye,
    }

    private readonly ImpatientCustomer_AI stateMachine;

    LeavingActions currentAction = LeavingActions.None;

    Transform exitPoint = null;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="_SM">State machine</param>
    public LeavingState_IC(ImpatientCustomer_AI _SM)
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

        EjectEquippedItem();
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

                    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainMenu")
                        stateMachine.gameManager.RemoveCustomer(stateMachine.transform);

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

    bool EjectEquippedItem()
    {
        if (stateMachine.IsHoldingItem())
        {
            Rigidbody rb = stateMachine.UnequipItem().GetComponent<Rigidbody>();

            rb.AddForce(0.0f, 500.0f, 0.0f);

            return true;
        }
        return false;
    }
}