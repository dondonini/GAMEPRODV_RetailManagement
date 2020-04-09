﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurchaseState_NC : PurchaseState, INormalCustomer_SM
{
    private new readonly NormalCustomer_AI stateMachine = null;

    public PurchaseState_NC(NormalCustomer_AI _SM)
    {
        base.stateMachine = _SM;
        stateMachine = _SM;
    }

    public override void UpdateActions()
    {
        base.UpdateActions();

        switch (currentAction)
        {
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

            default:
                break;
        }
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

        stateMachine.currentState =  stateMachine.leavingState;
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
}
