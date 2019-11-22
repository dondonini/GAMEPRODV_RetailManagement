using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurchaseState : NormalCustomer_SM
{
    enum PurchaseActions
    {
        None,
        Moving,
        Turning,
        Queuing,
        Purchasing,
    }

    readonly NormalCustomer_AI stateMachine;
    
    readonly float marginOfErrorAmount = 0.01f;
    PurchaseActions currentAction = PurchaseActions.None;
    Quaternion angularVelocity;
    CashRegister register;
    float waitTime = 0.0f;

    int previousQueueRank = 0;

    Vector3 targetRot;

    public PurchaseState(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        // Reset variables
        currentAction = PurchaseActions.None;
        stateMachine.taskDestination = null;
        stateMachine.taskDestinationPosition = Vector3.zero;
        register = null;

        waitTime = stateMachine.purchaseDuration;
    }

    public void ExitState()
    {

    }

    #region Transitions

    public void ToPurchaseState()
    {
        Debug.LogWarning("Cannot transition to self!", stateMachine);
    }

    public void ToGetProductState()
    {
        
    }

    public void ToLeaveStoreState()
    {
        stateMachine.currentState = stateMachine.leavingState;
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
        UpdateActions();
    }

    public void FixedUpdateState()
    {

    }

    void UpdateActions()
    {
        switch (currentAction)
        {
            case PurchaseActions.None:
                {
                    bool result = SetupDestination();

                    if (result)
                    {
                        stateMachine.agent.SetDestination(stateMachine.taskDestinationPosition);
                        currentAction = PurchaseActions.Moving;
                    }
                    break;
                }
            case PurchaseActions.Moving:
                {
                    if (stateMachine.agent.remainingDistance < stateMachine.maxPickupDistance)
                    {
                        currentAction = PurchaseActions.Turning;
                    }
                    break;
                }
            case PurchaseActions.Turning:
                {
                    // Calculate direction to target

                    Vector3 target;
                    int rank = register.GetCustomerQueueRank(stateMachine.gameObject);

                    if (rank != 0)
                    {
                        target = register.GetFrontOfLinePosition();
                    }
                    else
                    {
                        target = stateMachine.taskDestination.position;
                    }

                    targetRot = target - stateMachine.transform.position;
                    targetRot.y = 0.0f;
                    targetRot.Normalize();

                    // SmoothDamp towards to target rotation
                    stateMachine.transform.rotation =
                        QuaternionUtil.SmoothDamp(
                            stateMachine.transform.rotation,
                            Quaternion.LookRotation(targetRot),
                            ref angularVelocity,
                            stateMachine.rotationSpeed
                        );

                    // Debug visuals
                    Debug.DrawRay(stateMachine.transform.position, targetRot * 2.0f, Color.green);
                    Debug.DrawRay(stateMachine.transform.position, stateMachine.transform.forward * 2.0f, Color.red);

                    // Stop rotation if angle between target and forward vector is lower than margin of error
                    if (Vector3.Angle(stateMachine.transform.forward, targetRot) <= marginOfErrorAmount)
                    {
                        if (rank != 0)
                        {
                            previousQueueRank = rank;
                            currentAction = PurchaseActions.Queuing;
                        }
                        else
                        {
                            currentAction = PurchaseActions.Purchasing;
                        }
                    }
                    break;
                }
            case PurchaseActions.Queuing:
                {
                    //int rank = register.GetCustomerQueueRank(stateMachine.gameObject);

                    //if (rank != previousQueueRank)
                    //{
                    //    stateMachine.taskDestinationPosition = register.GetCustomerQueuePostion(stateMachine.gameObject);

                    //    stateMachine.agent.SetDestination(stateMachine.taskDestinationPosition);

                    //    currentAction = PurchaseActions.Moving;
                    //}

                    //previousQueueRank = rank;
                    break;
                }
            case PurchaseActions.Purchasing:
                {
                    // Buy the product in hand
                    if (stateMachine.IsHoldingItem())
                        PurchaseEquippedProduct();
                    
                    // Wait a little before leaving the line
                    if (waitTime > 0.0f)
                        waitTime -= Time.deltaTime;
                    else
                    {
                        register.QueueChanged.RemoveListener(QueueChanged);

                        // Leave the cash register queue
                        register.RemoveFromQueue(stateMachine.gameObject);

                        

                        // Leave the store
                        ToLeaveStoreState();
                    }

                    break;
                }
        }

        stateMachine.UpdateActionStatus(currentAction.ToString());
    }

    bool SetupDestination()
    {
        // Grab random shelf component on map with wanted product 
        register = stateMachine.mapManager.GetRandomCashRegister();

        if (register)
        {
            // Get shelf transform
            stateMachine.taskDestination = register.transform;

            // Get pickup position
            stateMachine.taskDestinationPosition = register.AddToQueue(stateMachine.gameObject);

            // Attach queue change event
            register.QueueChanged.AddListener(QueueChanged);

            return true;
        }

        return false;
    }

    void PurchaseEquippedProduct()
    {
        if (Vector3.Distance(register.transform.position, stateMachine.transform.position) <= 2.0f && stateMachine.IsHoldingItem())
        {
            // Purchase equipped item
            register.PurchaseProduct(stateMachine.equippedItem.transform);
            stateMachine.equippedItem = null;
        }
        else
        {
            Debug.Log("Customer " + stateMachine.gameObject + " is not going to a register or not holding a product.", stateMachine.gameObject);
        }
    }

    void QueueChanged()
    {
        stateMachine.taskDestinationPosition = register.GetCustomerQueuePostion(stateMachine.gameObject);

        stateMachine.agent.SetDestination(stateMachine.taskDestinationPosition);

        currentAction = PurchaseActions.Moving;

    }
}
