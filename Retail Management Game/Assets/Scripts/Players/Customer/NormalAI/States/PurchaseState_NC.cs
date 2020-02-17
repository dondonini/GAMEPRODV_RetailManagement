using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurchaseState_NC : NormalCustomer_SM
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

    bool isDonePurchasing = false;

    float patienceTimer = 0.0f;

    Vector3 targetRot;

    public PurchaseState_NC(NormalCustomer_AI _SM)
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

        isDonePurchasing = false;

        patienceTimer = stateMachine.queuingPatience;

        waitTime = stateMachine.purchaseDuration;

        stateMachine.patienceBillboard.gameObject.SetActive(true);
        stateMachine.patienceAnimator.SetBool("Show", false);
        stateMachine.patienceBillboard.gameObject.SetActive(false);
    }

    public void ExitState()
    {
        stateMachine.patienceAnimator.SetBool("Show", false);
        stateMachine.patienceBillboard.gameObject.SetActive(false);
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
        register.QueueChanged.RemoveListener(QueueChanged);
        register.CustomerCashedOut.RemoveListener(CashingOut);

        // Leave the cash register queue
        register.RemoveFromQueue(stateMachine.gameObject);

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

                // Wait in line (and get mad if players take too long)
            case PurchaseActions.Queuing:
                {
                    if (isDonePurchasing)
                    {
                        currentAction = PurchaseActions.Purchasing;
                    }
                    else if (patienceTimer <= 0.0f)
                    {
                        // Added this just so it won't give an error in the menu demo
                        if (stateMachine.gameManager)
                        {
                            stateMachine.gameManager.ForceLoseReasonMessage("CASH OUT CUSTOMERS, DUMMY");
                            stateMachine.gameManager.LostCustomer();
                        }

                        ToLeaveStoreState();
                    }

                    stateMachine.patienceBillboard.gameObject.SetActive(true);
                    stateMachine.patienceAnimator.SetBool("Show", true);
                    stateMachine.patienceText.color = Color.Lerp(Color.white, Color.red, 1.0f - (patienceTimer / stateMachine.queuingPatience));

                    patienceTimer -= Time.deltaTime;
                    break;
                }

                // Buy the product
            case PurchaseActions.Purchasing:
                {
                    if (isDonePurchasing)
                    {
                        // Buy the product in hand
                        if (stateMachine.IsHoldingItem())
                            PurchaseEquippedProduct();

                        // Wait a little before leaving the line
                        if (waitTime > 0.0f)
                            waitTime -= Time.deltaTime;
                        else
                        {
                            // Leave the store
                            ToLeaveStoreState();
                        }
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
            register.CustomerCashedOut.AddListener(CashingOut);

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

    void CashingOut()
    {
        if (Vector3.Distance(stateMachine.transform.position, register.transform.position)
            < 2.0f && register.GetCustomerQueueRank(stateMachine.gameObject) == 0)
            currentAction = PurchaseActions.Purchasing;

        if (currentAction == PurchaseActions.Purchasing)
            isDonePurchasing = true;
    }
}
