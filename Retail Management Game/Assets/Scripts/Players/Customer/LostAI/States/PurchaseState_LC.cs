using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurchaseState_LC : PurchaseState, ILostCustomer_SM
{
    private readonly LostCustomer_AI _stateMachine;
    private static readonly int show = Animator.StringToHash("Show");

    public PurchaseState_LC(LostCustomer_AI _SM)
    {
        base.stateMachine = _SM;
        _stateMachine = _SM;
    }
    
    #region Transitions

    public void ToPurchaseState()
    {
        Debug.LogWarning("Cannot transition to self!", _stateMachine);
    }

    public void ToWanderState()
    {
        throw new System.NotImplementedException();
    }

    public void ToFollowState()
    {
        throw new System.NotImplementedException();
    }

    public void ToGetProductState()
    {
        
    }

    public void ToLeaveStoreState()
    {
        register.QueueChanged.RemoveListener(QueueChanged);
        register.CustomerCashedOut.RemoveListener(CashingOut);

        // Leave the cash register queue
        register.RemoveFromQueue(_stateMachine.gameObject);

        _stateMachine.currentState =  _stateMachine.leavingState;
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
                        if (_stateMachine.gameManager)
                        {
                            _stateMachine.gameManager.ForceLoseReasonMessage("CASH OUT CUSTOMERS, DUMMY");
                            _stateMachine.gameManager.LostCustomer();
                        }

                        ToLeaveStoreState();
                    }
                    
                    // Show billboard with patience animator
                    _stateMachine.billboardAnimator.SetBool(show, true);
                    _stateMachine.patienceAnimator.SetBool(show, true);
                    
                    // Hide wanted image
                    _stateMachine.wantedThumbnail.gameObject.SetActive(false);

                    _stateMachine.patienceText.color = Color.Lerp(Color.white, Color.red, 1.0f - (patienceTimer / _stateMachine.queuingPatience));

                    patienceTimer -= Time.deltaTime;
                    break;
                }

            // Buy the product
            case PurchaseActions.Purchasing:
                {
                    if (isDonePurchasing)
                    {
                        // Buy the product in hand
                        if (_stateMachine.IsHoldingItem())
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

            case PurchaseActions.None:
                break;
            case PurchaseActions.Moving:
                break;
            case PurchaseActions.Turning:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
