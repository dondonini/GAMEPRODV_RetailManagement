using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class LostCustomer_AI : Customer_AI
{
    //************************************************************************
    // States

    //[HideInInspector] public GetProductState_NC getProductState;
    //[HideInInspector] public LeavingState_NC leavingState;
    //[HideInInspector] public PurchaseState_NC purchaseProductState;
    //[HideInInspector] public WaitingForProductState_NC waitForProductState;
    //[HideInInspector] public 

    //////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    public ILostCustomer_SM GetPreviousState()
    {
        return previousState as ILostCustomer_SM;
    }
}
