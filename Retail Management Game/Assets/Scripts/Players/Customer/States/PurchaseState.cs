using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurchaseState : NormalCustomer_SM
{
    readonly NormalCustomer_AI stateMachine;

    float waitTime = 0.0f;

    public PurchaseState(NormalCustomer_AI _SM)
    {
        stateMachine = _SM;
    }

    public void StartState()
    {
        stateMachine.currentTask = Tasks_AI.PuchaseProduct;
        waitTime = stateMachine.purchaseDuration;

        PurchaseEquippedProduct();
    }

    #region Transitions

    public void ToDecideProductState()
    {
        
    }

    public void ToDecideRegisterState()
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

    public void ToQueuingState()
    {
        
    }

    #endregion

    public void UpdateState()
    {
        if (waitTime > 0.0f)
        {
            waitTime -= Time.deltaTime;
        }
        else
        {
            CashRegister register = stateMachine.taskDestination.GetComponent<CashRegister>();

            if (register)
            {
                register.QueueChanged.RemoveListener(stateMachine.queuingState.QueueChanged);

                register.RemoveToQueue(stateMachine.gameObject);

                Object.Destroy(stateMachine.gameObject);
            }
            // TODO: Change state to leave store
        }
    }

    public void FixedUpdateState()
    {

    }

    void PurchaseEquippedProduct()
    {
        CashRegister register = stateMachine.TaskDestinationAsRegister();

        if (register && stateMachine.IsHoldingItem())
        {
            // Purchase equipped item
            register.PurchaseProduct(stateMachine.equippedItem.transform);
        }
        else
        {
            Debug.Log("Customer " + stateMachine.gameObject + " is not going to a register or not holding a product.", stateMachine.gameObject);
        }
    }
}
