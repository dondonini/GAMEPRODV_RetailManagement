using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurchaseState : IBase_SM
{
    protected Customer_AI stateMachine = null;


    protected float patienceTimer = 0.0f;
    protected float waitTime;
    Vector3 targetRot;
    protected readonly float marginOfErrorAmount = 0.01f;
    protected PurchaseActions currentAction = PurchaseActions.None;
    protected Quaternion angularVelocity;
    protected CashRegister register;

    protected bool isDonePurchasing = false;

    public enum PurchaseActions
    {
        None,
        Moving,
        Turning,
        Queuing,
        Purchasing,
    }

    public virtual void StartState()
    {
        // Reset variables
        currentAction = PurchaseActions.None;
        stateMachine.taskDestination = null;
        stateMachine.taskDestinationPosition = Vector3.zero;
        register = null;

        isDonePurchasing = false;

        patienceTimer = stateMachine.queuingPatience;

        waitTime = stateMachine.purchaseDuration;

        stateMachine.patienceBillboard.gameObject.SetActive(true);
        stateMachine.patienceAnimator.SetBool("Show", false);
        stateMachine.patienceBillboard.gameObject.SetActive(false);
    }

    public virtual void ExitState()
    {
        stateMachine.patienceAnimator.SetBool("Show", false);
        stateMachine.patienceBillboard.gameObject.SetActive(false);
    }

    public virtual void UpdateState()
    {
        UpdateActions();

        stateMachine.UpdateInternalDebug("CurrentState = " + currentAction.ToString());
    }

    public virtual void FixedUpdateState()
    {
        
    }

    public virtual void InterruptState()
    {
        
    }

    public virtual void UpdateActions()
    {
        switch (currentAction)
        {

            // Setup goal for AI
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

            // Walking to destination
            case PurchaseActions.Moving:
                {
                    // Remove patience indicator
                    stateMachine.patienceBillboard.gameObject.SetActive(true);
                    stateMachine.patienceAnimator.SetBool("Show", false);
                    stateMachine.patienceBillboard.gameObject.SetActive(false);

                    // Change action state when close to the destination
                    if (stateMachine.agent.remainingDistance < stateMachine.maxPickupDistance)
                    {
                        currentAction = PurchaseActions.Turning;
                    }
                    break;
                }

            // Turn towards destination
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

                    float fromToDelta = EssentialFunctions.RotateTowardsTargetSmoothDamp(stateMachine.transform, target, ref angularVelocity, stateMachine.rotationSpeed);

                    // Stop rotation if angle between target and forward vector is lower than margin of error
                    if (fromToDelta <= marginOfErrorAmount)
                        currentAction = PurchaseActions.Queuing;

                    break;
                }
        }
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
            register.CustomerCashedOut.AddListener(CashingOut);

            return true;
        }
        return false;
    }

    protected void QueueChanged()
    {
        stateMachine.taskDestinationPosition = register.GetCustomerQueuePostion(stateMachine.gameObject);

        stateMachine.agent.SetDestination(stateMachine.taskDestinationPosition);

        currentAction = PurchaseActions.Moving;
    }

    protected void CashingOut()
    {
        if (Vector3.Distance(stateMachine.transform.position, register.transform.position)
            < 2.0f && register.GetCustomerQueueRank(stateMachine.gameObject) == 0)
            currentAction = PurchaseActions.Purchasing;

        if (currentAction == PurchaseActions.Purchasing)
            isDonePurchasing = true;
    }

    protected void PurchaseEquippedProduct()
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
}
