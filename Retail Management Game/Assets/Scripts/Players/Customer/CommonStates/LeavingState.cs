﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeavingState : IBase_SM
{
    public Customer_AI stateMachine = null;

    LeavingActions currentAction = LeavingActions.None;
    Transform exitPoint = null;

    public enum LeavingActions
    {
        None,
        Moving,
        Bye,
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

    public virtual void UpdateState()
    {
        UpdateActions();

        stateMachine.UpdateInternalDebug("CurrentState = " + currentAction.ToString());
    }

    public virtual void UpdateActions()
    {
        switch (currentAction)
        {
            case LeavingActions.None:
                {
                    bool result = SetupDestination();

                    if (result)
                    {
                        stateMachine.navMeshAgent.SetDestination(stateMachine.taskDestination.position);

                        currentAction = LeavingActions.Moving;
                    }

                    break;
                }
            case LeavingActions.Moving:
                {
                    if (stateMachine.navMeshAgent.remainingDistance < 2.0f)
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
                    Object.DestroyImmediate(stateMachine.gameObject);

                    break;
                }
        }
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

    public virtual bool SetupDestination()
    {
        exitPoint = EssentialFunctions.GetRandomFromArray(stateMachine.mapManager.GetMapExitPoints());

        if (!exitPoint)
            return false;

        stateMachine.taskDestination = exitPoint;

        stateMachine.taskDestinationPosition = exitPoint.position;

        return true;
    }

    public virtual void ExitState()
    {
        
    }

    public virtual void FixedUpdateState()
    {
        
    }

    public virtual void InterruptState()
    {
        
    }
}
